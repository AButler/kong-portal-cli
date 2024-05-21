using Kong.Portal.CLI.ApiClient.Models;
using Kong.Portal.CLI.Services;

namespace Kong.Portal.CLI;

internal static class MetadataMappingExtensions
{
    public static DevPortal ToApiModel(this PortalMetadata metadata, string? id = null)
    {
        return new DevPortal(
            id ?? $"resolve://portal/{metadata.Name}",
            metadata.Name,
            metadata.CustomDomain,
            metadata.CustomClientDomain,
            metadata.IsPublic,
            metadata.AutoApproveDevelopers,
            metadata.AutoApproveApplications,
            metadata.RbacEnabled
        );
    }

    public static DevPortalAuthSettings ToApiModel(this PortalAuthSettingsMetadata metadata)
    {
        var oidcConfig =
            metadata.OidcConfig == null
                ? null
                : new DevPortalOidcConfig(
                    metadata.OidcConfig.Issuer,
                    metadata.OidcConfig.ClientId,
                    metadata.OidcConfig.ClientSecret,
                    metadata.OidcConfig.Scopes,
                    new DevPortalClaimMappings(
                        metadata.OidcConfig.ClaimMappings.Name,
                        metadata.OidcConfig.ClaimMappings.Email,
                        metadata.OidcConfig.ClaimMappings.Groups
                    )
                );

        return new DevPortalAuthSettings(
            metadata.BasicAuthEnabled,
            metadata.OidcAuthEnabled,
            metadata.OidcTeamMappingEnabled,
            metadata.KonnectMappingEnabled,
            oidcConfig
        );
    }

    public static DevPortalTeamMappingBody ToTeamMappingsApiModel(this PortalAuthSettingsMetadata metadata, SyncIdMap teamIdMap)
    {
        if (metadata.OidcTeamMappings == null)
        {
            return new DevPortalTeamMappingBody(new List<DevPortalTeamMapping>());
        }

        var teamMappings = new List<DevPortalTeamMapping>();
        foreach (var mapping in metadata.OidcTeamMappings)
        {
            var teamId = teamIdMap.GetIdOrDefault(mapping.Team, $"resolve://portal-team/{mapping.Team}");
            teamMappings.Add(new DevPortalTeamMapping(teamId, mapping.OidcGroups.ToList()));
        }

        return new DevPortalTeamMappingBody(teamMappings);
    }

    public static DevPortalAppearance ToApiModel(this PortalAppearanceMetadata metadata, ImageData imageData)
    {
        return new DevPortalAppearance(
            metadata.ThemeName,
            metadata.UseCustomFonts,
            metadata.CustomTheme.ToApiModel(),
            new DevPortalAppearanceCustomFonts(metadata.CustomFonts.Base, metadata.CustomFonts.Code, metadata.CustomFonts.Headings),
            new DevPortalAppearanceText(new DevPortalAppearanceTextCatalog(metadata.Text.WelcomeMessage, metadata.Text.PrimaryHeader)),
            metadata.Images.ToApiModel(imageData)
        );
    }

    public static DevPortalAppearanceCustomTheme? ToApiModel(this PortalCustomThemeMetadata? metadata)
    {
        if (metadata == null)
        {
            return null;
        }

        var section = new DevPortalAppearanceCustomThemeColorsSection(
            new PortalAppearanceColorValue(metadata.Colors.Section.Header),
            new PortalAppearanceColorValue(metadata.Colors.Section.Body),
            new PortalAppearanceColorValue(metadata.Colors.Section.Hero),
            new PortalAppearanceColorValue(metadata.Colors.Section.Accent),
            new PortalAppearanceColorValue(metadata.Colors.Section.Tertiary),
            new PortalAppearanceColorValue(metadata.Colors.Section.Stroke),
            new PortalAppearanceColorValue(metadata.Colors.Section.Footer)
        );

        var text = new DevPortalAppearanceCustomThemeColorsText(
            new PortalAppearanceColorValue(metadata.Colors.Text.Header),
            new PortalAppearanceColorValue(metadata.Colors.Text.Hero),
            new PortalAppearanceColorValue(metadata.Colors.Text.Headings),
            new PortalAppearanceColorValue(metadata.Colors.Text.Primary),
            new PortalAppearanceColorValue(metadata.Colors.Text.Secondary),
            new PortalAppearanceColorValue(metadata.Colors.Text.Accent),
            new PortalAppearanceColorValue(metadata.Colors.Text.Link),
            new PortalAppearanceColorValue(metadata.Colors.Text.Footer)
        );

        var button = new PortalAppearanceCustomThemeColorsButton(
            new PortalAppearanceColorValue(metadata.Colors.Button.PrimaryFill),
            new PortalAppearanceColorValue(metadata.Colors.Button.PrimaryText)
        );

        var colors = new DevPortalAppearanceCustomThemeColors(section, text, button);

        return new DevPortalAppearanceCustomTheme(colors);
    }

