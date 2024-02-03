using System.CommandLine;
using System.IO.Abstractions;
using Kong.Portal.CLI.Commands;
using Kong.Portal.CLI.Config;
using Kong.Portal.CLI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

var configuration = new ConfigurationBuilder().AddEnvironmentVariables().Build();

var services = new ServiceCollection()
    .AddSingleton(configuration)
    .AddSingleton<IValidateOptions<KongOptions>, KongOptionsValidator>()
    .Configure<KongOptions>(configuration.GetSection("Kong"))
    .AddSingleton<DumpCommand>()
    .AddSingleton<IFileSystem, FileSystem>()
    .AddSingleton<IConsoleOutput, ConsoleOutput>()
    .AddSingleton<DumpService>()
    .BuildServiceProvider();

try
{
    var rootCommand = new CliRootCommand();
    rootCommand.AddCommand(services.GetRequiredService<DumpCommand>());
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
