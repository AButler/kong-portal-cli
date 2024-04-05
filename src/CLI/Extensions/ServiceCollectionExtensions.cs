using System.IO.Abstractions;
using Kong.Portal.CLI.ApiClient;
using Kong.Portal.CLI.Commands;
using Kong.Portal.CLI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Kong.Portal.CLI;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services.AddCoreApplication().AddSingleton<DumpCommand>().AddSingleton<SyncCommand>();
    }

    internal static IServiceCollection AddCoreApplication(this IServiceCollection services)
    {
        return services
            .AddSingleton<KongApiClientFactory>()
            .AddSingleton<IFileSystem, FileSystem>()
            .AddSingleton<IConsoleOutput, ConsoleOutput>()
            .AddSingleton<MetadataSerializer>()
            .AddSingleton<DumpService>()
            .AddSingleton<SyncService>()
            .AddSingleton<ComparerService>()
            .AddSingleton<SourceDirectoryReader>()
            .AddSingleton<IEnvironmentVariableReader, EnvironmentVariableReader>()
            .AddTransient<VariableHelper>();
    }
}
