using System.CommandLine;
using Kong.Portal.CLI.ApiClient;
using Kong.Portal.CLI.Services;
using Pastel;

namespace Kong.Portal.CLI.Commands;

internal class DumpCommand : Command
{
    private readonly DumpService _dumpService;

    public DumpCommand(DumpService dumpService)
        : base("dump", "Dumps an existing portal to disk")
    {
        _dumpService = dumpService;
        SetupCommand();
    }

    private void SetupCommand()
    {
        var outputDirectoryOption = new Option<string>("--output", "Directory to dump Konnect data to") { IsRequired = true };

        AddOption(outputDirectoryOption);

        this.SetHandler(
            Handle,
            outputDirectoryOption,
            GlobalOptions.TokenOption,
            GlobalOptions.TokenFileOption,
            GlobalOptions.KonnectAddressOption,
            GlobalOptions.Debug
        );
    }

    private async Task<int> Handle(string outputDirectory, string? token, FileInfo? tokenFile, string konnectAddress, bool debug)
    {
        try
        {
            var resolvedToken = TokenResolutionHelper.ResolveToken(token, tokenFile);

            await _dumpService.Dump(
                Path.GetFullPath(outputDirectory),
                new KongApiClientOptions(resolvedToken, konnectAddress, DebugLoggingEnabled: debug)
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
