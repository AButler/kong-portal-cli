using System.Text;
using Kong.Portal.CLI.ApiClient.Models;

namespace Kong.Portal.CLI;

internal static class ApiModelExtensions
{
    public static string? GetSyncIdFromLabel(this ApiProduct apiProduct) => apiProduct.Labels.GetValueOrDefault(Constants.SyncIdLabel);

    public static ApiProductCreate ToCreateModel(this ApiProduct apiProduct) => new(apiProduct.Name, apiProduct.Description, apiProduct.Labels);

    public static ApiProductUpdate ToUpdateModel(this ApiProduct apiProduct) =>
        new(apiProduct.Name, apiProduct.Description, apiProduct.PortalIds, apiProduct.Labels);

    public static ApiProductVersionUpdate ToUpdateModel(this ApiProductVersion apiProductVersion) =>
        new(apiProductVersion.Name, apiProductVersion.PublishStatus, apiProductVersion.Deprecated);

    public static ApiProductSpecificationUpdate ToUpdateModel(this ApiProductSpecification apiProductSpecification) =>
        new(apiProductSpecification.Name, Convert.ToBase64String(Encoding.UTF8.GetBytes(apiProductSpecification.Content)));

    public static ApiProductDocumentUpdate ToUpdateModel(this ApiProductDocumentBody apiProductDocument)
    {
        var content = Convert.ToBase64String(Encoding.UTF8.GetBytes(apiProductDocument.MarkdownContent));

        return new ApiProductDocumentUpdate(
            apiProductDocument.ParentDocumentId,
            apiProductDocument.Slug,
            apiProductDocument.Status,
            apiProductDocument.Title,
            content
        );
    }

    public static DevPortalUpdate ToUpdateModel(this DevPortal devPortal) =>
        new(
            devPortal.CustomDomain,
            devPortal.CustomClientDomain,
            devPortal.IsPublic,
            devPortal.AutoApproveDevelopers,
            devPortal.AutoApproveApplications,
            devPortal.RbacEnabled
        );

    public static DevPortalAuthSettingsUpdate ToUpdateModel(this DevPortalAuthSettings authSettings)
    {
        return new DevPortalAuthSettingsUpdate(
            authSettings.BasicAuthEnabled,
            authSettings.OidcAuthEnabled,
            authSettings.OidcTeamMappingEnabled,
            authSettings.KonnectMappingEnabled,
            authSettings.OidcConfig?.Issuer,
            authSettings.OidcConfig?.ClientId,
            authSettings.OidcConfig?.Scopes,
            authSettings.OidcConfig?.ClaimMappings
        );
    }

    public static ApiProductDocumentBody ResolveDocumentId(this ApiProductDocumentBody document, IReadOnlyDictionary<string, string> map)
    {
        if (!document.TryResolveDocumentId(map, out var result))
        {
            throw new Exception("Could not resolve: " + document.ParentDocumentId);
        }

        return result;
    }

    public static bool TryResolveDocumentId(
        this ApiProductDocumentBody document,
        IReadOnlyDictionary<string, string> map,
        out ApiProductDocumentBody result
    )
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
