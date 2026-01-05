using System.Text;
using Kong.Portal.CLI.ApiClient.Models;

namespace Kong.Portal.CLI;

internal static class ApiModelExtensions
{
    extension(ApiProduct apiProduct)
    {
        public string? GetSyncIdFromLabel() => apiProduct.Labels.GetValueOrDefault(Constants.SyncIdLabel);

        public ApiProductCreate ToCreateModel() => new(apiProduct.Name, apiProduct.Description, apiProduct.Labels);

        public ApiProductUpdate ToUpdateModel() => new(apiProduct.Name, apiProduct.Description, apiProduct.Labels);
    }

    extension(ApiProductVersion apiProductVersion)
    {
        public ApiProductVersionUpdate ToUpdateModel() => new(apiProductVersion.Name, apiProductVersion.PublishStatus, apiProductVersion.Deprecated);
    }

    extension(ApiProductSpecification apiProductSpecification)
    {
        public ApiProductSpecificationUpdate ToUpdateModel() =>
            new(apiProductSpecification.Name, Convert.ToBase64String(Encoding.UTF8.GetBytes(apiProductSpecification.Content)));
    }

    extension(ApiProductDocumentBody document)
    {
        public ApiProductDocumentUpdate ToUpdateModel()
        {
            var content = Convert.ToBase64String(Encoding.UTF8.GetBytes(document.MarkdownContent));

            return new ApiProductDocumentUpdate(document.ParentDocumentId, document.Slug, document.Status, document.Title, content);
        }

        public ApiProductDocumentBody ResolveDocumentId(IReadOnlyDictionary<string, string> map)
        {
            if (!document.TryResolveDocumentId(map, out var result))
            {
                throw new Exception("Could not resolve: " + document.ParentDocumentId);
            }

            return result;
        }

        public bool TryResolveDocumentId(IReadOnlyDictionary<string, string> map, out ApiProductDocumentBody result)
        {
            if (document.ParentDocumentId == null)
            {
                result = document;
                return true;
            }

            if (!document.ParentDocumentId.StartsWith("resolve://api-product-document/"))
            {
                result = document;
                return true;
            }

            var parentSlug = document.ParentDocumentId.Substring("resolve://api-product-document/".Length);

            if (!map.TryGetValue(parentSlug, out var parentId))
            {
                result = document;
                return false;
            }

            result = document with { ParentDocumentId = parentId };
            return true;
        }
    }

    extension(DevPortal devPortal)
    {
        public DevPortalUpdate ToUpdateModel() =>
            new(
                devPortal.CustomDomain,
                devPortal.CustomClientDomain,
                devPortal.IsPublic,
                devPortal.AutoApproveDevelopers,
                devPortal.AutoApproveApplications,
                devPortal.RbacEnabled
            );
    }

    extension(DevPortalAuthSettings authSettings)
    {
        public DevPortalAuthSettingsUpdate ToUpdateModel()
        {
            return new DevPortalAuthSettingsUpdate(
                authSettings.BasicAuthEnabled,
                authSettings.OidcAuthEnabled,
                authSettings.OidcTeamMappingEnabled,
                authSettings.KonnectMappingEnabled,
                authSettings.OidcConfig?.Issuer,
                authSettings.OidcConfig?.ClientId,
                authSettings.OidcConfig?.ClientSecret,
                authSettings.OidcConfig?.Scopes,
                authSettings.OidcConfig?.ClaimMappings
            );
        }
    }

    extension(DevPortalTeam team)
    {
        public DevPortalTeamUpdate ToUpdateModel()
        {
            return new DevPortalTeamUpdate(team.Name, team.Description);
        }
    }

    extension(DevPortalTeamRole role)
    {
        public DevPortalTeamRoleCreate ToCreateModel()
        {
            return new DevPortalTeamRoleCreate(role.RoleName, role.EntityId, role.EntityTypeName, role.EntityRegion);
        }

        public DevPortalTeamRole Resolve(SyncIdMap apiProductIdMap)
        {
            return role.EntityId.StartsWith("resolve://api-product/")
                ? role with
                {
                    EntityId = apiProductIdMap.GetId(role.EntityId.Substring("resolve://api-product/".Length)),
                }
                : role;
        }
    }

    extension(DevPortalTeamMappingBody body)
    {
        public DevPortalTeamMappingBody Resolve(SyncIdMap teamIdMap)
        {
            var teamMappings = new List<DevPortalTeamMapping>();

            foreach (var teamMapping in body.Data)
            {
                var mapping = teamMapping.TeamId.StartsWith("resolve://portal-team/")
                    ? new DevPortalTeamMapping(
                        teamIdMap.GetId(teamMapping.TeamId.Substring("resolve://portal-team/".Length)),
                        teamMapping.Groups.ToList()
                    )
                    : teamMapping;

                teamMappings.Add(mapping);
            }

            return new DevPortalTeamMappingBody(teamMappings);
        }
    }
}
