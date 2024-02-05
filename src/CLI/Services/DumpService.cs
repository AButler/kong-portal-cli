using System.IO.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization;
using Kong.Portal.CLI.ApiClient;
using Kong.Portal.CLI.ApiClient.Models;

namespace Kong.Portal.CLI.Services;

internal class DumpService(KongApiClient apiClient, IFileSystem fileSystem, IConsoleOutput consoleOutput)
{
    private readonly JsonSerializerOptions _serializerOptions =
        new(JsonSerializerDefaults.Web)
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never
        };

    public async Task Dump(string outputDirectory)
    {
        Cleanup(outputDirectory);

        consoleOutput.WriteLine("Dumping...");
        consoleOutput.WriteLine("- API Products");

        var context = new DumpContext(outputDirectory);

        var apiProducts = await apiClient.GetApiProducts();
        foreach (var apiProduct in apiProducts)
        {
            await DumpApiProduct(context, apiProduct);
        }

        consoleOutput.WriteLine("- Portals");
        var portals = await apiClient.GetPortals();
        foreach (var portal in portals)
        {
            await DumpPortal(context, portal);
        }
    }

    private async Task DumpPortal(DumpContext context, ApiClient.Models.Portal portal)
    {
        consoleOutput.WriteLine($"  * {portal.Name}");

        var portalDirectory = Path.Combine(context.OutputDirectory, "portals", portal.Name);
        fileSystem.Directory.EnsureDirectory(portalDirectory);

        var products = await apiClient.GetPortalProducts(portal.Id);

        var metadata = new
        {
            portal.Name,
            portal.CustomDomain,
            portal.CustomClientDomain,
            portal.IsPublic,
            portal.AutoApproveDevelopers,
            portal.AutoApproveApplications,
            portal.RbacEnabled,
            products = products.Select(p => p.Name)
        };

        var metadataFilename = Path.Combine(portalDirectory, "portal.json");
        await using var file = fileSystem.File.Create(metadataFilename);
        await JsonSerializer.SerializeAsync(file, metadata, _serializerOptions);
    }

    private async Task DumpApiProduct(DumpContext context, ApiProduct apiProduct)
    {
        consoleOutput.WriteLine($"  * {apiProduct.Name}");

        var apiProductDirectory = context.GetApiProductDirectory(apiProduct);
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

        await DumpApiProductVersions(context, apiProduct);

        await DumpApiProductDocuments(context, apiProduct);
    }

    private async Task DumpApiProductVersions(DumpContext context, ApiProduct apiProduct)
    {
        consoleOutput.WriteLine("    - Versions");

        var versions = await apiClient.GetApiProductVersions(apiProduct.Id);

        if (!versions.Any())
        {
            consoleOutput.WriteLine($"      - (none)");
            return;
        }

        foreach (var apiProductVersion in versions)
        {
            consoleOutput.WriteLine($"      - {apiProductVersion.Name}");

            await DumpApiProductVersion(context, apiProduct, apiProductVersion);
        }
    }

    private async Task DumpApiProductVersion(DumpContext context, ApiProduct apiProduct, ApiProductVersion apiProductVersion)
    {
        var versionsDirectory = context.GetVersionDirectory(apiProduct);
        var specification = await apiClient.GetApiProductSpecification(apiProduct.Id, apiProductVersion.Id);

        context.StoreProductVersionId(apiProduct.Name, apiProductVersion.Name, apiProductVersion.Id);

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

    private async Task DumpApiProductDocuments(DumpContext context, ApiProduct apiProduct)
    {
        consoleOutput.WriteLine("    - Documents");
        var documents = await apiClient.GetApiProductDocuments(apiProduct.Id);

        if (!documents.Any())
        {
            consoleOutput.WriteLine("      - (none)");
            return;
        }

        foreach (var document in documents)
        {
            consoleOutput.WriteLine($"      - {document.Slug}");

            await DumpApiProductDocument(context, apiProduct, document.Id, document.Slug);
        }
    }

    private async Task DumpApiProductDocument(DumpContext context, ApiProduct apiProduct, string documentId, string fullSlug)
    {
        var documentsDirectory = context.GetDocumentsDirectory(apiProduct);
        var apiProductDocument = await apiClient.GetApiProductDocumentBody(apiProduct.Id, documentId);

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

    private class DumpContext(string outputDirectory)
    {
        private readonly Dictionary<string, (string ApiProductName, string Version)> _productVersions = new();

        public string OutputDirectory { get; } = outputDirectory;

        public void StoreProductVersionId(string apiProductName, string version, string versionId)
        {
            _productVersions.Add(versionId, (apiProductName, version));
        }

        public (string ApiProductName, string Version) GetProductVersionById(string versionId)
        {
            return _productVersions[versionId];
        }

        public string GetApiProductDirectory(ApiProduct apiProduct)
        {
            return Path.Combine(OutputDirectory, "api-products", apiProduct.Name);
        }

        public string GetVersionDirectory(ApiProduct apiProduct)
        {
            return Path.Combine(GetApiProductDirectory(apiProduct), "versions");
        }

        public string GetDocumentsDirectory(ApiProduct apiProduct)
        {
            return Path.Combine(GetApiProductDirectory(apiProduct), "documents");
        }
    }
}
