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
    public async Task Sync(string inputDirectory, bool apply, KongApiClientOptions apiClientOptions)
    {
        var apiClient = apiClientFactory.CreateClient(apiClientOptions);

        consoleOutput.WriteLine($"Input Directory: {inputDirectory}");
        if (!apply)
        {
            consoleOutput.WriteLine(" ** Dry run only - no changes will be made **".Pastel(ConsoleColor.Yellow));
        }

        consoleOutput.WriteLine("Reading input directory...");
        var sourceData = await sourceDirectoryReader.Read(inputDirectory);

        consoleOutput.WriteLine("Comparing...");
        var compareResult = await comparerService.Compare(sourceData, apiClient);

        if (!compareResult.AnyChanges)
        {
            consoleOutput.WriteLine("No changes required");
            return;
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

        foreach (var difference in compareResult.ApiProducts)
        {
            await SyncApiProduct(context, compareResult, difference);
        }

        consoleOutput.WriteLine("Done!");
    }

    private async Task SyncApiProduct(SyncContext context, CompareResult compareResult, Difference<ApiProduct> difference)
    {
        consoleOutput.WriteLine($"  {difference.DifferenceType.ToSymbol()} {difference.Entity.Name}");

        if (context.Apply)
        {
            switch (difference.DifferenceType)
            {
                case DifferenceType.Add:
                    var apiProduct = await context.ApiClient.ApiProducts.Create(difference.Entity);
                    context.ApiProductSyncIdMap.Add(difference.SyncId!, apiProduct.Id);
                    context.ApiProductVersionSyncIdMap.Add(difference.SyncId!, []);
                    break;
                case DifferenceType.Update:
                    await context.ApiClient.ApiProducts.Update(difference.Id!, difference.Entity);
                    context.ApiProductSyncIdMap.Add(difference.SyncId!, difference.Id!);
                    context.ApiProductVersionSyncIdMap.Add(difference.SyncId!, []);
                    break;
                case DifferenceType.Delete:
                    await context.ApiClient.ApiProducts.Delete(difference.Id!);
                    break;
                case DifferenceType.NoChange:
                    context.ApiProductSyncIdMap.Add(difference.SyncId!, difference.Id!);
                    context.ApiProductVersionSyncIdMap.Add(difference.SyncId!, []);
                    break;
            }
        }

        if (difference.SyncId != null)
        {
            foreach (var apiProductVersion in compareResult.ApiProductVersions[difference.SyncId])
            {
                await SyncApiProductVersion(context, compareResult, difference.SyncId, apiProductVersion);
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
        consoleOutput.WriteLine($"    {difference.DifferenceType.ToSymbol()} {difference.Entity.Name}");

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
        consoleOutput.WriteLine($"      {difference.DifferenceType.ToSymbol()} {difference.Entity.Name}");

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
    }
}
