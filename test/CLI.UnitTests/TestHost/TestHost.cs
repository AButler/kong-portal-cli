using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using CLI.UnitTests.Services;
using Flurl.Http.Testing;
using Kong.Portal.CLI.ApiClient;
using Kong.Portal.CLI.Commands;
using Kong.Portal.CLI.Config;
using Kong.Portal.CLI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CLI.UnitTests.TestHost;

public class TestHost : IDisposable
{
    private readonly HttpTest _httpTest;
    private readonly ServiceProvider _services;

    public IFileSystem FileSystem { get; }
    public GivenSteps Given { get; }
    public ThenSteps Then { get; }

    public TestHost()
    {
        _httpTest = new HttpTest();
        _services = ConfigureServices();

        FileSystem = _services.GetRequiredService<IFileSystem>();
        Given = _services.GetRequiredService<GivenSteps>();
        Then = _services.GetRequiredService<ThenSteps>();
    }

    public T? GetService<T>() => _services.GetService<T>();

    public T GetRequiredService<T>()
        where T : notnull => _services.GetRequiredService<T>();

    private static ServiceProvider ConfigureServices()
    {
        var mockFileSystem = new MockFileSystem();

        var services = new ServiceCollection()
            .AddSingleton<IValidateOptions<KongOptions>, KongOptionsValidator>()
            .Configure<KongOptions>(options => options.Token = "Test_Kong_Token")
            .AddSingleton<KongApiClient>()
            .AddSingleton<IFileSystem>(mockFileSystem)
            .AddSingleton<IConsoleOutput, NullConsoleOutput>()
            .AddSingleton<DumpService>()
            .AddSingleton<GivenSteps>()
            .AddSingleton<ThenSteps>()
            .AddSingleton<DumpedFileSteps>()
            //.AddSingleton<SyncService>()
            .BuildServiceProvider();

        return services;
    }

    public void Dispose()
    {
        _httpTest.Dispose();
        _services.Dispose();
    }
}
