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

        var apiProducts = await apiClient.ApiProducts.GetAll();
        foreach (var apiProduct in apiProducts)
        {
            await DumpApiProduct(context, apiProduct);
        }

        consoleOutput.WriteLine("- Portals");
        var portals = await apiClient.Portals.GetAll();
        foreach (var portal in portals)
        {
            await DumpPortal(context, portal);
        }
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

        context.StoreApiProductId(apiProduct.Id, apiProductSyncId);

        consoleOutput.WriteLine($"  * {apiProductSyncId}");

        var apiProductDirectory = context.GetApiProductDirectory(apiProductSyncId);
        fileSystem.Directory.EnsureDirectory(apiProductDirectory);

        var metadata = apiProduct.ToMetadata(apiProductSyncId);

        consoleOutput.WriteLine("    - Metadata");
        var metadataFilename = Path.Combine(apiProductDirectory, "api-product.json");
        await metadataSerializer.SerializeAsync(metadataFilename, metadata);

        await DumpApiProductVersions(context, apiProductSyncId, apiProduct);

        await DumpApiProductDocuments(context, apiProductSyncId, apiProduct);
    }

    private async Task DumpApiProductVersions(DumpContext context, string apiProductSyncId, ApiProduct apiProduct)
    {
        consoleOutput.WriteLine("    - Versions");

        var versions = await apiClient.ApiProductVersions.GetAll(apiProduct.Id);

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
        var specification = await apiClient.ApiProductVersions.GetSpecification(apiProduct.Id, apiProductVersion.Id);

        var apiProductVersionSyncId = context.ApiProductVersionSyncIdGenerator.Generate(apiProductVersion.Name);

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
        var documents = await apiClient.ApiProductDocuments.GetAll(apiProduct.Id);

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
        var apiProductDocument = await apiClient.ApiProductDocuments.GetBody(apiProduct.Id, documentId);

        var metadata = new ApiProductDocumentMetadata(apiProductDocument.Title, apiProductDocument.Slug, fullSlug, apiProductDocument.Status);
        var metadataFilename = Path.Combine(documentsDirectory, $"{fullSlug}.json");
        fileSystem.Directory.EnsureDirectory(Path.GetDirectoryName(metadataFilename)!);
        await metadataSerializer.SerializeAsync(metadataFilename, metadata);

        var contentFilename = Path.Combine(documentsDirectory, $"{fullSlug}.md");
        await fileSystem.File.WriteAllTextAsync(contentFilename, apiProductDocument.MarkdownContent);
    }

    private async Task DumpPortal(DumpContext context, ApiClient.Models.Portal portal)
    {
        consoleOutput.WriteLine($"  * {portal.Name}");

        var portalDirectory = context.GetPortalDirectory(portal.Name);
        fileSystem.Directory.EnsureDirectory(portalDirectory);

        var products = await apiClient.Portals.GetPortalProducts(portal.Id);
        var portalAppearance = await apiClient.Portals.GetAppearance(portal.Id);

        var appearanceMetadata = new PortalAppearanceMetadata(
            portalAppearance.ThemeName,
            portalAppearance.UseCustomFonts,
            portalAppearance.CustomTheme == null
                ? null
                : new PortalCustomTheme(
                    new PortalCustomThemeColors(
                        new PortalCustomThemeColorsSection(
                            portalAppearance.CustomTheme.Colors.Section.Header.Value,
                            portalAppearance.CustomTheme.Colors.Section.Body.Value,
                            portalAppearance.CustomTheme.Colors.Section.Header.Value,
                            portalAppearance.CustomTheme.Colors.Section.Accent.Value,
                            portalAppearance.CustomTheme.Colors.Section.Tertiary.Value,
                            portalAppearance.CustomTheme.Colors.Section.Stroke.Value,
                            portalAppearance.CustomTheme.Colors.Section.Footer.Value
                        ),
                        new PortalCustomThemeColorsText(
                            portalAppearance.CustomTheme.Colors.Text.Header.Value,
                            portalAppearance.CustomTheme.Colors.Text.Hero.Value,
                            portalAppearance.CustomTheme.Colors.Text.Headings.Value,
                            portalAppearance.CustomTheme.Colors.Text.Primary.Value,
                            portalAppearance.CustomTheme.Colors.Text.Secondary.Value,
                            portalAppearance.CustomTheme.Colors.Text.Accent.Value,
                            portalAppearance.CustomTheme.Colors.Text.Link.Value,
                            portalAppearance.CustomTheme.Colors.Text.Footer.Value
                        ),
                        new PortalCustomThemeColorsButton(
                            portalAppearance.CustomTheme.Colors.Button.PrimaryFill.Value,
                            portalAppearance.CustomTheme.Colors.Button.PrimaryText.Value
                        )
                    )
                ),
            new PortalCustomFonts(portalAppearance.CustomFonts?.Base, portalAppearance.CustomFonts?.Code, portalAppearance.CustomFonts?.Headings),
            new PortalText(portalAppearance.Text?.Catalog.WelcomeMessage, portalAppearance.Text?.Catalog.PrimaryHeader),
            new PortalImages(
                portalAppearance.Images.Favicon?.Filename,
                portalAppearance.Images.Logo?.Filename,
                portalAppearance.Images.CatalogCover?.Filename
            )
        );

        var metadata = new PortalMetadata(
            portal.Name,
            portal.CustomDomain,
            portal.CustomClientDomain,
            portal.IsPublic,
            portal.AutoApproveDevelopers,
            portal.AutoApproveApplications,
            portal.RbacEnabled,
            products.Select(p => context.GetApiProductSyncId(p.Id))
        );

        var metadataFilename = Path.Combine(portalDirectory, "portal.json");
        await metadataSerializer.SerializeAsync(metadataFilename, metadata);

        var appearanceMetadataFilename = Path.Combine(portalDirectory, "appearance.json");
        await metadataSerializer.SerializeAsync(appearanceMetadataFilename, appearanceMetadata);

        if (portalAppearance.Images.Favicon != null)
        {
            var imageFilename = Path.Combine(portalDirectory, portalAppearance.Images.Favicon.Filename);
            await fileSystem.File.WriteDataUriImage(imageFilename, portalAppearance.Images.Favicon.Data);
        }

        if (portalAppearance.Images.Logo != null)
        {
            var imageFilename = Path.Combine(portalDirectory, portalAppearance.Images.Logo.Filename);
            await fileSystem.File.WriteDataUriImage(imageFilename, portalAppearance.Images.Logo.Data);
        }

        if (portalAppearance.Images.CatalogCover != null)
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

    private class DumpContext(string outputDirectory)
    {
        private readonly Dictionary<string, string> _apiProductIdMap = new();

        public string OutputDirectory { get; } = outputDirectory;
        public SyncIdGenerator ApiProductSyncIdGenerator { get; } = new();
        public SyncIdGenerator ApiProductVersionSyncIdGenerator { get; } = new();

        public string GetApiProductDirectory(string syncId) => Path.Combine(OutputDirectory, "api-products", syncId);

        public string GetVersionDirectory(string syncId) => Path.Combine(GetApiProductDirectory(syncId), "versions");

        public string GetDocumentsDirectory(string syncId) => Path.Combine(GetApiProductDirectory(syncId), "documents");

        public string GetPortalDirectory(string portalName) => Path.Combine(OutputDirectory, "portals", portalName);

        public void StoreApiProductId(string apiProductId, string syncId)
        {
            _apiProductIdMap[apiProductId] = syncId;
        }

        public string GetApiProductSyncId(string apiProductId)
        {
            return _apiProductIdMap[apiProductId];
        }
    }
}
