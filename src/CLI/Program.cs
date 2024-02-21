using System.CommandLine;
using System.IO.Abstractions;
using Kong.Portal.CLI.ApiClient;
using Kong.Portal.CLI.Commands;
using Kong.Portal.CLI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

var services = new ServiceCollection()
    .AddSingleton<DumpCommand>()
    .AddSingleton<SyncCommand>()
    .AddSingleton<KongApiClientFactory>()
    .AddSingleton<IFileSystem, FileSystem>()
    .AddSingleton<IConsoleOutput, ConsoleOutput>()
    .AddSingleton<MetadataSerializer>()
    .AddSingleton<DumpService>()
    .AddSingleton<SyncService>()
    .AddSingleton<ComparerService>()
    .AddSingleton<SourceDirectoryReader>()
    .BuildServiceProvider();

try
{
    var rootCommand = new CliRootCommand();
    rootCommand.AddGlobalOption(GlobalOptions.TokenOption);
    rootCommand.AddGlobalOption(GlobalOptions.KonnectAddressOption);

    rootCommand.AddCommand(services.GetRequiredService<DumpCommand>());
    rootCommand.AddCommand(services.GetRequiredService<SyncCommand>());

    return await rootCommand.InvokeAsync(args);
}
catch (OptionsValidationException ex)
{
    var consoleColor = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Error.WriteLine($"ERROR: {ex.Message}");
    Console.ForegroundColor = consoleColor;
    return 1;
}
