using System.IO.Abstractions;
using Kong.Portal.CLI.ApiClient;
using Kong.Portal.CLI.Commands;
using Kong.Portal.CLI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Kong.Portal.CLI;

internal static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddApplication()
        {
            return services.AddCoreApplication().AddSingleton<DumpCommand>().AddSingleton<SyncCommand>();
        }

        internal IServiceCollection AddCoreApplication()
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
}
