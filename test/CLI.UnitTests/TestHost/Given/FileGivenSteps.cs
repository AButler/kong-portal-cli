using System.IO.Abstractions;
using Kong.Portal.CLI;
using Kong.Portal.CLI.Services;

namespace CLI.UnitTests.TestHost;

internal class FileGivenSteps(IFileSystem fileSystem, MetadataSerializer metadataSerializer)
{
    private readonly SyncIdGenerator _apiProductSyncIdGenerator = new();
    private readonly SyncIdGenerator _apiProductVersionSyncIdGenerator = new();

    public async Task AnExistingApiProduct(
        string inputDirectory,
        Discretionary<string> name = default,
        Discretionary<string> syncId = default,
        Discretionary<string> description = default,
        Discretionary<IReadOnlyCollection<string>> portals = default,
        IDictionary<string, string>? labels = null
    )
    {
        var productName = name.GetValueOrDefault(Guid.NewGuid().ToString());
        var productSyncId = syncId.GetValueOrDefault(_apiProductSyncIdGenerator.Generate(productName));

        var metadata = new ApiProductMetadata(
            productSyncId,
            productName,
            description.GetValueOrDefault(Guid.NewGuid().ToString()),
            portals.GetValueOrDefault([]),
            labels.ToLabelDictionary()
        );

        var apiProductDirectory = Path.Combine(inputDirectory, "api-products", productSyncId);
        fileSystem.Directory.EnsureDirectory(apiProductDirectory);

        var metadataFilename = Path.Combine(apiProductDirectory, "api-product.json");
        await metadataSerializer.SerializeAsync(metadataFilename, metadata);
    }

    public async Task AnExistingApiProductVersion(
        string inputDirectory,
        string apiProductSyncId,
        Discretionary<string> syncId = default,
        Discretionary<string> name = default,
        Discretionary<string> publishStatus = default,
        Discretionary<bool> deprecated = default,
        Discretionary<string?> specificationFilename = default
    )
    {
        var versionName = name.GetValueOrDefault("VERSION");
        var versionSyncId = syncId.GetValueOrDefault(_apiProductVersionSyncIdGenerator.Generate(versionName));

        var metadata = new ApiProductVersionMetadata(
            versionSyncId,
            versionName,
            publishStatus.GetValueOrDefault("published") == "published" ? MetadataPublishStatus.Published : MetadataPublishStatus.Unpublished,
            deprecated.GetValueOrDefault(false),
            specificationFilename.GetValueOrDefault(null)
        );

        var apiProductVersionDirectory = Path.Combine(inputDirectory, "api-products", apiProductSyncId, "versions", versionSyncId);
        fileSystem.Directory.EnsureDirectory(apiProductVersionDirectory);

        var metadataFilename = Path.Combine(apiProductVersionDirectory, "version.json");
        await metadataSerializer.SerializeAsync(metadataFilename, metadata);
    }

    public async Task AnExistingApiProductDocument(
        string inputDirectory,
        string apiProductSyncId,
        Discretionary<string> title = default,
        Discretionary<string> slug = default,
        Discretionary<string> fullSlug = default,
        Discretionary<string> status = default,
        Discretionary<string> content = default
    )
    {
        var metadata = new ApiProductDocumentMetadata(
            title.GetValueOrDefault("Document Title"),
            slug.GetValueOrDefault("/doc"),
            fullSlug.GetValueOrDefault("/doc"),
            status.GetValueOrDefault("published") == "published" ? MetadataPublishStatus.Published : MetadataPublishStatus.Unpublished
        );

        var markdownContent = content.GetValueOrDefault("# Document Title\n\nThis is a test document");

        var documentsDirectory = Path.Combine(inputDirectory, "api-products", apiProductSyncId, "documents");
        fileSystem.Directory.EnsureDirectory(documentsDirectory);

        var metadataFilename = Path.Combine(documentsDirectory, $"{fullSlug}.json");
        await metadataSerializer.SerializeAsync(metadataFilename, metadata);

        var contentFilename = Path.Combine(documentsDirectory, $"{fullSlug}.md");
        await fileSystem.File.WriteAllTextAsync(contentFilename, markdownContent);
    }

