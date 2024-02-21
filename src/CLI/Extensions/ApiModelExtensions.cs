using System.Text;
using Kong.Portal.CLI.ApiClient.Models;
using Kong.Portal.CLI.Services.Metadata;

namespace Kong.Portal.CLI;

internal static class ApiModelExtensions
{
    public static string? GetSyncIdFromLabel(this ApiProduct apiProduct) => apiProduct.Labels.GetValueOrDefault(Constants.SyncIdLabel);

    public static ApiProductCreate ToCreateModel(this ApiProduct apiProduct) => new(apiProduct.Name, apiProduct.Description, apiProduct.Labels);

    public static ApiProductUpdate ToUpdateModel(this ApiProduct apiProduct) =>
        new(apiProduct.Name, apiProduct.Description, apiProduct.PortalIds, apiProduct.Labels);

    public static ApiProductMetadata ToMetadata(this ApiProduct apiProduct, string syncId, IReadOnlyDictionary<string, string> portalMap)
    {
        var labels = apiProduct.Labels.Clone();
        labels.Remove(Constants.SyncIdLabel);

        var portals = new List<string>();
        foreach (var portalId in apiProduct.PortalIds)
        {
            portals.Add(portalMap[portalId]);
        }

        return new ApiProductMetadata(syncId, apiProduct.Name, apiProduct.Description, portals, labels);
    }

    public static ApiProductVersionUpdate ToUpdateModel(this ApiProductVersion apiProductVersion) =>
        new(apiProductVersion.Name, apiProductVersion.PublishStatus, apiProductVersion.Deprecated);

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

    public static ApiProductSpecificationUpdate ToUpdateModel(this ApiProductSpecification apiProductSpecification) =>
        new(apiProductSpecification.Name, Convert.ToBase64String(Encoding.UTF8.GetBytes(apiProductSpecification.Content)));

    private static ApiProductVersionMetadataPublishStatus ToMetadataPublishStatus(this ApiVersionPublishStatus publishStatus) =>
        publishStatus switch
        {
            ApiVersionPublishStatus.Published => ApiProductVersionMetadataPublishStatus.Published,
            ApiVersionPublishStatus.Unpublished => ApiProductVersionMetadataPublishStatus.Unpublished,
            _ => throw new ArgumentOutOfRangeException(nameof(publishStatus))
        };
}
