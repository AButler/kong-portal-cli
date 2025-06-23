using Kong.Portal.CLI.ApiClient;

namespace CLI.UnitTests;

internal static class SyncServiceExtensions
{
    public static async Task Sync(
        this SyncService syncService,
        string inputDirectory,
        KongApiClientOptions apiClientOptions,
        CancellationToken cancellationToken = default
    )
    {
        await syncService.Sync(inputDirectory, new Dictionary<string, string>(), true, apiClientOptions, cancellationToken);
    }
}
