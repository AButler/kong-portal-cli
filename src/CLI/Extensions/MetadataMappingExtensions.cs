using Kong.Portal.CLI.ApiClient.Models;
using Kong.Portal.CLI.Services.Metadata;

namespace Kong.Portal.CLI;

internal static class MetadataMappingExtensions
{
    public static ApiProduct ToApiProduct(this ApiProductMetadata metadata, string? id = null)
    {
        return new ApiProduct(
            id ?? $"resolve://api-product/{metadata.SyncId}",
            metadata.Name,
            metadata.Description,
            metadata.Labels.WithSyncId(metadata.SyncId)
        );
    }

    public static ApiProductVersion ToApiProductVersion(this ApiProductVersionMetadata metadata, string? id = null)
    {
        return new ApiProductVersion(
            id ?? $"resolve://api-product-version/{metadata.SyncId}",
            metadata.Name,
            metadata.PublishStatus.ToApiPublishStatus(),
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

    private static ApiVersionPublishStatus ToApiPublishStatus(this ApiProductVersionMetadataPublishStatus publishStatus) =>
        publishStatus switch
        {
            ApiProductVersionMetadataPublishStatus.Published => ApiVersionPublishStatus.Published,
            ApiProductVersionMetadataPublishStatus.Unpublished => ApiVersionPublishStatus.Unpublished,
            _ => throw new ArgumentOutOfRangeException(nameof(publishStatus))
        };

    private static LabelDictionary WithSyncId(this LabelDictionary labels, string syncId)
    {
        var newLabels = labels.Clone();
        newLabels[Constants.SyncIdLabel] = syncId;
        return newLabels;
    }
}
