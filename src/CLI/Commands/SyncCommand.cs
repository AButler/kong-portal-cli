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
        var variablesOptions = new Option<string[]>("--var", "Variables to replace within the data (e.g. URL=http://example.com)")
        {
            Arity = ArgumentArity.ZeroOrMore
        };

        AddOption(inputDirectoryOption);
        AddOption(applyOption);
        AddOption(variablesOptions);

        this.SetHandler(
            Handle,
            inputDirectoryOption,
            applyOption,
            variablesOptions,
            GlobalOptions.TokenOption,
            GlobalOptions.TokenFileOption,
            GlobalOptions.KonnectAddressOption,
            GlobalOptions.Debug
        );
    }

    private async Task<int> Handle(
        string inputDirectory,
        bool apply,
        string[] variables,
        string? token,
        FileInfo? tokenFile,
        string konnectAddress,
        bool debug
    )
    {
        try
        {
            var resolvedToken = TokenResolutionHelper.ResolveToken(token, tokenFile);

            var variablesDictionary = VariableHelper.Parse(variables);

            await _syncService.Sync(
                Path.GetFullPath(inputDirectory),
                variablesDictionary,
                apply,
                new KongApiClientOptions(resolvedToken, konnectAddress, debug)
            );

            return 0;
        }
        catch (OutputErrorException ex)
        {
            await Console.Error.WriteLineAsync(ex.Message.Pastel(ConsoleColor.Red));
            return 1;
        }
    }
}
