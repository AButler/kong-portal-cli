using System.Security.Cryptography;
using Kong.Portal.CLI.ApiClient;
using Kong.Portal.CLI.ApiClient.Models;
using Kong.Portal.CLI.Services.Models;
using Pastel;

namespace Kong.Portal.CLI.Services;

internal class SyncService(
    KongApiClientFactory apiClientFactory,
    SourceDirectoryReader sourceDirectoryReader,
    ComparerService comparerService,
    IConsoleOutput consoleOutput
)
{
    public async Task Sync(string inputDirectory, IReadOnlyDictionary<string, string> variables, bool apply, KongApiClientOptions apiClientOptions)
    {
        var apiClient = apiClientFactory.CreateClient(apiClientOptions);

        consoleOutput.WriteLine($"Input Directory: {inputDirectory}");
        if (!apply)
        {
            consoleOutput.WriteLine(" ** Dry run only - no changes will be made **".Pastel(ConsoleColor.Yellow));
        }

        consoleOutput.WriteLine("Reading input directory...");
        var sourceData = await sourceDirectoryReader.Read(inputDirectory, variables);

        consoleOutput.WriteLine("Comparing...");
        var compareResult = await comparerService.Compare(sourceData, apiClient);

        var validationErrors = compareResult.GetValidationErrors();
        if (validationErrors.Count > 0)
        {
            throw new SyncException(string.Join(Environment.NewLine, validationErrors));
        }

        if (apply)
        {
            consoleOutput.WriteLine("Applying changes...");
        }
        else
        {
            consoleOutput.WriteLine("Displaying changes...");
        }

        var context = new SyncContext(apiClient, apply);

        foreach (var difference in compareResult.Portals)
        {
            await SyncPortal(context, compareResult, difference);
        }

        foreach (var difference in compareResult.ApiProducts)
        {
            await SyncApiProduct(context, compareResult, difference);
        }

        consoleOutput.WriteLine("Done!");
    }

    private async Task SyncPortal(SyncContext context, CompareResult compareResult, Difference<DevPortal> difference)
    {
        consoleOutput.WriteDifference(difference, "Portal", difference.Entity.Name);

        if (context.Apply)
        {
            switch (difference.DifferenceType)
            {
                case DifferenceType.Add:
                    throw new InvalidOperationException("Cannot create Portals");
                case DifferenceType.Update:
                    await context.ApiClient.DevPortals.Update(difference.Id!, difference.Entity);
                    break;
                case DifferenceType.Delete:
                    throw new InvalidOperationException("Cannot delete Portals");
            }
        }

        if (difference.SyncId != null)
        {
            var portalAppearance = compareResult.PortalAppearances[difference.SyncId];
            await SyncPortalAppearance(context, difference.SyncId, portalAppearance);

            var portalAuthSettings = compareResult.PortalAuthSettings[difference.SyncId];
            await SyncPortalAuthSettings(context, difference.SyncId, portalAuthSettings);
        }
    }

    private async Task SyncPortalAuthSettings(SyncContext context, string portalName, Difference<DevPortalAuthSettings> difference)
    {
        consoleOutput.WriteDifference(difference, "Authentication Settings", 1);

        if (context.Apply)
        {
            switch (difference.DifferenceType)
            {
                case DifferenceType.Add:
                    throw new InvalidOperationException("Cannot create Portals");
                case DifferenceType.Update:
                    await context.ApiClient.DevPortals.UpdateAuthSettings(difference.Id!, difference.Entity);
                    break;
                case DifferenceType.Delete:
                    throw new InvalidOperationException("Cannot delete Portals");
            }
        }
    }

    private async Task SyncPortalAppearance(SyncContext context, string portalName, Difference<DevPortalAppearance> difference)
    {
        consoleOutput.WriteDifference(difference, "Appearance", 1);

        if (context.Apply)
        {
            switch (difference.DifferenceType)
            {
                case DifferenceType.Add:
                    throw new InvalidOperationException("Cannot create Portals");
                case DifferenceType.Update:
                    await context.ApiClient.DevPortals.UpdateAppearance(difference.Id!, difference.Entity);
                    break;
                case DifferenceType.Delete:
                    throw new InvalidOperationException("Cannot delete Portals");
            }
        }
    }

    private async Task SyncApiProduct(SyncContext context, CompareResult compareResult, Difference<ApiProduct> difference)
    {
        consoleOutput.WriteDifference(difference, "API Product", difference.Entity.Name);

        if (context.Apply)
        {
            switch (difference.DifferenceType)
            {
                case DifferenceType.Add:
                    var apiProduct = await context.ApiClient.ApiProducts.Create(difference.Entity);
                    context.ApiProductSyncIdMap.Add(difference.SyncId!, apiProduct.Id);
                    context.ApiProductVersionSyncIdMap.Add(difference.SyncId!, []);
                    context.ApiProductDocumentIdMap.Add(difference.SyncId!, []);
                    break;
                case DifferenceType.Update:
                    await context.ApiClient.ApiProducts.Update(difference.Id!, difference.Entity);
                    context.ApiProductSyncIdMap.Add(difference.SyncId!, difference.Id!);
                    context.ApiProductVersionSyncIdMap.Add(difference.SyncId!, []);
                    context.ApiProductDocumentIdMap.Add(difference.SyncId!, []);
                    break;
                case DifferenceType.Delete:
                    await context.ApiClient.ApiProducts.Delete(difference.Id!);
                    break;
                case DifferenceType.NoChange:
                    context.ApiProductSyncIdMap.Add(difference.SyncId!, difference.Id!);
                    context.ApiProductVersionSyncIdMap.Add(difference.SyncId!, []);
                    context.ApiProductDocumentIdMap.Add(difference.SyncId!, []);
                    break;
            }
        }

        if (difference.SyncId != null)
        {
            foreach (var apiProductVersion in compareResult.ApiProductVersions[difference.SyncId])
            {
                await SyncApiProductVersion(context, compareResult, difference.SyncId, apiProductVersion);
            }

            var orderedDocuments = compareResult.ApiProductDocuments[difference.SyncId].ToList().Order(new DocumentDifferenceComparer()).ToList();
            foreach (var apiProductDocument in orderedDocuments)
            {
                await SyncApiProductDocument(context, compareResult, difference.SyncId, apiProductDocument);
            }
        }
    }

    private async Task SyncApiProductDocument(
        SyncContext context,
        CompareResult compareResult,
        string apiProductSyncId,
        Difference<ApiProductDocumentBody> difference
    )
    {
        consoleOutput.WriteDifference(difference, "Document", difference.Entity.FullSlug, 1);

        if (context.Apply)
        {
            var apiProductId = context.ApiProductSyncIdMap[apiProductSyncId];
            var documentIdMap = context.ApiProductDocumentIdMap[apiProductSyncId];

            switch (difference.DifferenceType)
            {
                case DifferenceType.Add:
                    var createDocument = difference.Entity.ResolveDocumentId(documentIdMap);
                    var document = await context.ApiClient.ApiProductDocuments.Create(apiProductId, createDocument);
                    documentIdMap.Add(difference.Entity.FullSlug, document.Id);
                    break;
                case DifferenceType.Update:
                    var updateDocument = difference.Entity.ResolveDocumentId(documentIdMap);
                    await context.ApiClient.ApiProductDocuments.Update(apiProductId, difference.Id!, updateDocument);
                    documentIdMap.Add(difference.Entity.FullSlug, difference.Id!);
                    break;
                case DifferenceType.Delete:
                    await context.ApiClient.ApiProductDocuments.Delete(apiProductId, difference.Id!);
                    break;
                case DifferenceType.NoChange:
                    documentIdMap.Add(difference.Entity.FullSlug, difference.Id!);
                    break;
            }
        }
    }

    private async Task SyncApiProductVersion(
        SyncContext context,
        CompareResult compareResult,
        string apiProductSyncId,
        Difference<ApiProductVersion> difference
    )
    {
        consoleOutput.WriteDifference(difference, "Version", difference.Entity.Name, 1);

        if (context.Apply)
        {
            var apiProductId = context.ApiProductSyncIdMap[apiProductSyncId];
            switch (difference.DifferenceType)
            {
                case DifferenceType.Add:
                    var apiProductVersion = await context.ApiClient.ApiProductVersions.Create(apiProductId, difference.Entity);
                    context.ApiProductVersionSyncIdMap[apiProductSyncId].Add(difference.SyncId!, apiProductVersion.Id);
                    break;
                case DifferenceType.Update:
                    await context.ApiClient.ApiProductVersions.Update(apiProductId, difference.Id!, difference.Entity);
                    context.ApiProductVersionSyncIdMap[apiProductSyncId].Add(difference.SyncId!, difference.Id!);
                    break;
                case DifferenceType.Delete:
                    await context.ApiClient.ApiProductVersions.Delete(apiProductId, difference.Id!);
                    break;
                case DifferenceType.NoChange:
                    context.ApiProductVersionSyncIdMap[apiProductSyncId].Add(difference.SyncId!, difference.Id!);
                    break;
            }
        }

        if (
            difference.SyncId != null
            && compareResult.ApiProductVersionSpecifications[apiProductSyncId].TryGetValue(difference.SyncId, out var specification)
        )
        {
            await SyncApiProductVersionSpecification(context, apiProductSyncId, difference.SyncId, specification);
        }
    }

    private async Task SyncApiProductVersionSpecification(
        SyncContext context,
        string apiProductSyncId,
        string apiProductVersionSyncId,
        Difference<ApiProductSpecification> difference
    )
    {
        consoleOutput.WriteDifference(difference, "Specification", difference.Entity.Name, 2);

        if (context.Apply)
        {
            var apiProductId = context.ApiProductSyncIdMap[apiProductSyncId];
            var apiProductVersionId = context.ApiProductVersionSyncIdMap[apiProductSyncId][apiProductVersionSyncId];
            switch (difference.DifferenceType)
            {
                case DifferenceType.Add:
                    await context.ApiClient.ApiProductVersions.CreateSpecification(apiProductId, apiProductVersionId, difference.Entity);
                    break;
                case DifferenceType.Update:
                    await context.ApiClient.ApiProductVersions.UpdateSpecification(
                        apiProductId,
                        apiProductVersionId,
                        difference.Id!,
                        difference.Entity
                    );
                    break;
                case DifferenceType.Delete:
                    await context.ApiClient.ApiProductVersions.DeleteSpecification(apiProductId, apiProductVersionId, difference.Id!);
                    break;
            }
        }
    }

    private class SyncContext(KongApiClient apiClient, bool apply)
    {
        public KongApiClient ApiClient { get; } = apiClient;
        public bool Apply { get; } = apply;
        public Dictionary<string, string> ApiProductSyncIdMap { get; } = new();
        public Dictionary<string, Dictionary<string, string>> ApiProductVersionSyncIdMap { get; } = new();
        public Dictionary<string, Dictionary<string, string>> ApiProductDocumentIdMap { get; } = new();
    }
}

internal class DocumentDifferenceComparer : IComparer<Difference<ApiProductDocumentBody>>
{
    public int Compare(Difference<ApiProductDocumentBody>? x, Difference<ApiProductDocumentBody>? y)
    {
        if (ReferenceEquals(x, y))
        {
            return 0;
        }

        if (ReferenceEquals(null, y))
        {
            return 1;
        }

        if (ReferenceEquals(null, x))
        {
            return -1;
        }

        // Sort first by DifferenceType.Delete (opposite of sort order so compare y to x)
        var differenceTypeCompare = y.DifferenceType.CompareTo(x.DifferenceType);
        if (differenceTypeCompare != 0)
        {
            return differenceTypeCompare;
        }

        if (x.DifferenceType == DifferenceType.Delete)
        {
            return string.Compare(y.Entity.FullSlug, x.Entity.FullSlug, StringComparison.InvariantCulture);
        }

        return string.Compare(x.Entity.FullSlug, y.Entity.FullSlug, StringComparison.InvariantCulture);
    }
}