    public static DevPortalTeam ToApiModel(this PortalTeamMetadata metadata, string? id = null)
    {
        return new DevPortalTeam(id ?? $"resolve://portal-team/{metadata.Name}", metadata.Name, metadata.Description);
    }

    public static IReadOnlyCollection<DevPortalTeamRole> ToApiModel(
        this PortalTeamApiProduct metadata,
        SyncIdMap apiProductMap,
        string region,
        string? id = null
    )
    {
        var apiProductId = apiProductMap.GetIdOrDefault(metadata.ApiProduct, $"resolve://api-product/{metadata.ApiProduct}");

        return metadata.Roles.Select(role => ToApiModel(metadata.ApiProduct, apiProductId, role, region, id)).ToList();
    }

    public static DevPortalTeamRole ToApiModel(string apiProduct, string apiProductId, string role, string region, string? id = null)
    {
        return new DevPortalTeamRole(
            id ?? $"resolve://portal-team-role/{apiProduct}/{role}",
            role,
            apiProductId,
            Constants.ServicesRoleEntityTypeName,
            region
        );
    }

    private static DevPortalAppearanceImages ToApiModel(this PortalImagesMetadata metadata, ImageData imageData)
    {
        var favicon = imageData.Favicon == null ? null : new DevPortalAppearanceImage(imageData.Favicon, metadata.Favicon!);
        var logo = imageData.Logo == null ? null : new DevPortalAppearanceImage(imageData.Logo, metadata.Logo!);
        var catalogCover = imageData.CatalogCover == null ? null : new DevPortalAppearanceImage(imageData.CatalogCover, metadata.CatalogCover!);

        return new DevPortalAppearanceImages(favicon, logo, catalogCover);
    }

    public static ApiProduct ToApiModel(this ApiProductMetadata metadata, string? id = null)
    {
        return new ApiProduct(
            id ?? $"resolve://api-product/{metadata.SyncId}",
            metadata.Name,
            metadata.Description,
            [],
            metadata.Labels.WithSyncId(metadata.SyncId)
        );
    }

    public static ApiProductVersion ToApiModel(this ApiProductVersionMetadata metadata, string? id = null)
    {
        return new ApiProductVersion(
            id ?? $"resolve://api-product-version/{metadata.SyncId}",
            metadata.Name,
            metadata.PublishStatus.ToApiModel(),
            metadata.Deprecated
        );
    }

    public static ApiProductSpecification ToApiProductVersionSpecification(
        this ApiProductVersionMetadata metadata,
        string specificationContent,
        string? id = null
    )
    {
        return new ApiProductSpecification(
            id ?? $"resolve://api-product-specification/{metadata.SyncId}",
            metadata.SpecificationFilename!,
            specificationContent
        );
    }

    public static ApiProductDocumentBody ToApiModel(this ApiProductDocumentMetadata metadata, string contents, string? id = null)
    {
        var parentSlug = GetParentSlug(metadata.FullSlug);

        return new ApiProductDocumentBody(
            id ?? $"resolve://api-product-document/{metadata.FullSlug}",
            parentSlug == null ? null : $"resolve://api-product-document/{parentSlug}",
            metadata.Slug,
            metadata.FullSlug,
            metadata.Status.ToApiModel(),
            metadata.Title,
            contents
        );
    }

    private static ApiPublishStatus ToApiModel(this MetadataPublishStatus publishStatus) =>
        publishStatus switch
        {
            MetadataPublishStatus.Published => ApiPublishStatus.Published,
            MetadataPublishStatus.Unpublished => ApiPublishStatus.Unpublished,
            _ => throw new ArgumentOutOfRangeException(nameof(publishStatus))
        };

    private static LabelDictionary WithSyncId(this LabelDictionary labels, string syncId)
    {
        var newLabels = labels.Clone();
        newLabels[Constants.SyncIdLabel] = syncId;
        return newLabels;
    }

    private static string? GetRoleId(IDictionary<string, IDictionary<string, string>> roleIdMap, string apiProductId, string role)
    {
        if (!roleIdMap.TryGetValue(apiProductId, out var productRoleMap))
        {
            return null;
        }

        return productRoleMap.TryGetValue(role, out var roleId) ? roleId : null;
    }

    private static string? GetParentSlug(string slug)
    {
        var parentPath = Path.GetDirectoryName(slug)!.Replace(@"\", "/");

        return string.IsNullOrEmpty(parentPath) ? null : parentPath;
    }
}
