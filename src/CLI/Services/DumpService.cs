using System.IO.Abstractions;
using System.Text.Json;
using Flurl.Http;
using Kong.Portal.CLI.Config;
using Kong.Portal.CLI.Services.Models;
using Microsoft.Extensions.Options;

namespace Kong.Portal.CLI.Services;

public class DumpService(IFileSystem fileSystem, IConsoleOutput consoleOutput, IOptions<KongOptions> kongOptions)
{
    private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    private readonly FlurlClient _flurlClient = new FlurlClient(kongOptions.Value.GetKongBaseUri())
        .WithHeader("User-Agent", "Kong Portal CLI")
        .WithOAuthBearerToken(kongOptions.Value.Token);

    public async Task Dump(string outputDirectory)
    {
        Cleanup(outputDirectory);

        ApiProductsResponse apiProducts;
        var pageNumber = 1;

        consoleOutput.WriteLine("Dumping...");
        consoleOutput.WriteLine("- API Products");
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
        consoleOutput.WriteLine($"  * {apiProduct.Name}");

        var apiProductDirectory = Path.Combine(outputDirectory, "api-products", apiProduct.Name);
        fileSystem.Directory.EnsureDirectory(apiProductDirectory);

        var metadata = new
        {
            Name = apiProduct.Name,
            Description = apiProduct.Description,
            Labels = apiProduct.Labels
        };

        consoleOutput.WriteLine("    - Metadata");
        var metadataFilename = Path.Combine(apiProductDirectory, "api-product.json");
        await using var file = fileSystem.File.Create(metadataFilename);
        await JsonSerializer.SerializeAsync(file, metadata, _serializerOptions);

        var versionsDirectory = Path.Combine(apiProductDirectory, "versions");
        await DumpApiProductVersions(versionsDirectory, apiProduct.Id);

        var documentsDirectory = Path.Combine(apiProductDirectory, "documents");
        await DumpApiProductDocuments(documentsDirectory, apiProduct.Id);
    }

    private async Task DumpApiProductVersions(string versionsDirectory, string apiProductId)
    {
        fileSystem.Directory.EnsureDirectory(versionsDirectory);

        ApiProductVersionsResponse apiProductVersions;
        var pageNumber = 1;
        var anyVersions = false;

        consoleOutput.WriteLine("    - Versions");
        do
        {
            apiProductVersions = await GetApiProductVersions(pageNumber++, apiProductId);

            foreach (var apiProductVersion in apiProductVersions.Data)
            {
                anyVersions = true;
                consoleOutput.WriteLine($"      - {apiProductVersion.Name}");

                await DumpApiProductVersion(versionsDirectory, apiProductId, apiProductVersion);
            }
        } while (apiProductVersions.Meta.Page.HasMore());

        if (!anyVersions)
        {
            consoleOutput.WriteLine($"      - (none)");
        }
    }

    private async Task DumpApiProductVersion(string versionsDirectory, string apiProductId, ApiProductVersion apiProductVersion)
    {
        var metadata = new
        {
            Name = apiProductVersion.Name,
            PublishStatus = apiProductVersion.PublishStatus,
            Deprecated = apiProductVersion.Deprecated
        };

        var metadataFilename = Path.Combine(versionsDirectory, $"{apiProductVersion.Name}.json");
        await using var file = fileSystem.File.Create(metadataFilename);
        await JsonSerializer.SerializeAsync(file, metadata, _serializerOptions);

        //TODO: Dump swagger
    }

    private async Task DumpApiProductDocuments(string documentsDirectory, string apiProductId)
    {
        fileSystem.Directory.EnsureDirectory(documentsDirectory);

        ApiProductDocumentsResponse apiProductDocuments;
        var pageNumber = 1;

        consoleOutput.WriteLine("    - Documents");
        var anyDocuments = false;

        do
        {
            apiProductDocuments = await GetApiProductDocuments(pageNumber++, apiProductId);

            foreach (var apiProductDocument in apiProductDocuments.Data)
            {
                anyDocuments = true;
                consoleOutput.WriteLine($"      - {apiProductDocument.Slug}");

                await DumpApiProductDocument(documentsDirectory, apiProductId, apiProductDocument.Id, apiProductDocument.Slug);
            }
        } while (apiProductDocuments.Meta.Page.HasMore());

        if (!anyDocuments)
        {
            consoleOutput.WriteLine($"      - (none)");
        }
    }

    private async Task DumpApiProductDocument(string documentsDirectory, string apiProductId, string apiProductDocumentId, string fullSlug)
    {
        var apiProductDocument = await GetApiProductDocument(apiProductId, apiProductDocumentId);

        var metadata = new
        {
            Title = apiProductDocument.Title,
            Slug = apiProductDocument.Slug,
            Status = apiProductDocument.Status
        };
        var metadataFilename = Path.Combine(documentsDirectory, $"{fullSlug}.json");
        fileSystem.Directory.EnsureDirectory(Path.GetDirectoryName(metadataFilename)!);
        await using var file = fileSystem.File.Create(metadataFilename);
        await JsonSerializer.SerializeAsync(file, metadata, _serializerOptions);

        var contentFilename = Path.Combine(documentsDirectory, $"{fullSlug}.md");
        await fileSystem.File.WriteAllTextAsync(contentFilename, apiProductDocument.MarkdownContent);
    }

    private async Task<ApiProductVersionsResponse> GetApiProductVersions(int pageNumber, string apiProductId)
    {
        var response = await _flurlClient
            .Request($"api-products/{apiProductId}/product-versions")
            .SetQueryParam("page[number]", pageNumber)
            .GetAsync();

        var versions = await response.GetJsonAsync<ApiProductVersionsResponse>();
        return versions!;
    }

    private async Task<ApiProductDocumentBodyResponse> GetApiProductDocument(string apiProductId, string apiProductDocumentId)
    {
        var response = await _flurlClient.Request($"api-products/{apiProductId}/documents/{apiProductDocumentId}").GetAsync();

        var document = await response.GetJsonAsync<ApiProductDocumentBodyResponse>();
        return document!;
    }

    private async Task<ApiProductDocumentsResponse> GetApiProductDocuments(int pageNumber, string apiProductId)
    {
        var response = await _flurlClient.Request($"api-products/{apiProductId}/documents").SetQueryParam("page[number]", pageNumber).GetAsync();

        var documents = await response.GetJsonAsync<ApiProductDocumentsResponse>();
        return documents!;
    }

    private async Task<ApiProductsResponse> GetApiProducts(int pageNumber)
    {
        var response = await _flurlClient.Request($"api-products").SetQueryParam("page[number]", pageNumber).GetAsync();

        var apiProducts = await response.GetJsonAsync<ApiProductsResponse>();
        return apiProducts!;
    }

    private void Cleanup(string outputDirectory)
    {
        var apiProductsDirectory = fileSystem.DirectoryInfo.New(Path.Combine(outputDirectory, "api-products"));

        if (!apiProductsDirectory.Exists)
        {
            return;
        }

        apiProductsDirectory.Delete(true);
    }
}
