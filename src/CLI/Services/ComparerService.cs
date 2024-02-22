using Kong.Portal.CLI.ApiClient;
using Kong.Portal.CLI.ApiClient.Models;
using Kong.Portal.CLI.Services.Models;

namespace Kong.Portal.CLI.Services;

internal class ComparerService
{
    public async Task<CompareResult> Compare(SourceData sourceData, KongApiClient apiClient)
    {
        var context = new CompareContext(apiClient);

        await ComparePortals(sourceData, context);

        await CompareApiProducts(sourceData, context);

        var result = new CompareResult(
            context.ApiProducts,
            context.ApiProductVersions,
            context.ApiProductVersionSpecifications,
            context.Portals,
            context.PortalAppearances
        );

        return result;
    }

    private async Task ComparePortals(SourceData sourceData, CompareContext context)
    {
        var toMatch = sourceData.Portals.ToList();

        var serverPortals = await context.ApiClient.DevPortals.GetAll();

        foreach (var serverPortal in serverPortals)
        {
            var sourcePortal = toMatch.FirstOrDefault(p => p.Name == serverPortal.Name);
            if (sourcePortal != null)
            {
                toMatch.Remove(sourcePortal);

                var portal = sourcePortal.ToApiModel(serverPortal.Id);
                context.Portals.Add(Difference.UpdateOrNoChange(sourcePortal.Name, serverPortal.Id, serverPortal, portal));

                await ComparePortalAppearance(sourceData, context, sourcePortal.Name, serverPortal.Id);

                continue;
            }

            context.Portals.Add(Difference.Delete(serverPortal.Id, serverPortal));
        }

        foreach (var sourcePortal in toMatch)
        {
            context.Portals.Add(Difference.Add(sourcePortal.Name, sourcePortal.ToApiModel()));

            await ComparePortalAppearance(sourceData, context, sourcePortal.Name);
        }
    }

    private async Task ComparePortalAppearance(SourceData sourceData, CompareContext context, string portalName, string? portalId = null)
    {
        var toMatch = sourceData.PortalAppearances[portalName];
        var imageData = sourceData.PortalAppearanceImageData[portalName];

        if (portalId == null)
        {
            context.PortalAppearances[portalName] = Difference.Add(portalName, toMatch.ToApiModel(imageData));
            return;
        }

        var serverPortalAppearance = await context.ApiClient.DevPortals.GetAppearance(portalId);
        context.PortalAppearances[portalName] = Difference.UpdateOrNoChange(
            portalName,
            portalId,
            serverPortalAppearance,
            toMatch.ToApiModel(imageData)
        );
    }

    private async Task CompareApiProducts(SourceData sourceData, CompareContext context)
    {
        var toMatch = sourceData.ApiProducts.ToList();

        var serverApiProducts = await context.ApiClient.ApiProducts.GetAll();

        foreach (var serverApiProduct in serverApiProducts)
        {
            var sourceApiProduct = FindApiProduct(toMatch, serverApiProduct);
            if (sourceApiProduct != null)
            {
                toMatch.Remove(sourceApiProduct);

                var apiProduct = sourceApiProduct.ToApiModel(context.PortalNameMap, serverApiProduct.Id);
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
            context.ApiProducts.Add(Difference.Add(sourceApiProduct.SyncId, sourceApiProduct.ToApiModel(context.PortalNameMap)));

            context.ApiProductVersions.Add(sourceApiProduct.SyncId, []);
            context.ApiProductVersionSpecifications.Add(sourceApiProduct.SyncId, []);
            await CompareApiProductVersions(sourceData, context, sourceApiProduct.SyncId);
        }
    }

    private async Task CompareApiProductVersions(SourceData sourceData, CompareContext context, string apiProductSyncId, string? apiProductId = null)
    {
        var toMatch = sourceData.ApiProductVersions[apiProductSyncId].ToList();

        if (apiProductId == null)
        {
            foreach (var versionMetadata in toMatch)
            {
                context.ApiProductVersions[apiProductSyncId].Add(Difference.Add(versionMetadata.SyncId, versionMetadata.ToApiModel()));

                var sourceApiProductVersionSpecification = sourceData.ApiProductVersionSpecifications[apiProductSyncId][versionMetadata.SyncId];
                if (sourceApiProductVersionSpecification != null)
                {
                    context
                        .ApiProductVersionSpecifications[apiProductSyncId]
                        .Add(
                            versionMetadata.SyncId,
                            Difference.Add(
                                versionMetadata.SyncId,
                                new ApiProductSpecification(
                                    $"resolve://api-product-specification/{versionMetadata.SyncId}",
                                    versionMetadata.SpecificationFilename!,
                                    sourceApiProductVersionSpecification
                                )
                            )
                        );
                }
            }

            return;
        }

        var serverApiProductVersions = await context.ApiClient.ApiProductVersions.GetAll(apiProductId);

        var versionDifferences = context.ApiProductVersions[apiProductSyncId];

        foreach (var serverApiProductVersion in serverApiProductVersions)
        {
            var sourceApiProductVersion = toMatch.FirstOrDefault(v => v.Name == serverApiProductVersion.Name);

            if (sourceApiProductVersion != null)
            {
                toMatch.Remove(sourceApiProductVersion);

                var apiProductVersion = sourceApiProductVersion.ToApiModel(serverApiProductVersion.Id);
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
            versionDifferences.Add(Difference.Add(sourceApiProductVersion.SyncId, sourceApiProductVersion.ToApiModel()));

            await CompareApiProductVersionSpecification(sourceData, context, apiProductSyncId, apiProductId, sourceApiProductVersion.SyncId);
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
        var apiProductVersion = sourceData.ApiProductVersions[apiProductSyncId].Single(v => v.SyncId == apiProductVersionSyncId);

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
                            apiProductVersion.ToApiProductVersionSpecification(sourceApiProductVersionSpecification)
                        )
                    );
            }

            return;
        }

        var serverApiProductVersionSpecification = await context.ApiClient.ApiProductVersions.GetSpecification(apiProductId, apiProductVersionId);

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
                            apiProductVersion.ToApiProductVersionSpecification(sourceApiProductVersionSpecification)
                        )
                    );
            }
            else
            {
                var apiProductVersionSpecification = apiProductVersion.ToApiProductVersionSpecification(
                    sourceApiProductVersionSpecification,
                    serverApiProductVersionSpecification.Id
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

    private class CompareContext(KongApiClient apiClient)
    {
        public KongApiClient ApiClient { get; } = apiClient;
        public List<Difference<DevPortal>> Portals { get; } = new();
        public Dictionary<string, Difference<DevPortalAppearance>> PortalAppearances { get; } = new();
        public List<Difference<ApiProduct>> ApiProducts { get; } = new();
        public Dictionary<string, List<Difference<ApiProductVersion>>> ApiProductVersions { get; } = new();
        public Dictionary<string, Dictionary<string, Difference<ApiProductSpecification>>> ApiProductVersionSpecifications { get; } = new();
        public IReadOnlyDictionary<string, string> PortalNameMap =>
            Portals.Where(p => p is { SyncId: not null, Id: not null }).ToDictionary(kvp => kvp.SyncId!, kvp => kvp.Id!);
    }
}
