using Kong.Portal.CLI.ApiClient;
using Kong.Portal.CLI.ApiClient.Models;
using Kong.Portal.CLI.Services.Metadata;
using Kong.Portal.CLI.Services.Models;

namespace Kong.Portal.CLI.Services;

internal class ComparerService(KongApiClient apiClient)
{
    public async Task<CompareResult> Compare(SourceData sourceData)
    {
        var context = new CompareContext();

        await CompareApiProducts(sourceData, context);

        var result = new CompareResult(context.ApiProducts);

        return result;
    }

    private async Task CompareApiProducts(SourceData sourceData, CompareContext context)
    {
        var toMatch = sourceData.ApiProducts.ToList();

        var serverApiProducts = await apiClient.ApiProducts.GetAll();

        foreach (var serverApiProduct in serverApiProducts)
        {
            var sourceApiProduct = FindApiProduct(toMatch, serverApiProduct);
            if (sourceApiProduct != null)
            {
                toMatch.Remove(sourceApiProduct);

                var apiProduct = sourceApiProduct.ToApiProduct(serverApiProduct.Id);
                context.ApiProducts.Add(Difference.UpdateOrNoChange(sourceApiProduct.SyncId, serverApiProduct.Id, serverApiProduct, apiProduct));

                continue;
            }

            context.ApiProducts.Add(Difference.Delete(serverApiProduct.Id, serverApiProduct));
        }

        foreach (var sourceApiProduct in toMatch)
        {
            context.ApiProducts.Add(Difference.Add(sourceApiProduct.SyncId, sourceApiProduct.ToApiProduct()));
        }
    }

    private static ApiProductMetadata? FindApiProduct(List<ApiProductMetadata> existing, ApiProduct apiProduct)
    {
        if (apiProduct.Labels.TryGetValue(Constants.SyncIdLabel, out var syncIdLabel))
        {
            var matchedOnSyncId = existing.FirstOrDefault(p => p.SyncId == syncIdLabel);
            if (matchedOnSyncId != null)
            {
                return matchedOnSyncId;
            }
        }

        var matchedOnName = existing.FirstOrDefault(p => p.Name == apiProduct.Name);
        return matchedOnName;
    }

    private class CompareContext
    {
        public List<Difference<ApiProduct>> ApiProducts { get; } = new();
    }
}
