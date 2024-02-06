using System.IO.Abstractions;
using Kong.Portal.CLI.ApiClient;
using Kong.Portal.CLI.ApiClient.Models;
using Kong.Portal.CLI.Services.Metadata;

namespace Kong.Portal.CLI.Services;

internal class DumpService(KongApiClient apiClient, MetadataSerializer metadataSerializer, IFileSystem fileSystem, IConsoleOutput consoleOutput)
{
    public async Task Dump(string outputDirectory)
    {
        Cleanup(outputDirectory);

        consoleOutput.WriteLine($"Output Directory: {outputDirectory}");
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

        var metadata = new PortalMetadata(
            portal.Name,
            portal.CustomDomain,
            portal.CustomClientDomain,
            portal.IsPublic,
            portal.AutoApproveDevelopers,
            portal.AutoApproveApplications,
            portal.RbacEnabled,
            products.Select(p => context.ApiProductSyncIdGenerator.GetSyncId(p.Id))
        );

        var metadataFilename = Path.Combine(portalDirectory, "portal.json");
        await metadataSerializer.SerializeAsync(metadataFilename, metadata);
    }

    private async Task DumpApiProduct(DumpContext context, ApiProduct apiProduct)
    {
        var apiProductSyncId = context.ApiProductSyncIdGenerator.Generate(apiProduct.Id, apiProduct.Name);

        consoleOutput.WriteLine($"  * {apiProductSyncId}");

        var apiProductDirectory = context.GetApiProductDirectory(apiProductSyncId);
        fileSystem.Directory.EnsureDirectory(apiProductDirectory);

        var metadata = new ApiProductMetadata(apiProductSyncId, apiProduct.Name, apiProduct.Description, apiProduct.Labels);

        consoleOutput.WriteLine("    - Metadata");
        var metadataFilename = Path.Combine(apiProductDirectory, "api-product.json");
        await metadataSerializer.SerializeAsync(metadataFilename, metadata);

        await DumpApiProductVersions(context, apiProductSyncId, apiProduct);

        await DumpApiProductDocuments(context, apiProductSyncId, apiProduct);
    }

    private async Task DumpApiProductVersions(DumpContext context, string apiProductSyncId, ApiProduct apiProduct)
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

            await DumpApiProductVersion(context, apiProductSyncId, apiProduct, apiProductVersion);
        }
    }

    private async Task DumpApiProductVersion(DumpContext context, string apiProductSyncId, ApiProduct apiProduct, ApiProductVersion apiProductVersion)
    {
        var versionsDirectory = context.GetVersionDirectory(apiProductSyncId);
        var specification = await apiClient.GetApiProductSpecification(apiProduct.Id, apiProductVersion.Id);

        var apiProductVersionSyncId = context.ApiProductVersionSyncIdGenerator.Generate(apiProductVersion.Id, apiProductVersion.Name);

        var metadata = new ApiProductVersionMetadata(
            apiProductVersionSyncId,
            apiProductVersion.Name,
            apiProductVersion.PublishStatus,
            apiProductVersion.Deprecated,
            specification?.Name
        );

        var versionDirectory = Path.Combine(versionsDirectory, $"{apiProductVersion.Name}");
        fileSystem.Directory.EnsureDirectory(versionDirectory);

        var metadataFilename = Path.Combine(versionDirectory, "version.json");
        await metadataSerializer.SerializeAsync(metadataFilename, metadata);

        if (specification == null)
        {
            return;
        }

        var specificationFilename = Path.Combine(versionDirectory, specification.Name.TrimStart('/'));
        await fileSystem.File.WriteAllTextAsync(specificationFilename, specification.Content);
    }

    private async Task DumpApiProductDocuments(DumpContext context, string apiProductSyncId, ApiProduct apiProduct)
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

            await DumpApiProductDocument(context, apiProductSyncId, apiProduct, document.Id, document.Slug);
        }
    }

    private async Task DumpApiProductDocument(DumpContext context, string apiProductSyncId, ApiProduct apiProduct, string documentId, string fullSlug)
    {
        var documentsDirectory = context.GetDocumentsDirectory(apiProductSyncId);
        var apiProductDocument = await apiClient.GetApiProductDocumentBody(apiProduct.Id, documentId);

        var metadata = new ApiProductDocumentMetadata(apiProductDocument.Title, apiProductDocument.Slug, fullSlug, apiProductDocument.Status);
        var metadataFilename = Path.Combine(documentsDirectory, $"{fullSlug}.json");
        fileSystem.Directory.EnsureDirectory(Path.GetDirectoryName(metadataFilename)!);
        await metadataSerializer.SerializeAsync(metadataFilename, metadata);

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
        public string OutputDirectory { get; } = outputDirectory;
        public SyncIdGenerator ApiProductSyncIdGenerator { get; } = new();
        public SyncIdGenerator ApiProductVersionSyncIdGenerator { get; } = new();

        public string GetApiProductDirectory(string syncId)
        {
            return Path.Combine(OutputDirectory, "api-products", syncId);
        }

        public string GetVersionDirectory(string syncId)
        {
            return Path.Combine(GetApiProductDirectory(syncId), "versions");
        }

        public string GetDocumentsDirectory(string syncId)
        {
            return Path.Combine(GetApiProductDirectory(syncId), "documents");
        }
    }
}
