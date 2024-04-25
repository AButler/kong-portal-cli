using Kong.Portal.CLI;

namespace CLI.UnitTests;

public class SyncPortalApiProductsTests
{
    [Fact]
    public async Task PortalApiProductsAreSynced()
    {
        using var testHost = new TestHost.TestHost();

        var portalId = Guid.NewGuid().ToString();
        var productId = Guid.NewGuid().ToString();

        var labels = new Dictionary<string, string> { [Constants.SyncIdLabel] = "api-product-1" };

        testHost.Given.Api.AnExistingDevPortal(portalId: portalId, name: "default");
        testHost.Given.Api.AnExistingApiProduct(productId: productId, name: "API Product 1", description: "API Product 1", labels: labels);

        await testHost.Given.File.AnExistingApiProduct(inputDirectory: @"c:\temp\input", name: "API Product 1", description: "API Product 1");
        await testHost.Given.File.AnExistingDevPortal(inputDirectory: @"c:\temp\input", portalName: "default");
        await testHost.Given.File.AnExistingDevPortalApiProduct(
            inputDirectory: @"c:\temp\input",
            portalName: "default",
            apiProductSyncId: "api-product-1"
        );

        var syncService = testHost.GetRequiredService<SyncService>();

        await syncService.Sync(@"c:\temp\input", new Dictionary<string, string>(), true, testHost.ApiClientOptions);

        testHost.Then.Api.ApiProductShouldHaveBeenUpdated(productId);
    }

    [Fact]
    public async Task PortalApiProductsAreInSync()
    {
        using var testHost = new TestHost.TestHost();

        var portalId = Guid.NewGuid().ToString();
        var productId = Guid.NewGuid().ToString();

        var labels = new Dictionary<string, string> { [Constants.SyncIdLabel] = "api-product-1" };

        testHost.Given.Api.AnExistingDevPortal(portalId: portalId, name: "default");
        testHost.Given.Api.AnExistingApiProduct(
            productId: productId,
            name: "API Product 1",
            description: "API Product 1",
            portalIds: new[] { portalId },
            labels: labels
        );

        await testHost.Given.File.AnExistingApiProduct(inputDirectory: @"c:\temp\input", name: "API Product 1", description: "API Product 1");
        await testHost.Given.File.AnExistingDevPortal(inputDirectory: @"c:\temp\input", portalName: "default");
        await testHost.Given.File.AnExistingDevPortalApiProduct(
            inputDirectory: @"c:\temp\input",
            portalName: "default",
            apiProductSyncId: "api-product-1"
        );

        var syncService = testHost.GetRequiredService<SyncService>();

        await syncService.Sync(@"c:\temp\input", new Dictionary<string, string>(), true, testHost.ApiClientOptions);

        testHost.Then.Api.ApiProductShouldNotHaveBeenUpdated(productId);
    }
}
