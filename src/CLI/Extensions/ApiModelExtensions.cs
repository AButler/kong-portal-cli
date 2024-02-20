using Kong.Portal.CLI.ApiClient.Models;
using Kong.Portal.CLI.Services.Metadata;

namespace Kong.Portal.CLI;

internal static class ApiModelExtensions
{
    public static ApiProductUpdate ToUpdateModel(this ApiProduct apiProduct) => new(apiProduct.Name, apiProduct.Description, apiProduct.Labels);

    public static string? GetSyncIdFromLabel(this ApiProduct apiProduct) => apiProduct.Labels.GetValueOrDefault(Constants.SyncIdLabel);

    public static ApiProductMetadata ToMetadata(this ApiProduct apiProduct, string syncId)
    {
        var labels = apiProduct.Labels.Clone();
        labels.Remove(Constants.SyncIdLabel);

        return new ApiProductMetadata(syncId, apiProduct.Name, apiProduct.Description, labels);
    }
}
