using CLI.UnitTests.TestHost;
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
            productId: productId,
            name: "API Product 1",
            description: "This is API Product 1",
            labels: new Dictionary<string, string> { ["Foo"] = "Bar" }
        );

        await testHost.Given.File.AnExistingApiProduct(
            inputDirectory: @"c:\temp\input",
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
            inputDirectory: @"c:\temp\input",
            name: "Api Product 1",
            description: "This is API Product One",
            labels: new Dictionary<string, string> { ["Foo"] = "Bar" }
        );

        var syncService = testHost.GetRequiredService<SyncService>();

        await syncService.Sync(@"c:\temp\input", true, testHost.ApiClientOptions);

        testHost.Then.Api.ApiProductShouldHaveBeenCreated("api-product-1");
    }

    [Fact]
    public async Task NameChangeUpdates()
    {
        using var testHost = new TestHost.TestHost();

        var productId = Guid.NewGuid().ToString();

        testHost.Given.Api.AnExistingApiProduct(
            productId: productId,
            name: "API Product 1",
            description: "This is API Product One",
            labels: new Dictionary<string, string> { ["Foo"] = "Bar", [Constants.SyncIdLabel] = "api-product-1" }
        );

        await testHost.Given.File.AnExistingApiProduct(
            inputDirectory: @"c:\temp\input",
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

        await testHost.Given.File.AnExistingApiProduct(inputDirectory: @"c:\temp\input", syncId: "api-product-1");

        await testHost.Given.File.AnExistingApiProductVersion(
            inputDirectory: @"c:\temp\input",
            apiProductSyncId: "api-product-1",
            syncId: "1.0.0",
            name: "1.0.0",
            publishStatus: "published",
            deprecated: false
        );

        var syncService = testHost.GetRequiredService<SyncService>();

        await syncService.Sync(@"c:\temp\input", true, testHost.ApiClientOptions);

        testHost.Then.Api.ApiProductShouldHaveBeenCreated("api-product-1");

        var apiProductId = await testHost.Then.Api.GetApiProductId("api-product-1");
        testHost.Then.Api.ApiProductVersionShouldHaveBeenCreated(apiProductId);
    }

    [Fact]
    public async Task PortalSettingsAreSynced()
    {
        using var testHost = new TestHost.TestHost();

        var portalId = Guid.NewGuid().ToString();

        testHost.Given.Api.AnExistingDevPortal(portalId: portalId, name: "default");

        await testHost.Given.File.AnExistingDevPortal(
            inputDirectory: @"c:\temp\input",
            name: "default",
            customDomain: "dev-portal.com",
            customClientDomain: "client.dev-portal.com",
            isPublic: true,
            rbacEnabled: true,
            autoApproveDevelopers: true,
            autoApproveApplications: true
        );

        await testHost.Given.File.AnExistingDevPortalAppearance(inputDirectory: @"c:\temp\input", name: "default");
        await testHost.Given.File.AnExistingDevPortalAuthSettings(inputDirectory: @"c:\temp\input", name: "default");

        var syncService = testHost.GetRequiredService<SyncService>();

        await syncService.Sync(@"c:\temp\input", true, testHost.ApiClientOptions);

        testHost.Then.Api.PortalShouldHaveBeenUpdated(portalId);
    }

    [Fact]
    public async Task PortalAppearanceIsSynced()
    {
        using var testHost = new TestHost.TestHost();

        var portalId = Guid.NewGuid().ToString();

        testHost.Given.Api.AnExistingDevPortal(portalId: portalId, name: "default");

        await testHost.Given.File.AnExistingDevPortal(inputDirectory: @"c:\temp\input", name: "default");
        await testHost.Given.File.AnExistingDevPortalAppearance(inputDirectory: @"c:\temp\input", name: "default", themeName: "custom");
        await testHost.Given.File.AnExistingDevPortalAuthSettings(inputDirectory: @"c:\temp\input", name: "default");

        var syncService = testHost.GetRequiredService<SyncService>();

        await syncService.Sync(@"c:\temp\input", true, testHost.ApiClientOptions);

        testHost.Then.Api.PortalAppearanceShouldHaveBeenUpdated(portalId);
    }

    [Fact]
    public async Task NewApiDocumentAdded()
    {
        using var testHost = new TestHost.TestHost();

        await testHost.Given.File.AnExistingApiProduct(inputDirectory: @"c:\temp\input", syncId: "api-product-1");
        await testHost.Given.File.AnExistingApiProductDocument(
            inputDirectory: @"c:\temp\input",
            apiProductSyncId: "api-product-1",
            title: "New Document",
            slug: "new-doc",
            fullSlug: "new-doc",
            status: "published"
        );

        var syncService = testHost.GetRequiredService<SyncService>();

        await syncService.Sync(@"c:\temp\input", true, testHost.ApiClientOptions);

        var apiProductId = await testHost.Then.Api.GetApiProductId("api-product-1");
        testHost.Then.Api.ApiProductDocumentShouldHaveBeenCreated(apiProductId);
    }

    [Fact]
    public async Task PortalAuthSettingsAreSynced()
    {
        using var testHost = new TestHost.TestHost();

        var portalId = Guid.NewGuid().ToString();

        testHost.Given.Api.AnExistingDevPortal(portalId: portalId, name: "default");

        await testHost.Given.File.AnExistingDevPortal(inputDirectory: @"c:\temp\input", name: "default");
        await testHost.Given.File.AnExistingDevPortalAppearance(inputDirectory: @"c:\temp\input", name: "default");
        await testHost.Given.File.AnExistingDevPortalAuthSettings(
            inputDirectory: @"c:\temp\input",
            name: "default",
            oidcAuthEnabled: true,
            oidcConfig: new OidcAuthSettings(
                "MyIssuer",
                "MyClientId",
                ["openid", "profile", "email"],
                new OidcClaimMappings("name", "email", "groups")
            )
        );

        var syncService = testHost.GetRequiredService<SyncService>();

        await syncService.Sync(@"c:\temp\input", true, testHost.ApiClientOptions);

        testHost.Then.Api.PortalAuthSettingsShouldHaveBeenUpdated(portalId);
    }
}
