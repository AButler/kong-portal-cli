using System.CommandLine;
using Kong.Portal.CLI;
using Kong.Portal.CLI.Commands;
using Microsoft.Extensions.DependencyInjection;
using Pastel;

var services = new ServiceCollection().AddApplication().BuildServiceProvider();

try
{
    var rootCommand = new CliRootCommand();
    rootCommand.AddGlobalOption(GlobalOptions.TokenOption);
    rootCommand.AddGlobalOption(GlobalOptions.TokenFileOption);
    rootCommand.AddGlobalOption(GlobalOptions.KonnectAddressOption);
    rootCommand.AddGlobalOption(GlobalOptions.Debug);

    rootCommand.AddCommand(services.GetRequiredService<DumpCommand>());
    rootCommand.AddCommand(services.GetRequiredService<SyncCommand>());

    return await rootCommand.InvokeAsync(args);
}
catch (OutputErrorException ex)
{
    Console.Error.WriteLine($"ERROR: {ex.Message}".Pastel(ConsoleColor.Red));

    return 1;
}
