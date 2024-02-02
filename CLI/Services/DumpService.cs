using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Kong.Portal.CLI.Config;
using Kong.Portal.CLI.Helpers;
using Microsoft.Extensions.Options;

namespace Kong.Portal.CLI.Services;

public class DumpService
{
    private readonly KongOptions _kongOptions;
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly HttpClient _httpClient;

    public DumpService(IOptions<KongOptions> kongOptions)
    {
        _kongOptions = kongOptions.Value;
        _serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = true
        };

        _httpClient = new HttpClient { BaseAddress = _kongOptions.GetKongBaseUri() };
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _kongOptions.Token
        );
    }

    public async Task Dump(string outputDirectory)
    {
        Cleanup(outputDirectory);

        ApiProductsResponse apiProducts;
        var pageNumber = 1;

        Console.WriteLine("Dumping...");

        do
        {
            apiProducts = await GetApiProducts(pageNumber++);

            foreach (var apiProduct in apiProducts.Data)
            {
                await DumpApiProduct(outputDirectory, apiProduct);
            }
        } while (apiProducts.Meta.Page.HasMore());
    }

    private async Task DumpApiProduct(string outputDirectory, ApiProductResponse apiProduct)
    {
        Console.WriteLine($"  - {apiProduct.Name}");

        var apiProductDirectory = Path.Combine(outputDirectory, "api-products", apiProduct.Name);
        DirectoryHelpers.EnsureDirectory(apiProductDirectory);

        var metadata = new
        {
            Id = apiProduct.Id,
            Description = apiProduct.Description,
            Labels = apiProduct.Labels
        };

        Console.WriteLine($"    - api-product.json");
        var metadataFilename = Path.Combine(apiProductDirectory, "api-product.json");
        await using var file = File.Create(metadataFilename);
        await JsonSerializer.SerializeAsync(file, metadata, _serializerOptions);

        ApiProductDocumentsResponse apiProductDocuments;
        var pageNumber = 1;

        do
        {
            apiProductDocuments = await GetApiProductDocuments(pageNumber++, apiProduct.Id);

            foreach (var apiProductDocument in apiProductDocuments.Data)
            {
                Console.WriteLine($"    - {apiProductDocument.Slug}");
            }
        } while (apiProductDocuments.Meta.Page.HasMore());
    }

    private async Task<ApiProductDocumentsResponse> GetApiProductDocuments(
        int pageNumber,
        string apiProductId
    )
    {
        var response = await _httpClient.GetAsync(
            $"api-products/{apiProductId}/documents?page%5Bnumber%5D={pageNumber}"
        );
        response.EnsureSuccessStatusCode();

        var documents = await response.Content.ReadFromJsonAsync<ApiProductDocumentsResponse>();
        return documents!;
    }

    private async Task<ApiProductsResponse> GetApiProducts(int pageNumber)
    {
        var response = await _httpClient.GetAsync($"api-products?page%5Bnumber%5D={pageNumber}");
        response.EnsureSuccessStatusCode();

        var apiProducts = await response.Content.ReadFromJsonAsync<ApiProductsResponse>();
        return apiProducts!;
    }

    private static void Cleanup(string outputDirectory)
    {
        var apiProductsDirectory = new DirectoryInfo(Path.Combine(outputDirectory, "api-products"));

        if (!apiProductsDirectory.Exists)
        {
            return;
        }

        apiProductsDirectory.Delete(true);
    }
}

internal record ApiProductDocumentsResponse(
    List<ApiProductDocumentResponse> Data,
    ApiMetadata Meta
);

internal record ApiProductDocumentResponse(
    string Id,
    [property: JsonPropertyName("parent_document_id")] string ParentDocumentId,
    string Slug,
    string Status,
    string Title
);

public record ApiProductsResponse(List<ApiProductResponse> Data, ApiMetadata Meta);

public record ApiMetadata(PageMetadata Page);

public record PageMetadata(int Total, int Size, int Number)
{
    public bool HasMore() => Number * Size > Total;
}

public record ApiProductResponse(
    Dictionary<string, string> Labels,
    string Id,
    string Name,
    string Description,
    [property: JsonPropertyName("portal_ids")] List<string> PortalIds,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("updated_at")] DateTimeOffset UpdatedAt,
    [property: JsonPropertyName("version_count")] int VersionCount
);
