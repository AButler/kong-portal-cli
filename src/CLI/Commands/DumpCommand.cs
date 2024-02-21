using System.CommandLine;
using Kong.Portal.CLI.ApiClient;
using Kong.Portal.CLI.Services;

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

        this.SetHandler(Handle, outputDirectoryOption, GlobalOptions.TokenOption, GlobalOptions.KonnectAddressOption);
    }

    private async Task<int> Handle(string outputDirectory, string token, string konnectAddress)
    {
        await _dumpService.Dump(Path.GetFullPath(outputDirectory), new KongApiClientOptions(token, konnectAddress));

        return 0;
    }
}
