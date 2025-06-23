using System.CommandLine;
using Kong.Portal.CLI.ApiClient;
using Kong.Portal.CLI.Services;
using Pastel;

namespace Kong.Portal.CLI.Commands;

internal class SyncCommand : Command
{
    private readonly SyncService _syncService;

    public SyncCommand(SyncService syncService)
        : base("sync")
    {
        _syncService = syncService;
        SetupCommand();
    }

    private void SetupCommand()
    {
        Description = "Syncs the filesystem to Kong";

        var inputDirectoryOption = new Option<string>("--input") { Description = "Input directory containing data to sync from", Required = true };
        var applyOption = new Option<bool>("--apply") { Description = "Applies changes to Konnect (do not set to perform a dry-run)" };
        var variablesOptions = new Option<string[]>("--var")
        {
            Description = "Variables to replace within the data (e.g. URL=http://example.com)",
            Arity = ArgumentArity.ZeroOrMore,
        };

        var konnectRegionOption = new Option<string>("--konnect-region")
        {
            Description = "Overrides the region when assigning Roles. Defaults based on --konnect-addr",
        };

        Options.Add(inputDirectoryOption);
        Options.Add(applyOption);
        Options.Add(variablesOptions);
        Options.Add(konnectRegionOption);

        SetAction(
            async (parseResult, cancellationToken) =>
            {
                var inputDirectory = parseResult.GetValue(inputDirectoryOption)!;
                var apply = parseResult.GetValue(applyOption);
                var variables = parseResult.GetValue(variablesOptions) ?? [];
                var token = parseResult.GetValue(GlobalOptions.TokenOption);
                var tokenFile = parseResult.GetValue(GlobalOptions.TokenFileOption);
                var konnectAddress = parseResult.GetValue(GlobalOptions.KonnectAddressOption)!;
                var konnectRegion = parseResult.GetValue(konnectRegionOption);
                var debug = parseResult.GetValue(GlobalOptions.Debug);
                return await Handle(inputDirectory, apply, variables, token, tokenFile, konnectAddress, konnectRegion, debug, cancellationToken);
            }
        );
    }

    private async Task<int> Handle(
        string inputDirectory,
        bool apply,
        string[] variables,
        string? token,
        FileInfo? tokenFile,
        string konnectAddress,
        string? konnectRegion,
        bool debug,
        CancellationToken cancellationToken
    )
    {
        //TODO: Cancellation token support
        try
        {
            var resolvedToken = TokenResolutionHelper.ResolveToken(token, tokenFile);

            var variablesDictionary = VariableHelper.Parse(variables);

            var region = string.IsNullOrEmpty(konnectRegion) ? KonnectRegionHelpers.FromAddress(konnectAddress) : konnectRegion;

            await _syncService.Sync(
                Path.GetFullPath(inputDirectory),
                variablesDictionary,
                apply,
                new KongApiClientOptions(resolvedToken, konnectAddress, region, debug)
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
