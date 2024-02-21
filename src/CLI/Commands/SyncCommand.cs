using System.CommandLine;
using Kong.Portal.CLI.ApiClient;
using Kong.Portal.CLI.Services;
using Pastel;

namespace Kong.Portal.CLI.Commands;

internal class SyncCommand : Command
{
    private readonly SyncService _syncService;

    public SyncCommand(SyncService syncService)
        : base("sync", "Syncs the filesystem to Kong")
    {
        _syncService = syncService;
        SetupCommand();
    }

    private void SetupCommand()
    {
        var inputDirectoryOption = new Option<string>("--input", "Input directory containing data to sync from") { IsRequired = true };
        var applyOption = new Option<bool>("--apply", "Applies changes to Konnect (do not set to perform a dry-run)");

        AddOption(inputDirectoryOption);
        AddOption(applyOption);

        this.SetHandler(
            Handle,
            inputDirectoryOption,
            applyOption,
            GlobalOptions.TokenOption,
            GlobalOptions.TokenFileOption,
            GlobalOptions.KonnectAddressOption
        );
    }

    private async Task<int> Handle(string inputDirectory, bool apply, string token, string tokenFile, string konnectAddress)
    {
        try
        {
            var resolvedToken = TokenResolutionHelper.ResolveToken(token, tokenFile);

            await _syncService.Sync(Path.GetFullPath(inputDirectory), apply, new KongApiClientOptions(resolvedToken, konnectAddress));

            return 0;
        }
        catch (OutputErrorException ex)
        {
            await Console.Error.WriteLineAsync(ex.Message.Pastel(ConsoleColor.Red));
            return 1;
        }
    }
}
