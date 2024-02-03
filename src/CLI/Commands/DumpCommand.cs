using System.CommandLine;
using Kong.Portal.CLI.Services;

namespace Kong.Portal.CLI.Commands;

public class DumpCommand : Command
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
        var outputDirectoryOption = new Option<string>("--output", "Output directory") { IsRequired = true };

        AddOption(outputDirectoryOption);

        this.SetHandler(Handle, outputDirectoryOption);
    }

    private async Task<int> Handle(string outputDirectory)
    {
        await _dumpService.Dump(outputDirectory);

        return 0;
    }
}
