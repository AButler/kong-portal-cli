using Kong.Portal.CLI.ApiClient.Models;
using Kong.Portal.CLI.Services.Metadata;

namespace Kong.Portal.CLI;

internal static class MetadataMappingExtensions
{
    public static ApiProduct ToApiProduct(this ApiProductMetadata metadata, string? id = null)
    {
        return new ApiProduct(id ?? "**ID**", metadata.Name, metadata.Description, metadata.Labels.WithSyncId(metadata.SyncId));
    }

    private static LabelDictionary WithSyncId(this LabelDictionary labels, string syncId)
    {
        var newLabels = labels.Clone();
        newLabels[Constants.SyncIdLabel] = syncId;
        return newLabels;
    }
}
