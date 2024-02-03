using System.IO.Abstractions;
using System.Text.Json;
using Kong.Portal.CLI.ApiClient;
using Kong.Portal.CLI.ApiClient.Models;

namespace Kong.Portal.CLI.Services;

internal class DumpService(KongApiClient apiClient, IFileSystem fileSystem, IConsoleOutput consoleOutput)
{
    private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public async Task Dump(string outputDirectory)
    {
        Cleanup(outputDirectory);

        consoleOutput.WriteLine("Dumping...");
        consoleOutput.WriteLine("- API Products");

        var apiProducts = await apiClient.GetApiProducts();
        foreach (var apiProduct in apiProducts)
        {
            await DumpApiProduct(outputDirectory, apiProduct);
        }
    }

    private async Task DumpApiProduct(string outputDirectory, ApiProduct apiProduct)
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

        consoleOutput.WriteLine("    - Versions");

        var versions = await apiClient.GetApiProductVersions(apiProductId);

        if (!versions.Any())
        {
            consoleOutput.WriteLine($"      - (none)");
            return;
        }

        foreach (var apiProductVersion in versions)
        {
            consoleOutput.WriteLine($"      - {apiProductVersion.Name}");

            await DumpApiProductVersion(versionsDirectory, apiProductId, apiProductVersion);
        }
    }

    private async Task DumpApiProductVersion(string versionsDirectory, string apiProductId, ApiProductVersion apiProductVersion)
    {
        var specification = await apiClient.GetApiProductSpecification(apiProductId, apiProductVersion.Id);

        var metadata = new
        {
            Name = apiProductVersion.Name,
            PublishStatus = apiProductVersion.PublishStatus,
            Deprecated = apiProductVersion.Deprecated,
            SpecificationFilename = specification?.Name
        };

        var versionDirectory = Path.Combine(versionsDirectory, $"{apiProductVersion.Name}");
        fileSystem.Directory.EnsureDirectory(versionDirectory);

        var metadataFilename = Path.Combine(versionDirectory, "version.json");
        await using var file = fileSystem.File.Create(metadataFilename);
        await JsonSerializer.SerializeAsync(file, metadata, _serializerOptions);

        if (specification == null)
        {
            return;
        }

        var specificationFilename = Path.Combine(versionDirectory, specification.Name.TrimStart('/'));
        await fileSystem.File.WriteAllTextAsync(specificationFilename, specification.Content);
    }

    private async Task DumpApiProductDocuments(string documentsDirectory, string apiProductId)
    {
        fileSystem.Directory.EnsureDirectory(documentsDirectory);

        consoleOutput.WriteLine("    - Documents");
        var documents = await apiClient.GetApiProductDocuments(apiProductId);

        if (!documents.Any())
        {
            consoleOutput.WriteLine("      - (none)");
            return;
        }

        foreach (var document in documents)
        {
            consoleOutput.WriteLine($"      - {document.Slug}");

            await DumpApiProductDocument(documentsDirectory, apiProductId, document.Id, document.Slug);
        }
    }

    private async Task DumpApiProductDocument(string documentsDirectory, string apiProductId, string documentId, string fullSlug)
    {
        var apiProductDocument = await apiClient.GetApiProductDocumentBody(apiProductId, documentId);

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
