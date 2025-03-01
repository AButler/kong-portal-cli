﻿using Kong.Portal.CLI.ApiClient;
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

        await ComparePortalsPhase2(sourceData, context);

        var result = new CompareResult(
            context.ApiProducts,
            context.ApiProductVersions,
            context.ApiProductVersionSpecifications,
            context.ApiProductDocuments,
            context.Portals,
            context.ApiProductAssociations,
            context.PortalAppearances,
            context.PortalAuthSettings,
            context.PortalTeams,
            context.PortalTeamRoles,
            context.PortalTeamMappings
        );

        return result;
    }

    private static async Task ComparePortalsPhase2(SourceData sourceData, CompareContext context)
    {
        foreach (var portal in context.Portals)
        {
            if (portal.SyncId == null)
            {
                continue;
            }

            foreach (var team in context.PortalTeams[portal.SyncId])
            {
                if (team.SyncId == null)
                {
                    continue;
                }

                context.PortalTeamRoles[portal.SyncId!].Add(team.SyncId!, []);

                var sourceTeam = sourceData.PortalTeams[portal.SyncId!].First(t => t.Name == team.SyncId);

                await ComparePortalTeamRoles(context, portal.SyncId, sourceTeam, portal.Id, team.Id);
            }
        }
    }

    private static async Task ComparePortals(SourceData sourceData, CompareContext context)
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
                context.PortalTeams.Add(sourcePortal.Name, []);
                context.PortalTeamRoles.Add(sourcePortal.Name, []);

                await ComparePortalAppearance(sourceData, context, sourcePortal.Name, serverPortal.Id);

                await ComparePortalTeams(sourceData, context, sourcePortal.Name, serverPortal.Id);

                await ComparePortalAuthSettings(sourceData, context, sourcePortal.Name, serverPortal.Id);

                continue;
            }

            context.Portals.Add(Difference.Delete(serverPortal.Id, serverPortal));
        }

        foreach (var sourcePortal in toMatch)
        {
            context.Portals.Add(Difference.Add(sourcePortal.Name, sourcePortal.ToApiModel()));
            context.PortalTeams.Add(sourcePortal.Name, []);
            context.PortalTeamRoles.Add(sourcePortal.Name, []);

            await ComparePortalAppearance(sourceData, context, sourcePortal.Name);
            await ComparePortalTeams(sourceData, context, sourcePortal.Name);
            await ComparePortalAuthSettings(sourceData, context, sourcePortal.Name);
        }
    }

    private static async Task ComparePortalTeams(SourceData sourceData, CompareContext context, string portalName, string? portalId = null)
    {
        var toMatch = sourceData.PortalTeams[portalName].ToList();
        var differences = context.PortalTeams[portalName];

        if (portalId == null)
        {
            foreach (var sourcePortalTeam in toMatch)
            {
                differences.Add(Difference.Add(sourcePortalTeam.Name, sourcePortalTeam.ToApiModel()));
            }

            return;
        }

        var serverPortalTeams = await context.ApiClient.DevPortals.GetTeams(portalId);

        foreach (var serverPortalTeam in serverPortalTeams)
        {
            var sourcePortalTeam = toMatch.FirstOrDefault(t => t.Name == serverPortalTeam.Name);

            if (sourcePortalTeam != null)
            {
                toMatch.Remove(sourcePortalTeam);

                var team = sourcePortalTeam.ToApiModel(serverPortalTeam.Id);

                differences.Add(Difference.UpdateOrNoChange(sourcePortalTeam.Name, serverPortalTeam.Id, serverPortalTeam, team));

                continue;
            }

            differences.Add(Difference.Delete(serverPortalTeam.Id, serverPortalTeam));
        }

        foreach (var sourcePortalTeam in toMatch)
        {
            differences.Add(Difference.Add(sourcePortalTeam.Name, sourcePortalTeam.ToApiModel()));
        }
    }

    private static async Task ComparePortalTeamRoles(
        CompareContext context,
        string portalName,
        PortalTeamMetadata sourcePortalTeam,
        string? portalId = null,
        string? teamId = null
    )
    {
        var differences = context.PortalTeamRoles[portalName][sourcePortalTeam.Name];

        if (portalId == null || teamId == null)
        {
            foreach (var apiProduct in sourcePortalTeam.ApiProducts)
            {
                var roles = apiProduct.ToApiModel(context.ApiProductMap, context.ApiClient.Region);
                foreach (var role in roles)
                {
                    differences.Add(Difference.Add($"{apiProduct.ApiProduct} - {role.RoleName}", role));
                }
            }

            return;
        }

        var processedRoles = new List<(string ApiProduct, string RoleName)>();
        var serverTeamRoles = await context.ApiClient.DevPortals.GetTeamRoles(portalId, teamId);

        foreach (var serverTeamRole in serverTeamRoles)
        {
            if (serverTeamRole.EntityTypeName != Constants.ServicesRoleEntityTypeName)
            {
                continue;
            }

            var sourceTeamRole = FindPortalTeamRole(context, sourcePortalTeam.ApiProducts, serverTeamRole.EntityId, serverTeamRole.RoleName);
            if (sourceTeamRole != null)
            {
                processedRoles.Add((sourceTeamRole.ApiProduct, serverTeamRole.RoleName));

                differences.Add(
                    Difference.NoChange(
                        $"{context.ApiProductMap.GetSyncId(serverTeamRole.EntityId)} - {serverTeamRole.RoleName}",
                        serverTeamRole.Id,
                        serverTeamRole
                    )
                );
                continue;
            }

            differences.Add(Difference.Delete(serverTeamRole.Id, serverTeamRole));
        }

        foreach (var apiProduct in sourcePortalTeam.ApiProducts)
        {
            foreach (var roleName in apiProduct.Roles)
            {
                if (processedRoles.Contains((apiProduct.ApiProduct, roleName)))
                {
                    continue;
                }

                var role = MetadataMappingExtensions.ToApiModel(
                    apiProduct.ApiProduct,
                    context.ApiProductMap.GetIdOrDefault(apiProduct.ApiProduct, $"resolve://api-product/{apiProduct.ApiProduct}"),
                    roleName,
                    context.ApiClient.Region
                );

                differences.Add(Difference.Add($"{apiProduct.ApiProduct} - {roleName}", role));
            }
        }
    }

    private static async Task ComparePortalAuthSettings(SourceData sourceData, CompareContext context, string portalName, string? portalId = null)
    {
        var sourcePortalAuthSettings = sourceData.PortalAuthSettings[portalName];

        var teamIdMap = context.PortalTeamIdMap[portalName];

        if (portalId == null)
        {
            context.PortalAuthSettings[portalName] = Difference.Add(portalName, sourcePortalAuthSettings.ToApiModel());
            context.PortalTeamMappings[portalName] = Difference.Add(portalName, sourcePortalAuthSettings.ToTeamMappingsApiModel(teamIdMap));
            return;
        }

        var serverPortalAuthSettings = await context.ApiClient.DevPortals.GetAuthSettings(portalId);
        context.PortalAuthSettings[portalName] = Difference.UpdateOrNoChange(
            portalName,
            portalId,
            serverPortalAuthSettings,
            sourcePortalAuthSettings.ToApiModel()
        );

        var serverPortalTeamMappings = new DevPortalTeamMappingBody(await context.ApiClient.DevPortals.GetAuthTeamMappings(portalId));
        context.PortalTeamMappings[portalName] = Difference.UpdateOrNoChange(
            portalName,
            portalId,
            serverPortalTeamMappings,
            sourcePortalAuthSettings.ToTeamMappingsApiModel(teamIdMap)
        );
    }

    private static async Task ComparePortalAppearance(SourceData sourceData, CompareContext context, string portalName, string? portalId = null)
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

    private static async Task CompareApiProducts(SourceData sourceData, CompareContext context)
    {
        var toMatch = sourceData.ApiProducts.ToList();

        var serverApiProducts = await context.ApiClient.ApiProducts.GetAll();

        foreach (var serverApiProduct in serverApiProducts)
        {
            var sourceApiProduct = FindApiProduct(toMatch, serverApiProduct);
            if (sourceApiProduct != null)
            {
                toMatch.Remove(sourceApiProduct);

                var apiProduct = sourceApiProduct.ToApiModel(serverApiProduct.Id);
                context.ApiProducts.Add(Difference.UpdateOrNoChange(sourceApiProduct.SyncId, serverApiProduct.Id, serverApiProduct, apiProduct));

                var serverApiProductAssociation = new ApiProductAssociation(
                    serverApiProduct.PortalIds.Select(id => context.PortalMap.GetSyncId(id)).ToList()
                );

                var sourceApiProductAssociation = new ApiProductAssociation(
                    sourceData.PortalApiProducts.Where(p => p.Value.ApiProducts.Contains(sourceApiProduct.SyncId)).Select(p => p.Key).ToList()
                );

                context.ApiProductAssociations.Add(
                    sourceApiProduct.SyncId,
                    Difference.UpdateOrNoChange(
                        sourceApiProduct.SyncId,
                        serverApiProduct.Id,
                        serverApiProductAssociation,
                        sourceApiProductAssociation
                    )
                );

                context.ApiProductVersions.Add(sourceApiProduct.SyncId, []);
                context.ApiProductVersionSpecifications.Add(sourceApiProduct.SyncId, []);
                await CompareApiProductVersions(sourceData, context, sourceApiProduct.SyncId, apiProduct.Id);

                context.ApiProductDocuments.Add(sourceApiProduct.SyncId, []);
                await CompareApiProductDocuments(sourceData, context, sourceApiProduct.SyncId, apiProduct.Id);

                continue;
            }

            context.ApiProducts.Add(Difference.Delete(serverApiProduct.Id, serverApiProduct));
        }

        foreach (var sourceApiProduct in toMatch)
        {
            context.ApiProducts.Add(Difference.Add(sourceApiProduct.SyncId, sourceApiProduct.ToApiModel()));

            var sourceApiProductAssociation = new ApiProductAssociation(
                sourceData.PortalApiProducts.Where(p => p.Value.ApiProducts.Contains(sourceApiProduct.SyncId)).Select(p => p.Key).ToList()
            );

            context.ApiProductAssociations.Add(sourceApiProduct.SyncId, Difference.Add(sourceApiProduct.SyncId, sourceApiProductAssociation));

            context.ApiProductVersions.Add(sourceApiProduct.SyncId, []);
            context.ApiProductVersionSpecifications.Add(sourceApiProduct.SyncId, []);
            await CompareApiProductVersions(sourceData, context, sourceApiProduct.SyncId);

            context.ApiProductDocuments.Add(sourceApiProduct.SyncId, []);
            await CompareApiProductDocuments(sourceData, context, sourceApiProduct.SyncId);
        }
    }

    private static async Task CompareApiProductDocuments(
        SourceData sourceData,
        CompareContext context,
        string apiProductSyncId,
        string? apiProductId = null
    )
    {
        var toMatch = sourceData.ApiProductDocuments[apiProductSyncId].OrderBy(d => d.FullSlug).ToList();

        if (apiProductId == null)
        {
            foreach (var document in toMatch)
            {
                var contents = sourceData.ApiProductDocumentContents[apiProductSyncId][document.FullSlug];
                context.ApiProductDocuments[apiProductSyncId].Add(Difference.Add(document.FullSlug, document.ToApiModel(contents)));
            }

            return;
        }

        var serverApiDocuments = await context.ApiClient.ApiProductDocuments.GetAll(apiProductId);
        var documentDifferences = context.ApiProductDocuments[apiProductSyncId];

        var serverApiDocumentIdMap = serverApiDocuments.ToDictionary(d => d.Slug, d => d.Id);

        foreach (var serverApiDocument in serverApiDocuments)
        {
            var serverApiDocumentBody = await context.ApiClient.ApiProductDocuments.GetBody(apiProductId, serverApiDocument.Id) with
            {
                FullSlug = serverApiDocument.Slug,
            };

            var sourceApiProductDocument = toMatch.FirstOrDefault(v => v.FullSlug == serverApiDocument.Slug);

            if (sourceApiProductDocument != null)
            {
                toMatch.Remove(sourceApiProductDocument);

                var sourceContents = sourceData.ApiProductDocumentContents[apiProductSyncId][sourceApiProductDocument.FullSlug];

                var apiProductDocument = sourceApiProductDocument.ToApiModel(sourceContents, serverApiDocument.Id);
                if (apiProductDocument.TryResolveDocumentId(serverApiDocumentIdMap, out var resolvedDocument))
                {
                    apiProductDocument = resolvedDocument;
                }

                documentDifferences.Add(
                    Difference.UpdateOrNoChange(sourceApiProductDocument.FullSlug, serverApiDocument.Id, serverApiDocumentBody, apiProductDocument)
                );

                continue;
            }

            documentDifferences.Add(Difference.Delete(serverApiDocument.Id, serverApiDocumentBody));
        }

        foreach (var sourceApiProductDocument in toMatch)
        {
            var sourceContents = sourceData.ApiProductDocumentContents[apiProductSyncId][sourceApiProductDocument.FullSlug];
            documentDifferences.Add(Difference.Add(sourceApiProductDocument.FullSlug, sourceApiProductDocument.ToApiModel(sourceContents)));
        }
    }

    private static async Task CompareApiProductVersions(
        SourceData sourceData,
        CompareContext context,
        string apiProductSyncId,
        string? apiProductId = null
    )
    {
        var toMatch = sourceData.ApiProductVersions[apiProductSyncId].ToList();
        var versionDifferences = context.ApiProductVersions[apiProductSyncId];

        if (apiProductId == null)
        {
            foreach (var versionMetadata in toMatch)
            {
                versionDifferences.Add(Difference.Add(versionMetadata.SyncId, versionMetadata.ToApiModel()));

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

    private static async Task CompareApiProductVersionSpecification(
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

    private static PortalTeamApiProduct? FindPortalTeamRole(
        CompareContext context,
        IReadOnlyCollection<PortalTeamApiProduct> apiProducts,
        string apiProductId,
        string roleName
    )
    {
        var apiProductName = context.ApiProductMap.GetSyncId(apiProductId);
        var apiProduct = apiProducts.SingleOrDefault(p => p.ApiProduct == apiProductName);
        if (apiProduct == null)
        {
            return null;
        }

        return apiProduct.Roles.Contains(roleName) ? apiProduct : null;
    }

    private class CompareContext(KongApiClient apiClient)
    {
        public KongApiClient ApiClient { get; } = apiClient;
        public List<Difference<DevPortal>> Portals { get; } = new();
        public Dictionary<string, Difference<ApiProductAssociation>> ApiProductAssociations { get; } = new();
        public Dictionary<string, Difference<DevPortalAppearance>> PortalAppearances { get; } = new();
        public Dictionary<string, Difference<DevPortalAuthSettings>> PortalAuthSettings { get; } = new();
        public Dictionary<string, List<Difference<DevPortalTeam>>> PortalTeams { get; } = new();
        public Dictionary<string, Dictionary<string, List<Difference<DevPortalTeamRole>>>> PortalTeamRoles { get; } = new();
        public Dictionary<string, Difference<DevPortalTeamMappingBody>> PortalTeamMappings { get; } = new();
        public List<Difference<ApiProduct>> ApiProducts { get; } = new();
        public Dictionary<string, List<Difference<ApiProductVersion>>> ApiProductVersions { get; } = new();
        public Dictionary<string, Dictionary<string, Difference<ApiProductSpecification>>> ApiProductVersionSpecifications { get; } = new();
        public Dictionary<string, List<Difference<ApiProductDocumentBody>>> ApiProductDocuments { get; } = new();

        public SyncIdMap PortalMap =>
            new(Portals.Where(p => p is { SyncId: not null, Id: not null }).ToDictionary(kvp => kvp.SyncId!, kvp => kvp.Id!));

        public SyncIdMap ApiProductMap =>
            new(ApiProducts.Where(p => p is { SyncId: not null, Id: not null }).ToDictionary(kvp => kvp.SyncId!, kvp => kvp.Id!));

        public Dictionary<string, SyncIdMap> PortalTeamIdMap =>
            PortalTeams.ToDictionary(
                kvp => kvp.Key,
                kvp => new SyncIdMap(kvp.Value.Where(p => p is { SyncId: not null, Id: not null }).ToDictionary(kvp => kvp.SyncId!, kvp => kvp.Id!))
            );
    }
}
