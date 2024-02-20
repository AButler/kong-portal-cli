using Kong.Portal.CLI.ApiClient;
using Kong.Portal.CLI.ApiClient.Models;
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

        var context = new SyncContext(apply);

        foreach (var difference in compareResult.ApiProducts)
        {
            await SyncApiProduct(context, difference);
        }
    }

    private async Task SyncApiProduct(SyncContext context, Difference<ApiProduct> difference)
    {
        consoleOutput.WriteLine($"  {difference.DifferenceType.ToSymbol()} {difference.Entity.Name}");

        if (context.Apply)
        {
            switch (difference.DifferenceType)
            {
                case DifferenceType.Add:
                    var apiProduct = await apiClient.ApiProducts.Create(difference.Entity);
                    context.ApiProductSyncIdMap.Add(difference.SyncId!, apiProduct.Id);
                    break;
                case DifferenceType.Update:
                    await apiClient.ApiProducts.Update(difference.Id!, difference.Entity);
                    context.ApiProductSyncIdMap.Add(difference.SyncId!, difference.Id!);
                    break;
                case DifferenceType.Delete:
                    await apiClient.ApiProducts.Delete(difference.Id!);
                    break;
            }
        }
    }

    private class SyncContext(bool apply)
    {
        public bool Apply { get; } = apply;
        public Dictionary<string, string> ApiProductSyncIdMap { get; } = new();
    }
}
