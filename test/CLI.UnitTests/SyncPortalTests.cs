using CLI.UnitTests.TestHost;

namespace CLI.UnitTests;

public class SyncPortalTests
{
    [Fact]
    public async Task PortalSettingsAreSynced()
    {
        using var testHost = new TestHost.TestHost();

        var portalId = Guid.NewGuid().ToString();

        testHost.Given.Api.AnExistingDevPortal(portalId: portalId, name: "default");

        await testHost.Given.File.AnExistingDevPortal(
            inputDirectory: @"c:\temp\input",
            portalName: "default",
            customDomain: "dev-portal.com",
            customClientDomain: "client.dev-portal.com",
            isPublic: true,
            rbacEnabled: true,
            autoApproveDevelopers: true,
            autoApproveApplications: true
        );

        await testHost.Given.File.AnExistingDevPortalAppearance(inputDirectory: @"c:\temp\input", portalName: "default");
        await testHost.Given.File.AnExistingDevPortalAuthSettings(inputDirectory: @"c:\temp\input", portalName: "default");
        await testHost.Given.File.ExistingDevPortalTeams(inputDirectory: @"c:\temp\input", portalName: "default");

        var syncService = testHost.GetRequiredService<SyncService>();

        await syncService.Sync(@"c:\temp\input", new Dictionary<string, string>(), true, testHost.ApiClientOptions);

        testHost.Then.Api.PortalShouldHaveBeenUpdated(portalId);
    }

    [Fact]
    public async Task PortalAppearanceIsSynced()
    {
        using var testHost = new TestHost.TestHost();

        var portalId = Guid.NewGuid().ToString();

        testHost.Given.Api.AnExistingDevPortal(portalId: portalId, name: "default");

        await testHost.Given.File.AnExistingDevPortal(inputDirectory: @"c:\temp\input", portalName: "default");
        await testHost.Given.File.AnExistingDevPortalAppearance(inputDirectory: @"c:\temp\input", portalName: "default", themeName: "custom");
        await testHost.Given.File.AnExistingDevPortalAuthSettings(inputDirectory: @"c:\temp\input", portalName: "default");
        await testHost.Given.File.ExistingDevPortalTeams(inputDirectory: @"c:\temp\input", portalName: "default");

        var syncService = testHost.GetRequiredService<SyncService>();

        await syncService.Sync(@"c:\temp\input", new Dictionary<string, string>(), true, testHost.ApiClientOptions);

        testHost.Then.Api.PortalAppearanceShouldHaveBeenUpdated(portalId);
    }

    [Fact]
    public async Task PortalAuthSettingsAreSynced()
    {
        using var testHost = new TestHost.TestHost();

        var portalId = Guid.NewGuid().ToString();

        testHost.Given.Api.AnExistingDevPortal(portalId: portalId, name: "default");

        await testHost.Given.File.AnExistingDevPortal(inputDirectory: @"c:\temp\input", portalName: "default");
        await testHost.Given.File.AnExistingDevPortalAppearance(inputDirectory: @"c:\temp\input", portalName: "default");
        await testHost.Given.File.AnExistingDevPortalAuthSettings(
            inputDirectory: @"c:\temp\input",
            portalName: "default",
            oidcAuthEnabled: true,
            oidcConfig: new OidcAuthSettings(
                "MyIssuer",
                "MyClientId",
                "MyClientSecret",
                ["openid", "profile", "email"],
                new OidcClaimMappings("name", "email", "groups")
            )
        );
        await testHost.Given.File.ExistingDevPortalTeams(inputDirectory: @"c:\temp\input", portalName: "default");

        var syncService = testHost.GetRequiredService<SyncService>();

        await syncService.Sync(@"c:\temp\input", new Dictionary<string, string>(), true, testHost.ApiClientOptions);

        testHost.Then.Api.PortalAuthSettingsShouldHaveBeenUpdated(portalId);
    }
}
