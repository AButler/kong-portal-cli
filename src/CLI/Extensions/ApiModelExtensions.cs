using Kong.Portal.CLI.ApiClient.Models;

namespace Kong.Portal.CLI;

internal static class ApiModelExtensions
{
    public static ApiProduct WithSyncIdLabel(this ApiProduct apiProduct, string syncId)
    {
        var labels = apiProduct.Labels.Clone();
        labels[Constants.SyncIdLabel] = syncId;

        return apiProduct with
        {
            Labels = labels
        };
    }
}
