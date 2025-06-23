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

        var syncService = testHost.GetRequiredService<SyncService>();

        await syncService.Sync(@"c:\temp\input", testHost.ApiClientOptions, cancellationToken: TestContext.Current.CancellationToken);

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

        var syncService = testHost.GetRequiredService<SyncService>();

        await syncService.Sync(@"c:\temp\input", testHost.ApiClientOptions, cancellationToken: TestContext.Current.CancellationToken);

        testHost.Then.Api.PortalAppearanceShouldHaveBeenUpdated(portalId);
    }

    [Fact]
    public async Task PortalAuthSettingsAreSynced()
    {
        using var testHost = new TestHost.TestHost();

        var portalId = Guid.NewGuid().ToString();

        testHost.Given.Api.AnExistingDevPortal(portalId: portalId, name: "default");

        await testHost.Given.File.AnExistingDevPortal(inputDirectory: @"c:\temp\input", portalName: "default");
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

        var syncService = testHost.GetRequiredService<SyncService>();

        await syncService.Sync(@"c:\temp\input", testHost.ApiClientOptions, cancellationToken: TestContext.Current.CancellationToken);

        testHost.Then.Api.PortalAuthSettingsShouldHaveBeenUpdated(portalId);
    }

    [Fact]
    public async Task PortalTeamMappingsAreSynced()
    {
        using var testHost = new TestHost.TestHost();

        var portalId = Guid.NewGuid().ToString();

        testHost.Given.Api.AnExistingDevPortal(portalId: portalId, name: "default");

        await testHost.Given.File.AnExistingDevPortal(inputDirectory: @"c:\temp\input", portalName: "default");

        await testHost.Given.File.AnExistingDevPortalTeam(
            inputDirectory: @"c:\temp\input",
            portalName: "default",
            name: "Team 1",
            description: "Team One"
        );

        await testHost.Given.File.AnExistingDevPortalAuthSettings(
            inputDirectory: @"c:\temp\input",
            portalName: "default",
            oidcAuthEnabled: true,
            oidcTeamMappingEnabled: true,
            oidcConfig: new OidcAuthSettings(
                "MyIssuer",
                "MyClientId",
                "MyClientSecret",
                ["openid", "profile", "email"],
                new OidcClaimMappings("name", "email", "groups")
            ),
            oidcTeamMappings: new[] { new OidcTeamMapping("Team 1", ["Group 1"]) }
        );

        var syncService = testHost.GetRequiredService<SyncService>();

        await syncService.Sync(@"c:\temp\input", testHost.ApiClientOptions, cancellationToken: TestContext.Current.CancellationToken);

        testHost.Then.Api.PortalTeamMappingsShouldHaveBeenUpdated(portalId);
    }

    [Fact]
    public async Task PortalTeamMappingsWithExistingTeamAreSynced()
    {
        using var testHost = new TestHost.TestHost();

        var portalId = Guid.NewGuid().ToString();
        var teamId = Guid.NewGuid().ToString();

        testHost.Given.Api.AnExistingDevPortal(portalId: portalId, name: "default");
        testHost.Given.Api.AnExistingDevPortalTeam(portalId: portalId, name: "Team 1", description: "Team One", teamId: teamId);

        await testHost.Given.File.AnExistingDevPortal(inputDirectory: @"c:\temp\input", portalName: "default");

        await testHost.Given.File.AnExistingDevPortalTeam(
            inputDirectory: @"c:\temp\input",
            portalName: "default",
            name: "Team 1",
            description: "Team One"
        );

        await testHost.Given.File.AnExistingDevPortalAuthSettings(
            inputDirectory: @"c:\temp\input",
            portalName: "default",
            oidcAuthEnabled: true,
            oidcTeamMappingEnabled: true,
            oidcConfig: new OidcAuthSettings(
                "MyIssuer",
                "MyClientId",
                "MyClientSecret",
                ["openid", "profile", "email"],
                new OidcClaimMappings("name", "email", "groups")
            ),
            oidcTeamMappings: new[] { new OidcTeamMapping("Team 1", ["Group 1"]) }
        );

        var syncService = testHost.GetRequiredService<SyncService>();

        await syncService.Sync(@"c:\temp\input", testHost.ApiClientOptions, cancellationToken: TestContext.Current.CancellationToken);

        testHost.Then.Api.PortalTeamMappingsShouldHaveBeenUpdated(portalId);
    }
}