    public async Task AnExistingDevPortal(
        string inputDirectory,
        Discretionary<string> name = default,
        Discretionary<string?> customDomain = default,
        Discretionary<string?> customClientDomain = default,
        Discretionary<bool> isPublic = default,
        Discretionary<bool> rbacEnabled = default,
        Discretionary<bool> autoApproveDevelopers = default,
        Discretionary<bool> autoApproveApplications = default
    )
    {
        var portalName = name.GetValueOrDefault("default");

        var metadata = new PortalMetadata(
            portalName,
            customDomain.GetValueOrDefault(null),
            customClientDomain.GetValueOrDefault(null),
            isPublic.GetValueOrDefault(false),
            autoApproveDevelopers.GetValueOrDefault(false),
            autoApproveApplications.GetValueOrDefault(false),
            rbacEnabled.GetValueOrDefault(false)
        );

        var portalDirectory = Path.Combine(inputDirectory, "portals", portalName);
        fileSystem.Directory.EnsureDirectory(portalDirectory);

        var metadataFilename = Path.Combine(portalDirectory, "portal.json");
        await metadataSerializer.SerializeAsync(metadataFilename, metadata);
    }

    public async Task AnExistingDevPortalAppearance(
        string inputDirectory,
        Discretionary<string> name = default,
        Discretionary<string> themeName = default
    )
    {
        var portalName = name.GetValueOrDefault("default");

        var metadata = new PortalAppearanceMetadata(
            themeName.GetValueOrDefault("mint_rocket"),
            false,
            CreateDefaultCustomTheme(),
            new PortalCustomFontsMetadata("Roboto", "Roboto Mono", "Lato"),
            new PortalTextMetadata("Welcome", "Header"),
            PortalImagesMetadata.NullValue
        );

        var portalDirectory = Path.Combine(inputDirectory, "portals", portalName);
        fileSystem.Directory.EnsureDirectory(portalDirectory);

        var metadataFilename = Path.Combine(portalDirectory, "appearance.json");
        await metadataSerializer.SerializeAsync(metadataFilename, metadata);
    }

    private static PortalCustomThemeMetadata CreateDefaultCustomTheme()
    {
        return new PortalCustomThemeMetadata(
            new PortalCustomThemeColorsMetadata(
                new PortalCustomThemeColorsSectionMetadata("#F8F8F8", "#FFFFFF", "#F8F8F8", "#F8F8F8", "#FFFFFF", "rgba(0,0,0,0.1)", "#07A88D"),
                new PortalCustomThemeColorsTextMetadata(
                    "rgba(0,0,0,0.8)",
                    "#FFFFFF",
                    "rgba(0,0,0,0.8)",
                    "rgba(0,0,0,0.8)",
                    "rgba(0,0,0,0.8)",
                    "#07A88D",
                    "#07A88D",
                    "#FFFFFF"
                ),
                new PortalCustomThemeColorsButtonMetadata("#1155CB", "#FFFFFF")
            )
        );
    }

    public async Task AnExistingDevPortalAuthSettings(
        string inputDirectory,
        Discretionary<string> name = default,
        Discretionary<bool> basicAuthEnabled = default,
        Discretionary<bool> oidcAuthEnabled = default,
        Discretionary<bool> oidcTeamMappingEnabled = default,
        Discretionary<bool> konnectMappingEnabled = default,
        Discretionary<OidcAuthSettings?> oidcConfig = default
    )
    {
        var portalName = name.GetValueOrDefault("default");

        var oidcConfigValue = oidcConfig.GetValueOrDefault(null);
        var oidcConfigMetadata =
            oidcConfigValue == null
                ? null
                : new PortalOidcConfig(
                    oidcConfigValue.Issuer,
                    oidcConfigValue.ClientId,
                    oidcConfigValue.ClientSecret,
                    oidcConfigValue.Scopes,
                    new PortalClaimMappings(
                        oidcConfigValue.ClaimMappings.Name,
                        oidcConfigValue.ClaimMappings.Email,
                        oidcConfigValue.ClaimMappings.Groups
                    )
                );

        var metadata = new PortalAuthSettingsMetadata(
            basicAuthEnabled.GetValueOrDefault(false),
            oidcAuthEnabled.GetValueOrDefault(false),
            oidcTeamMappingEnabled.GetValueOrDefault(false),
            konnectMappingEnabled.GetValueOrDefault(false),
            oidcConfigMetadata
        );

        var portalDirectory = Path.Combine(inputDirectory, "portals", portalName);
        fileSystem.Directory.EnsureDirectory(portalDirectory);

        var metadataFilename = Path.Combine(portalDirectory, "authentication-settings.json");
        await metadataSerializer.SerializeAsync(metadataFilename, metadata);
    }
}
