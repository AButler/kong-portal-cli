using Kong.Portal.CLI.ApiClient.Models;
using Kong.Portal.CLI.Services.Metadata;

namespace Kong.Portal.CLI;

internal static class MetadataMappingExtensions
{
    public static ApiClient.Models.Portal ToPortal(this PortalMetadata metadata, string? id = null)
    {
        return new ApiClient.Models.Portal(
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

    public static ApiProduct ToApiProduct(this ApiProductMetadata metadata, IReadOnlyDictionary<string, string> portalNameMap, string? id = null)
    {
        var portalIds = new List<string>();

        foreach (var portalName in metadata.Portals)
        {
            portalIds.Add(portalNameMap[portalName]);
        }

        return new ApiProduct(
            id ?? $"resolve://api-product/{metadata.SyncId}",
            metadata.Name,
            metadata.Description,
            portalIds,
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
