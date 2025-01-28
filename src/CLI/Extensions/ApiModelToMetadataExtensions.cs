using Kong.Portal.CLI.ApiClient.Models;
using Kong.Portal.CLI.Services;

namespace Kong.Portal.CLI;

internal static class ApiModelToMetadataExtensions
{
    public static ApiProductMetadata ToMetadata(this ApiProduct apiProduct, string syncId)
    {
        var labels = apiProduct.Labels.Clone();
        labels.Remove(Constants.SyncIdLabel);

        return new ApiProductMetadata(syncId, apiProduct.Name, apiProduct.Description, labels);
    }

    public static ApiProductVersionMetadata ToMetadata(this ApiProductVersion apiProductVersion, string syncId, string? specificationFilename)
    {
        return new ApiProductVersionMetadata(
            syncId,
            apiProductVersion.Name,
            apiProductVersion.PublishStatus.ToMetadataPublishStatus(),
            apiProductVersion.Deprecated,
            specificationFilename
        );
    }

    public static ApiProductDocumentMetadata ToMetadata(this ApiProductDocumentBody apiProductDocument, string fullSlug)
    {
        return new ApiProductDocumentMetadata(
            apiProductDocument.Title,
            apiProductDocument.Slug,
            fullSlug,
            apiProductDocument.Status.ToMetadataPublishStatus()
        );
    }

    public static PortalMetadata ToMetadata(this DevPortal portal)
    {
        return new PortalMetadata(
            portal.Name,
            portal.CustomDomain,
            portal.CustomClientDomain,
            portal.IsPublic,
            portal.AutoApproveDevelopers,
            portal.AutoApproveApplications,
            portal.RbacEnabled
        );
    }

    public static PortalAppearanceMetadata ToMetadata(this DevPortalAppearance portalAppearance)
    {
        return new PortalAppearanceMetadata(
            portalAppearance.ThemeName,
            portalAppearance.UseCustomFonts,
            portalAppearance.CustomTheme == null
                ? null
                : new PortalCustomThemeMetadata(
                    new PortalCustomThemeColorsMetadata(
                        new PortalCustomThemeColorsSectionMetadata(
                            portalAppearance.CustomTheme.Colors.Section.Header.Value,
                            portalAppearance.CustomTheme.Colors.Section.Body.Value,
                            portalAppearance.CustomTheme.Colors.Section.Hero.Value,
                            portalAppearance.CustomTheme.Colors.Section.Accent.Value,
                            portalAppearance.CustomTheme.Colors.Section.Tertiary.Value,
                            portalAppearance.CustomTheme.Colors.Section.Stroke.Value,
                            portalAppearance.CustomTheme.Colors.Section.Footer.Value
                        ),
                        new PortalCustomThemeColorsTextMetadata(
                            portalAppearance.CustomTheme.Colors.Text.Header.Value,
                            portalAppearance.CustomTheme.Colors.Text.Hero.Value,
                            portalAppearance.CustomTheme.Colors.Text.Headings.Value,
                            portalAppearance.CustomTheme.Colors.Text.Primary.Value,
                            portalAppearance.CustomTheme.Colors.Text.Secondary.Value,
                            portalAppearance.CustomTheme.Colors.Text.Accent.Value,
                            portalAppearance.CustomTheme.Colors.Text.Link.Value,
                            portalAppearance.CustomTheme.Colors.Text.Footer.Value
                        ),
                        new PortalCustomThemeColorsButtonMetadata(
                            portalAppearance.CustomTheme.Colors.Button.PrimaryFill.Value,
                            portalAppearance.CustomTheme.Colors.Button.PrimaryText.Value
                        )
                    )
                ),
            new PortalCustomFontsMetadata(
                portalAppearance.CustomFonts?.Base ?? "Roboto",
                portalAppearance.CustomFonts?.Code ?? "Roboto Mono",
                portalAppearance.CustomFonts?.Headings ?? "Lato"
            ),
            new PortalTextMetadata(portalAppearance.Text?.Catalog.WelcomeMessage ?? "", portalAppearance.Text?.Catalog.PrimaryHeader ?? ""),
            portalAppearance.Images == null
                ? PortalImagesMetadata.NullValue
                : new PortalImagesMetadata(
                    portalAppearance.Images.Favicon?.Filename,
                    portalAppearance.Images.Logo?.Filename,
                    portalAppearance.Images.CatalogCover?.Filename
                )
        );
    }

    public static PortalAuthSettingsMetadata ToMetadata(
        this DevPortalAuthSettings authSettings,
        IReadOnlyCollection<DevPortalTeamMapping> teamMappings,
        SyncIdMap teamSyncIdMap
    )
    {
        var oidcConfig =
            authSettings.OidcConfig == null
                ? null
                : new PortalOidcConfig(
                    authSettings.OidcConfig.Issuer,
                    authSettings.OidcConfig.ClientId,
                    authSettings.OidcConfig.ClientSecret ?? "",
                    authSettings.OidcConfig.Scopes,
                    new PortalClaimMappings(
                        authSettings.OidcConfig.ClaimMappings.Name,
                        authSettings.OidcConfig.ClaimMappings.Email,
                        authSettings.OidcConfig.ClaimMappings.Groups
                    )
                );

        var oidcTeamMappings =
            teamMappings.Count > 0
                ? teamMappings.Select(tm => new PortalAuthTeamMapping(teamSyncIdMap.GetSyncId(tm.TeamId), tm.Groups.ToList())).ToList()
                : null;

        return new PortalAuthSettingsMetadata(
            authSettings.BasicAuthEnabled,
            authSettings.OidcAuthEnabled,
            authSettings.OidcTeamMappingEnabled,
            authSettings.KonnectMappingEnabled,
            oidcConfig,
            oidcTeamMappings
        );
    }

    public static PortalTeamsMetadata ToMetadata(
        this IReadOnlyList<DevPortalTeam> teams,
        Dictionary<string, IReadOnlyCollection<DevPortalTeamRole>> teamRolesMap,
        SyncIdMap apiProductIdMap
    )
    {
        var teamsMetadata = new List<PortalTeamMetadata>();

        foreach (var team in teams)
        {
            var products = teamRolesMap[team.Id]
                .Where(r => r.EntityTypeName == Constants.ServicesRoleEntityTypeName)
                .GroupBy(r => r.EntityId)
                .Select(r => new PortalTeamApiProduct(apiProductIdMap.GetSyncId(r.Key), r.Select(v => v.RoleName).ToList()))
                .ToList();

            var teamMetadata = new PortalTeamMetadata(team.Name, team.Description, products);
            teamsMetadata.Add(teamMetadata);
        }

        return new PortalTeamsMetadata(teamsMetadata);
    }

    private static MetadataPublishStatus ToMetadataPublishStatus(this ApiPublishStatus publishStatus) =>
        publishStatus switch
        {
            ApiPublishStatus.Published => MetadataPublishStatus.Published,
            ApiPublishStatus.Unpublished => MetadataPublishStatus.Unpublished,
            _ => throw new ArgumentOutOfRangeException(nameof(publishStatus)),
        };
}
