using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using CLI.UnitTests.Services;
using Flurl.Http.Testing;
using Kong.Portal.CLI;
using Kong.Portal.CLI.ApiClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CLI.UnitTests.TestHost;

internal class TestHost : IDisposable
{
    private readonly HttpTest _httpTest;
    private readonly ServiceProvider _services;

    public IFileSystem FileSystem { get; }
    public GivenSteps Given { get; }
    public ThenSteps Then { get; }
    public KongApiClientOptions ApiClientOptions { get; }

    public TestHost()
    {
        _httpTest = new HttpTest();
        _services = ConfigureServices();

        FileSystem = _services.GetRequiredService<IFileSystem>();
        Given = _services.GetRequiredService<GivenSteps>();
        Then = _services.GetRequiredService<ThenSteps>();
        ApiClientOptions = _services.GetRequiredService<KongApiClientOptions>();
    }

    public T? GetService<T>() => _services.GetService<T>();

    public T GetRequiredService<T>()
        where T : notnull => _services.GetRequiredService<T>();

    private static ServiceProvider ConfigureServices()
    {
        var mockFileSystem = new MockFileSystem();

        var services = new ServiceCollection()
            .AddCoreApplication()
            .RemoveAll<IFileSystem>()
            .RemoveAll<IConsoleOutput>()
            .RemoveAll<IEnvironmentVariableReader>()
            .AddSingleton<IFileSystem>(mockFileSystem)
            .AddSingleton<IConsoleOutput, NullConsoleOutput>()
            .AddSingleton<IEnvironmentVariableReader, MockEnvironmentVariableReader>();

        ConfigureTestServices(services);

        return services.BuildServiceProvider();
    }

    private static void ConfigureTestServices(IServiceCollection services)
    {
        services
            .AddSingleton<DumpService>()
            .AddSingleton<GivenSteps>()
            .AddSingleton<ThenSteps>()
            .AddSingleton<ApiGivenSteps>()
            .AddSingleton<FileGivenSteps>()
            .AddSingleton<DumpedFileThenSteps>()
            .AddSingleton<ApiThenSteps>()
            .AddSingleton<KongApiClientOptions>(_ => new KongApiClientOptions("KONG-API-TOKEN", KongRegions.EuRegion));
    }

    public void Dispose()
    {
        _httpTest.Dispose();
        _services.Dispose();
    }
}
