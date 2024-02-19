using Kong.Portal.CLI.ApiClient;
using Kong.Portal.CLI.Services.Models;

namespace Kong.Portal.CLI.Services;

internal class SyncService(
    KongApiClient apiClient,
    SourceDirectoryReader sourceDirectoryReader,
    ComparerService comparerService,
    IConsoleOutput consoleOutput
)
{
    public async Task Sync(string inputDirectory, bool apply)
    {
        consoleOutput.WriteLine($"Input Directory: {inputDirectory}");
        if (!apply)
        {
            consoleOutput.WriteLine(" ** Dry run only - no changes will be made **");
        }

        consoleOutput.WriteLine("Reading input directory...");
        var sourceData = await sourceDirectoryReader.Read(inputDirectory);

        consoleOutput.WriteLine("Comparing...");
        var compareResult = await comparerService.Compare(sourceData);

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

        var context = new SyncContext();

        await SyncApiProducts(compareResult, context);
    }

    private async Task SyncApiProducts(CompareResult compareResult, SyncContext context)
    {
        foreach (var difference in compareResult.ApiProducts)
        {
            switch (difference.DifferenceType)
            {
                case DifferenceType.Add:
                    var apiProduct = await apiClient.ApiProducts.Create(difference.Entity.WithSyncIdLabel(difference.SyncId!));
                    context.ApiProductSyncIdMap.Add(difference.SyncId!, apiProduct.Id);
                    break;
                case DifferenceType.Update:
                    await apiClient.ApiProducts.Update(difference.Id!, difference.Entity.WithSyncIdLabel(difference.SyncId!));
                    context.ApiProductSyncIdMap.Add(difference.SyncId!, difference.Id!);
                    break;
                case DifferenceType.Delete:
                    await apiClient.ApiProducts.Delete(difference.Id!);
                    break;
            }
        }
    }

    private class SyncContext
    {
        public Dictionary<string, string> ApiProductSyncIdMap { get; } = new();
    }
}
