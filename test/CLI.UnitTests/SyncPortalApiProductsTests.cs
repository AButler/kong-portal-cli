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

        await syncService.Sync(@"c:\temp\input", testHost.ApiClientOptions, cancellationToken: TestContext.Current.CancellationToken);

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

        await syncService.Sync(@"c:\temp\input", testHost.ApiClientOptions, cancellationToken: TestContext.Current.CancellationToken);

        testHost.Then.Api.ApiProductShouldNotHaveBeenUpdated(productId);
    }

    [Fact]
    public async Task PortalApiProductsAreCreatedAndSynced()
    {
        using var testHost = new TestHost.TestHost();

        var portalId = Guid.NewGuid().ToString();

        testHost.Given.Api.AnExistingDevPortal(portalId: portalId, name: "default");

        await testHost.Given.File.AnExistingApiProduct(inputDirectory: @"c:\temp\input", name: "API Product 1", description: "API Product 1");
        await testHost.Given.File.AnExistingDevPortal(inputDirectory: @"c:\temp\input", portalName: "default");
        await testHost.Given.File.AnExistingDevPortalApiProduct(
            inputDirectory: @"c:\temp\input",
            portalName: "default",
            apiProductSyncId: "api-product-1"
        );
        await testHost.Given.File.AnExistingDevPortalTeam(@"c:\temp\input", "default", "Team 1", "Team One");
        await testHost.Given.File.AnExistingDevPortalTeamRole(@"c:\temp\input", "default", "Team 1", "api-product-1", "API Viewer");

        var syncService = testHost.GetRequiredService<SyncService>();

        await syncService.Sync(@"c:\temp\input", testHost.ApiClientOptions, cancellationToken: TestContext.Current.CancellationToken);

        var productId = await testHost.Then.Api.GetApiProductId("api-product-1");
        var teamId = await testHost.Then.Api.GetPortalTeamId(portalId, "Team 1");

        testHost.Then.Api.ApiProductShouldHaveBeenUpdated(productId);
        testHost.Then.Api.PortalTeamRoleShouldHaveBeenAssigned(portalId, teamId);
    }
}
