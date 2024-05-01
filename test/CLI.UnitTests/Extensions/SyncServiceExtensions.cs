using Kong.Portal.CLI.ApiClient;

namespace CLI.UnitTests;

internal static class SyncServiceExtensions
{
    public static async Task Sync(this SyncService syncService, string inputDirectory, KongApiClientOptions apiClientOptions)
    {
        await syncService.Sync(inputDirectory, new Dictionary<string, string>(), true, apiClientOptions);
    }
}
