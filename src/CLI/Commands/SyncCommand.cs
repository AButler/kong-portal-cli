using System.CommandLine;
using Kong.Portal.CLI.Services;

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
        var inputDirectory = new Option<string>("--input", "Input directory") { IsRequired = true };
        var apply = new Option<bool>("--apply", "Apply changes");

        AddOption(inputDirectory);
        AddOption(apply);

        this.SetHandler(Handle, inputDirectory, apply);
    }

    private async Task<int> Handle(string inputDirectory, bool apply)
    {
        await _syncService.Sync(Path.GetFullPath(inputDirectory), apply);

        return 0;
    }
}
