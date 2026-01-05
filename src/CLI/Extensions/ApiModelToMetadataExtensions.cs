using Kong.Portal.CLI.ApiClient.Models;
using Kong.Portal.CLI.Services;

namespace Kong.Portal.CLI;

internal static class ApiModelToMetadataExtensions
{
    extension(ApiProduct apiProduct)
    {
        public ApiProductMetadata ToMetadata(string syncId)
        {
            var labels = apiProduct.Labels.Clone();
            labels.Remove(Constants.SyncIdLabel);

            return new ApiProductMetadata(syncId, apiProduct.Name, apiProduct.Description, labels);
        }
    }

    extension(ApiProductVersion apiProductVersion)
    {
        public ApiProductVersionMetadata ToMetadata(string syncId, string? specificationFilename)
        {
            return new ApiProductVersionMetadata(
                syncId,
                apiProductVersion.Name,
                apiProductVersion.PublishStatus.ToMetadataPublishStatus(),
                apiProductVersion.Deprecated,
                specificationFilename
            );
        }
    }

    extension(ApiProductDocumentBody apiProductDocument)
    {
        public ApiProductDocumentMetadata ToMetadata(string fullSlug)
        {
            return new ApiProductDocumentMetadata(
                apiProductDocument.Title,
                apiProductDocument.Slug,
                fullSlug,
                apiProductDocument.Status.ToMetadataPublishStatus()
            );
        }
    }

    extension(DevPortal portal)
    {
        public PortalMetadata ToMetadata()
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
    }

    extension(DevPortalAppearance portalAppearance)
    {
        public PortalAppearanceMetadata ToMetadata()
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
    }

    extension(DevPortalAuthSettings authSettings)
    {
        public PortalAuthSettingsMetadata ToMetadata(IReadOnlyCollection<DevPortalTeamMapping> teamMappings, SyncIdMap teamSyncIdMap)
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
    }

    extension(IReadOnlyList<DevPortalTeam> teams)
    {
        public PortalTeamsMetadata ToMetadata(Dictionary<string, IReadOnlyCollection<DevPortalTeamRole>> teamRolesMap, SyncIdMap apiProductIdMap)
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
    }

    extension(ApiPublishStatus publishStatus)
    {
        private MetadataPublishStatus ToMetadataPublishStatus() =>
            publishStatus switch
            {
                ApiPublishStatus.Published => MetadataPublishStatus.Published,
                ApiPublishStatus.Unpublished => MetadataPublishStatus.Unpublished,
                _ => throw new ArgumentOutOfRangeException(nameof(publishStatus)),
            };
    }
}
