using System.IO.Abstractions;
using Kong.Portal.CLI.ApiClient;
using Kong.Portal.CLI.ApiClient.Models;

namespace Kong.Portal.CLI.Services;

internal class DumpService(
    KongApiClientFactory apiClientFactory,
    MetadataSerializer metadataSerializer,
    IFileSystem fileSystem,
    IConsoleOutput consoleOutput
)
{
    public async Task Dump(string outputDirectory, KongApiClientOptions apiClientOptions)
    {
        Cleanup(outputDirectory);

        consoleOutput.WriteLine($"Output Directory: {outputDirectory}");
        consoleOutput.WriteLine("Dumping...");

        var apiClient = apiClientFactory.CreateClient(apiClientOptions);
        var context = new DumpContext(apiClient, outputDirectory);

        consoleOutput.WriteLine("- Portals");
        var portals = await apiClient.DevPortals.GetAll();
        foreach (var portal in portals)
        {
            await DumpPortal(context, portal);
        }

        consoleOutput.WriteLine("- API Products");

        var apiProducts = await apiClient.ApiProducts.GetAll();
        foreach (var apiProduct in apiProducts)
        {
            await DumpApiProduct(context, apiProduct);
        }

        consoleOutput.WriteLine("Done!");
    }

    private async Task DumpApiProduct(DumpContext context, ApiProduct apiProduct)
    {
        var syncIdFromLabel = apiProduct.GetSyncIdFromLabel();
        string apiProductSyncId;

        if (syncIdFromLabel != null)
        {
            context.ApiProductSyncIdGenerator.StoreExistingSyncId(syncIdFromLabel);
            apiProductSyncId = syncIdFromLabel;
        }
        else
        {
            apiProductSyncId = context.ApiProductSyncIdGenerator.Generate(apiProduct.Name);
        }

        consoleOutput.WriteLine($"  - {apiProductSyncId}");

        var apiProductDirectory = context.GetApiProductDirectory(apiProductSyncId);
        fileSystem.Directory.EnsureDirectory(apiProductDirectory);

        var metadata = apiProduct.ToMetadata(apiProductSyncId, context.PortalIdMap);

        var metadataFilename = Path.Combine(apiProductDirectory, "api-product.json");
        await metadataSerializer.SerializeAsync(metadataFilename, metadata);

        await DumpApiProductVersions(context, apiProductSyncId, apiProduct);

        await DumpApiProductDocuments(context, apiProductSyncId, apiProduct);
    }

    private async Task DumpApiProductVersions(DumpContext context, string apiProductSyncId, ApiProduct apiProduct)
    {
        var versions = await context.ApiClient.ApiProductVersions.GetAll(apiProduct.Id);

        foreach (var apiProductVersion in versions)
        {
            consoleOutput.WriteLine($"    - Version: {apiProductVersion.Name}");

            await DumpApiProductVersion(context, apiProductSyncId, apiProduct, apiProductVersion);
        }
    }

    private async Task DumpApiProductVersion(DumpContext context, string apiProductSyncId, ApiProduct apiProduct, ApiProductVersion apiProductVersion)
    {
        var versionsDirectory = context.GetVersionDirectory(apiProductSyncId);
        var specification = await context.ApiClient.ApiProductVersions.GetSpecification(apiProduct.Id, apiProductVersion.Id);

        var apiProductVersionSyncId = context.ApiProductVersionSyncIdGenerator.Generate(apiProductVersion.Name);

        var metadata = apiProductVersion.ToMetadata(apiProductVersionSyncId, specification?.Name);

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
        var documents = await context.ApiClient.ApiProductDocuments.GetAll(apiProduct.Id);

        foreach (var document in documents)
        {
            consoleOutput.WriteLine($"    - Document: {document.Slug}");

            await DumpApiProductDocument(context, apiProductSyncId, apiProduct, document.Id, document.Slug);
        }
    }

    private async Task DumpApiProductDocument(DumpContext context, string apiProductSyncId, ApiProduct apiProduct, string documentId, string fullSlug)
    {
        var documentsDirectory = context.GetDocumentsDirectory(apiProductSyncId);
        var apiProductDocument = await context.ApiClient.ApiProductDocuments.GetBody(apiProduct.Id, documentId);

        var metadata = apiProductDocument.ToMetadata(fullSlug);
        var metadataFilename = Path.Combine(documentsDirectory, $"{fullSlug}.json");
        fileSystem.Directory.EnsureDirectory(Path.GetDirectoryName(metadataFilename)!);
        await metadataSerializer.SerializeAsync(metadataFilename, metadata);

        var contentFilename = Path.Combine(documentsDirectory, $"{fullSlug}.md");
        await fileSystem.File.WriteAllTextAsync(contentFilename, apiProductDocument.MarkdownContent);
    }

    private async Task DumpPortal(DumpContext context, DevPortal devPortal)
    {
        consoleOutput.WriteLine($"  - {devPortal.Name}");

        context.StorePortalId(devPortal.Id, devPortal.Name);

        var portalDirectory = context.GetPortalDirectory(devPortal.Name);
        fileSystem.Directory.EnsureDirectory(portalDirectory);

        var metadata = devPortal.ToMetadata();

        var metadataFilename = Path.Combine(portalDirectory, "portal.json");
        await metadataSerializer.SerializeAsync(metadataFilename, metadata);

        await DumpPortalAppearance(context, devPortal);

        await DumpPortalAuthSettings(context, devPortal);

        await DumpPortalTeams(context, devPortal);
    }

    private async Task DumpPortalTeams(DumpContext context, DevPortal devPortal)
    {
        var portalDirectory = context.GetPortalDirectory(devPortal.Name);
        var teams = await context.ApiClient.DevPortals.GetTeams(devPortal.Id);

        var teamsMetadata = teams.ToMetadata();

        var teamsMetadataFilename = Path.Combine(portalDirectory, "teams.json");
        await metadataSerializer.SerializeAsync(teamsMetadataFilename, teamsMetadata);
    }

    private async Task DumpPortalAuthSettings(DumpContext context, DevPortal devPortal)
    {
        var portalDirectory = context.GetPortalDirectory(devPortal.Name);
        var portalAuthSettings = await context.ApiClient.DevPortals.GetAuthSettings(devPortal.Id);

        var authSettingsMetadata = portalAuthSettings.ToMetadata();

        var authSettingsMetadataFilename = Path.Combine(portalDirectory, "authentication-settings.json");
        await metadataSerializer.SerializeAsync(authSettingsMetadataFilename, authSettingsMetadata);
    }

    private async Task DumpPortalAppearance(DumpContext context, DevPortal devPortal)
    {
        var portalDirectory = context.GetPortalDirectory(devPortal.Name);
        var portalAppearance = await context.ApiClient.DevPortals.GetAppearance(devPortal.Id);

        var appearanceMetadata = portalAppearance.ToMetadata();

        var appearanceMetadataFilename = Path.Combine(portalDirectory, "appearance.json");
        await metadataSerializer.SerializeAsync(appearanceMetadataFilename, appearanceMetadata);

        if (portalAppearance.Images?.Favicon != null)
        {
            var imageFilename = Path.Combine(portalDirectory, portalAppearance.Images.Favicon.Filename);
            await fileSystem.File.WriteDataUriImage(imageFilename, portalAppearance.Images.Favicon.Data);
        }

        if (portalAppearance.Images?.Logo != null)
        {
            var imageFilename = Path.Combine(portalDirectory, portalAppearance.Images.Logo.Filename);
            await fileSystem.File.WriteDataUriImage(imageFilename, portalAppearance.Images.Logo.Data);
        }

        if (portalAppearance.Images?.CatalogCover != null)
        {
            var imageFilename = Path.Combine(portalDirectory, portalAppearance.Images.CatalogCover.Filename);
            await fileSystem.File.WriteDataUriImage(imageFilename, portalAppearance.Images.CatalogCover.Data);
        }
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

    private class DumpContext(KongApiClient apiClient, string outputDirectory)
    {
        private readonly Dictionary<string, string> _portalIdMap = new();

        public KongApiClient ApiClient { get; } = apiClient;
        public string OutputDirectory { get; } = outputDirectory;
        public SyncIdGenerator ApiProductSyncIdGenerator { get; } = new();
        public SyncIdGenerator ApiProductVersionSyncIdGenerator { get; } = new();
        public IReadOnlyDictionary<string, string> PortalIdMap => _portalIdMap;

        public string GetApiProductDirectory(string syncId) => Path.Combine(OutputDirectory, "api-products", syncId);

        public string GetVersionDirectory(string syncId) => Path.Combine(GetApiProductDirectory(syncId), "versions");

        public string GetDocumentsDirectory(string syncId) => Path.Combine(GetApiProductDirectory(syncId), "documents");

        public string GetPortalDirectory(string portalName) => Path.Combine(OutputDirectory, "portals", portalName);

        public void StorePortalId(string portalId, string portalName)
        {
            _portalIdMap[portalId] = portalName;
        }
    }
}
