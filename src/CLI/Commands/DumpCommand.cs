using System.CommandLine;
using Kong.Portal.CLI.ApiClient;
using Kong.Portal.CLI.Services;
using Pastel;

namespace Kong.Portal.CLI.Commands;

internal class DumpCommand : Command
{
    private readonly DumpService _dumpService;

    public DumpCommand(DumpService dumpService)
        : base("dump")
    {
        _dumpService = dumpService;
        SetupCommand();
    }

    private void SetupCommand()
    {
        Description = "Dumps an existing portal to disk";

        var outputDirectoryOption = new Option<string>("--output") { Description = "Directory to dump Konnect data to", Required = true };

        Options.Add(outputDirectoryOption);

        SetAction(
            async (parseResult, cancellationToken) =>
            {
                var outputDirectory = parseResult.GetValue(outputDirectoryOption)!;
                var token = parseResult.GetValue(GlobalOptions.TokenOption);
                var tokenFile = parseResult.GetValue(GlobalOptions.TokenFileOption);
                var konnectAddress = parseResult.GetValue(GlobalOptions.KonnectAddressOption)!;
                var debug = parseResult.GetValue(GlobalOptions.Debug);
                return await Handle(outputDirectory, token, tokenFile, konnectAddress, debug, cancellationToken);
            }
        );
    }

    private async Task<int> Handle(
        string outputDirectory,
        string? token,
        FileInfo? tokenFile,
        string konnectAddress,
        bool debug,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var resolvedToken = TokenResolutionHelper.ResolveToken(token, tokenFile);

            await _dumpService.Dump(
                Path.GetFullPath(outputDirectory),
                new KongApiClientOptions(resolvedToken, konnectAddress, DebugLoggingEnabled: debug),
                cancellationToken
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
