using Kong.Portal.CLI.ApiClient.Models;
using Kong.Portal.CLI.Services.Metadata;

namespace Kong.Portal.CLI;

internal static class ApiModelExtensions
{
    public static string? GetSyncIdFromLabel(this ApiProduct apiProduct) => apiProduct.Labels.GetValueOrDefault(Constants.SyncIdLabel);

    public static ApiProductUpdate ToUpdateModel(this ApiProduct apiProduct) => new(apiProduct.Name, apiProduct.Description, apiProduct.Labels);

    public static ApiProductMetadata ToMetadata(this ApiProduct apiProduct, string syncId)
    {
        var labels = apiProduct.Labels.Clone();
        labels.Remove(Constants.SyncIdLabel);

        return new ApiProductMetadata(syncId, apiProduct.Name, apiProduct.Description, labels);
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

    private static ApiProductVersionMetadataPublishStatus ToMetadataPublishStatus(this ApiVersionPublishStatus publishStatus) =>
        publishStatus switch
        {
            ApiVersionPublishStatus.Published => ApiProductVersionMetadataPublishStatus.Published,
            ApiVersionPublishStatus.Unpublished => ApiProductVersionMetadataPublishStatus.Unpublished,
            _ => throw new ArgumentOutOfRangeException(nameof(publishStatus))
        };
}
