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

        var result = new CompareResult(context.ApiProducts, context.ApiProductVersions, context.ApiProductVersionSpecifications);

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

                context.ApiProductVersions.Add(sourceApiProduct.SyncId, []);
                context.ApiProductVersionSpecifications.Add(sourceApiProduct.SyncId, []);
                await CompareApiProductVersions(sourceData, context, sourceApiProduct.SyncId, apiProduct.Id);

                continue;
            }

            context.ApiProducts.Add(Difference.Delete(serverApiProduct.Id, serverApiProduct));
        }

        foreach (var sourceApiProduct in toMatch)
        {
            context.ApiProducts.Add(Difference.Add(sourceApiProduct.SyncId, sourceApiProduct.ToApiProduct()));
            context.ApiProductVersionSpecifications.Add(sourceApiProduct.SyncId, []);
            await CompareApiProductVersions(sourceData, context, sourceApiProduct.SyncId);
        }
    }

    private async Task CompareApiProductVersions(SourceData sourceData, CompareContext context, string apiProductSyncId, string? apiProductId = null)
    {
        var toMatch = sourceData.ApiProductVersions[apiProductSyncId].ToList();

        if (apiProductId == null)
        {
            var versions = toMatch.Select(versionMetadata => Difference.Add(versionMetadata.SyncId, versionMetadata.ToApiProductVersion())).ToList();

            context.ApiProductVersions[apiProductSyncId].AddRange(versions);
            return;
        }

        var serverApiProductVersions = await apiClient.ApiProductVersions.GetAll(apiProductId);

        var versionDifferences = context.ApiProductVersions[apiProductSyncId];

        foreach (var serverApiProductVersion in serverApiProductVersions)
        {
            var sourceApiProductVersion = toMatch.FirstOrDefault(v => v.Name == serverApiProductVersion.Name);

            if (sourceApiProductVersion != null)
            {
                toMatch.Remove(sourceApiProductVersion);

                var apiProductVersion = sourceApiProductVersion.ToApiProductVersion(serverApiProductVersion.Id);
                versionDifferences.Add(
                    Difference.UpdateOrNoChange(
                        sourceApiProductVersion.SyncId,
                        serverApiProductVersion.Id,
                        serverApiProductVersion,
                        apiProductVersion
                    )
                );

                await CompareApiProductVersionSpecification(
                    sourceData,
                    context,
                    apiProductSyncId,
                    apiProductId,
                    sourceApiProductVersion.SyncId,
                    serverApiProductVersion.Id
                );

                continue;
            }

            versionDifferences.Add(Difference.Delete(serverApiProductVersion.Id, serverApiProductVersion));
        }

        foreach (var sourceApiProductVersion in toMatch)
        {
            versionDifferences.Add(Difference.Add(sourceApiProductVersion.SyncId, sourceApiProductVersion.ToApiProductVersion()));
        }
    }

    private async Task CompareApiProductVersionSpecification(
        SourceData sourceData,
        CompareContext context,
        string apiProductSyncId,
        string apiProductId,
        string apiProductVersionSyncId,
        string? apiProductVersionId = null
    )
    {
        var filename = sourceData.ApiProductVersions[apiProductSyncId].Single(v => v.SyncId == apiProductVersionSyncId).SpecificationFilename;

        var sourceApiProductVersionSpecification = sourceData.ApiProductVersionSpecifications[apiProductSyncId][apiProductVersionSyncId];

        if (apiProductVersionId == null)
        {
            if (sourceApiProductVersionSpecification != null)
            {
                context
                    .ApiProductVersionSpecifications[apiProductSyncId]
                    .Add(
                        apiProductVersionSyncId,
                        Difference.Add(
                            apiProductVersionSyncId,
                            new ApiProductSpecification(
                                $"resolve://api-product-specification/{apiProductVersionSyncId}",
                                filename!,
                                sourceApiProductVersionSpecification
                            )
                        )
                    );
            }

            return;
        }

        var serverApiProductVersionSpecification = await apiClient.ApiProductVersions.GetSpecification(apiProductId, apiProductVersionId);

        if (sourceApiProductVersionSpecification != null)
        {
            // Has specification locally
            if (serverApiProductVersionSpecification == null)
            {
                context
                    .ApiProductVersionSpecifications[apiProductSyncId]
                    .Add(
                        apiProductVersionSyncId,
                        Difference.Add(
                            apiProductVersionSyncId,
                            new ApiProductSpecification(
                                $"resolve://api-product-specification/{apiProductVersionSyncId}",
                                filename!,
                                sourceApiProductVersionSpecification
                            )
                        )
                    );
            }
            else
            {
                var apiProductVersionSpecification = new ApiProductSpecification(
                    serverApiProductVersionSpecification.Id,
                    filename!,
                    sourceApiProductVersionSpecification
                );

                context
                    .ApiProductVersionSpecifications[apiProductSyncId]
                    .Add(
                        apiProductVersionSyncId,
                        Difference.UpdateOrNoChange(
                            apiProductVersionSyncId,
                            serverApiProductVersionSpecification.Id,
                            serverApiProductVersionSpecification,
                            apiProductVersionSpecification
                        )
                    );
            }
        }
        else
        {
            // Doesn't have locally
            if (serverApiProductVersionSpecification != null)
            {
                context
                    .ApiProductVersionSpecifications[apiProductSyncId]
                    .Add(apiProductVersionSyncId, Difference.Delete(serverApiProductVersionSpecification.Id, serverApiProductVersionSpecification));
            }
        }
    }

    private static ApiProductMetadata? FindApiProduct(List<ApiProductMetadata> existing, ApiProduct apiProduct)
    {
        var syncIdLabel = apiProduct.GetSyncIdFromLabel();

        if (syncIdLabel != null)
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
        public Dictionary<string, List<Difference<ApiProductVersion>>> ApiProductVersions { get; } = new();
        public Dictionary<string, Dictionary<string, Difference<ApiProductSpecification>>> ApiProductVersionSpecifications { get; } = new();
    }
}
