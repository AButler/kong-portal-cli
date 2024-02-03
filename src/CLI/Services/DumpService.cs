using System.IO.Abstractions;
using System.Text.Json;
using Flurl.Http;
using Kong.Portal.CLI.Config;
using Kong.Portal.CLI.Services.Models;
using Microsoft.Extensions.Options;

namespace Kong.Portal.CLI.Services;

public class DumpService(IFileSystem fileSystem, IOptions<KongOptions> kongOptions)
{
    private readonly JsonSerializerOptions _serializerOptions =
        new(JsonSerializerDefaults.Web) { WriteIndented = true };

    private readonly FlurlClient _flurlClient = new FlurlClient(kongOptions.Value.GetKongBaseUri())
        .WithHeader("User-Agent", "Kong Portal CLI")
        .WithOAuthBearerToken(kongOptions.Value.Token);

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
        if (!fileSystem.Directory.Exists(apiProductDirectory))
        {
            fileSystem.Directory.CreateDirectory(apiProductDirectory);
        }

        var metadata = new
        {
            Name = apiProduct.Name,
            Description = apiProduct.Description,
            Labels = apiProduct.Labels
        };

        Console.WriteLine($"    - api-product.json");
        var metadataFilename = Path.Combine(apiProductDirectory, "api-product.json");
        await using var file = fileSystem.File.Create(metadataFilename);
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
        var response = await _flurlClient
            .Request($"api-products/{apiProductId}/documents")
            .SetQueryParam("page[number]", pageNumber)
            .GetAsync();

        var documents = await response.GetJsonAsync<ApiProductDocumentsResponse>();
        return documents!;
    }

    private async Task<ApiProductsResponse> GetApiProducts(int pageNumber)
    {
        var response = await _flurlClient
            .Request($"api-products")
            .SetQueryParam("page[number]", pageNumber)
            .GetAsync();

        var apiProducts = await response.GetJsonAsync<ApiProductsResponse>();
        return apiProducts!;
    }

    private void Cleanup(string outputDirectory)
    {
        var apiProductsDirectory = fileSystem.DirectoryInfo.New(
            Path.Combine(outputDirectory, "api-products")
        );

        if (!apiProductsDirectory.Exists)
        {
            return;
        }

        apiProductsDirectory.Delete(true);
    }
}
