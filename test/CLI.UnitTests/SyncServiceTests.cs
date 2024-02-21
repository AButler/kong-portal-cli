using Kong.Portal.CLI;
using Kong.Portal.CLI.Services;

namespace CLI.UnitTests;

public class SyncServiceTests
{
    [Fact]
    public async Task NoSourceDataAndNoServerDataDoesNothing()
    {
        using var testHost = new TestHost.TestHost();

        var syncService = testHost.GetRequiredService<SyncService>();

        await syncService.Sync(@"c:\temp\input", true, testHost.ApiClientOptions);

        testHost.Then.Api.ShouldNotHaveReceivedAnyUpdates();
    }

    [Fact]
    public async Task DescriptionChangeUpdates()
    {
        using var testHost = new TestHost.TestHost();

        var productId = Guid.NewGuid().ToString();

        testHost.Given.Api.AnExistingApiProduct(
            productId,
            "API Product 1",
            "This is API Product 1",
            new Dictionary<string, string> { ["Foo"] = "Bar" }
        );

        await testHost.Given.File.AnExistingApiProduct(
            @"c:\temp\input",
            name: "API Product 1",
            description: "This is API Product One",
            labels: new Dictionary<string, string> { ["Foo"] = "Bar" }
        );

        var syncService = testHost.GetRequiredService<SyncService>();

        await syncService.Sync(@"c:\temp\input", true, testHost.ApiClientOptions);

        testHost.Then.Api.ApiProductShouldHaveBeenUpdated(productId);
    }

    [Fact]
    public async Task NewApiProductIsCreated()
    {
        using var testHost = new TestHost.TestHost();

        await testHost.Given.File.AnExistingApiProduct(
            @"c:\temp\input",
            name: "Api Product 1",
            description: "This is API Product One",
            labels: new Dictionary<string, string> { ["Foo"] = "Bar" }
        );

        var syncService = testHost.GetRequiredService<SyncService>();

        await syncService.Sync(@"c:\temp\input", true, testHost.ApiClientOptions);

        await testHost.Then.Api.ApiProductShouldHaveBeenCreated("api-product-1");
    }

    [Fact]
    public async Task NameChangeUpdates()
    {
        using var testHost = new TestHost.TestHost();

        var productId = Guid.NewGuid().ToString();

        testHost.Given.Api.AnExistingApiProduct(
            productId,
            "API Product 1",
            "This is API Product One",
            new Dictionary<string, string> { ["Foo"] = "Bar", [Constants.SyncIdLabel] = "api-product-1" }
        );

        await testHost.Given.File.AnExistingApiProduct(
            @"c:\temp\input",
            name: "API Product One",
            syncId: "api-product-1",
            description: "This is API Product One",
            labels: new Dictionary<string, string> { ["Foo"] = "Bar" }
        );

        var syncService = testHost.GetRequiredService<SyncService>();

        await syncService.Sync(@"c:\temp\input", true, testHost.ApiClientOptions);

        testHost.Then.Api.ApiProductShouldHaveBeenUpdated(productId);
    }

    [Fact]
    public async Task ApiProductAndVersionCreated()
    {
        using var testHost = new TestHost.TestHost();

        await testHost.Given.File.AnExistingApiProduct(@"c:\temp\input", syncId: "api-product-1");

        await testHost.Given.File.AnExistingApiProductVersion(
            @"c:\temp\input",
            apiProductSyncId: "api-product-1",
            syncId: "1.0.0",
            name: "1.0.0",
            publishStatus: "published",
            deprecated: false
        );

        var syncService = testHost.GetRequiredService<SyncService>();

        await syncService.Sync(@"c:\temp\input", true, testHost.ApiClientOptions);

        var apiProductId = await testHost.Then.Api.ApiProductShouldHaveBeenCreated("api-product-1");
        testHost.Then.Api.ApiProductVersionShouldHaveBeenCreated(apiProductId, "1.0.0");
    }
}
