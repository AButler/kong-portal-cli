using Kong.Portal.CLI;
using Kong.Portal.CLI.Commands;
using Microsoft.Extensions.DependencyInjection;
using Pastel;

var services = new ServiceCollection().AddApplication().BuildServiceProvider();

try
{
    var rootCommand = new CliRootCommand();
    rootCommand.Options.Add(GlobalOptions.TokenOption);
    rootCommand.Options.Add(GlobalOptions.TokenFileOption);
    rootCommand.Options.Add(GlobalOptions.KonnectAddressOption);
    rootCommand.Options.Add(GlobalOptions.Debug);

    rootCommand.Subcommands.Add(services.GetRequiredService<DumpCommand>());
    rootCommand.Subcommands.Add(services.GetRequiredService<SyncCommand>());

    return await rootCommand.Parse(args).InvokeAsync();
}
catch (OutputErrorException ex)
{
    Console.Error.WriteLine($"ERROR: {ex.Message}".Pastel(ConsoleColor.Red));

    return 1;
}
