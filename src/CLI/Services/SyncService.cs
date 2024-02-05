namespace Kong.Portal.CLI.Services;

internal class SyncService(SourceDirectoryReader sourceDirectoryReader, IConsoleOutput consoleOutput)
{
    public async Task Sync(string inputDirectory, bool apply)
    {
        consoleOutput.WriteLine($"Input Directory: {inputDirectory}");
        if (!apply)
        {
            consoleOutput.WriteLine(" ** Dry run only - no changes will be made **");
        }
        consoleOutput.WriteLine("Reading input directory...");
        var sourceData = await sourceDirectoryReader.Read(inputDirectory);

        consoleOutput.WriteLine("Syncing...");

        await Task.Delay(1);
    }
}
