using CLI.UnitTests.TestHost;

namespace CLI.UnitTests;

public class SyncPortalTeamsTests
{
    [Fact]
    public async Task NoTeamsAreSynced()
    {
        using var testHost = new TestHost.TestHost();

        var portalId = Guid.NewGuid().ToString();

        await testHost.Given.File.AnExistingDevPortal(inputDirectory: @"c:\temp\input", portalName: "default");
        await testHost.Given.File.AnExistingDevPortalAppearance(inputDirectory: @"c:\temp\input", portalName: "default");
        await testHost.Given.File.AnExistingDevPortalAuthSettings(inputDirectory: @"c:\temp\input", portalName: "default");
        await testHost.Given.File.ExistingDevPortalTeams(inputDirectory: @"c:\temp\input", portalName: "default");

        testHost.Given.Api.AnExistingDevPortal(portalId: portalId, name: "default");

        var syncService = testHost.GetRequiredService<SyncService>();

        await syncService.Sync(@"c:\temp\input", new Dictionary<string, string>(), true, testHost.ApiClientOptions);

        testHost.Then.Api.NoPortalTeamsShouldHaveBeenCreated(portalId);
    }

    [Fact]
    public async Task TeamIsCreated()
    {
        using var testHost = new TestHost.TestHost();

        var portalId = Guid.NewGuid().ToString();

        await testHost.Given.File.AnExistingDevPortal(inputDirectory: @"c:\temp\input", portalName: "default");
        await testHost.Given.File.AnExistingDevPortalAppearance(inputDirectory: @"c:\temp\input", portalName: "default");
        await testHost.Given.File.AnExistingDevPortalAuthSettings(inputDirectory: @"c:\temp\input", portalName: "default");
        await testHost.Given.File.ExistingDevPortalTeams(inputDirectory: @"c:\temp\input", portalName: "default", new Team("Team1", "Team One"));

        testHost.Given.Api.AnExistingDevPortal(portalId: portalId, name: "default");

        var syncService = testHost.GetRequiredService<SyncService>();

        await syncService.Sync(@"c:\temp\input", new Dictionary<string, string>(), true, testHost.ApiClientOptions);

        testHost.Then.Api.PortalTeamShouldHaveBeenCreated(portalId);
    }

    [Fact]
    public async Task TeamIsUpdated()
    {
        using var testHost = new TestHost.TestHost();

        var portalId = Guid.NewGuid().ToString();

        await testHost.Given.File.AnExistingDevPortal(inputDirectory: @"c:\temp\input", portalName: "default");
        await testHost.Given.File.AnExistingDevPortalAppearance(inputDirectory: @"c:\temp\input", portalName: "default");
        await testHost.Given.File.AnExistingDevPortalAuthSettings(inputDirectory: @"c:\temp\input", portalName: "default");
        await testHost.Given.File.ExistingDevPortalTeams(
            inputDirectory: @"c:\temp\input",
            portalName: "default",
            new Team("Team1", "Updated description")
        );

        testHost.Given.Api.AnExistingDevPortal(portalId: portalId, name: "default");
        testHost.Given.Api.AnExistingDevPortalTeam(portalId: portalId, name: "Team1", description: "Team One");

        var syncService = testHost.GetRequiredService<SyncService>();

        await syncService.Sync(@"c:\temp\input", new Dictionary<string, string>(), true, testHost.ApiClientOptions);

        testHost.Then.Api.PortalTeamShouldHaveBeenUpdated(portalId);
    }

    [Fact]
    public async Task TeamsAreInSync()
    {
        using var testHost = new TestHost.TestHost();

        var portalId = Guid.NewGuid().ToString();

        await testHost.Given.File.AnExistingDevPortal(inputDirectory: @"c:\temp\input", portalName: "default");
        await testHost.Given.File.AnExistingDevPortalAppearance(inputDirectory: @"c:\temp\input", portalName: "default");
        await testHost.Given.File.AnExistingDevPortalAuthSettings(inputDirectory: @"c:\temp\input", portalName: "default");
        await testHost.Given.File.ExistingDevPortalTeams(
            inputDirectory: @"c:\temp\input",
            portalName: "default",
            new Team("Team1", "Team One"),
            new Team("Team2", "Team Two")
        );

        testHost.Given.Api.AnExistingDevPortal(portalId: portalId, name: "default");
        testHost.Given.Api.AnExistingDevPortalTeam(portalId: portalId, name: "Team1", description: "Team One");
        testHost.Given.Api.AnExistingDevPortalTeam(portalId: portalId, name: "Team2", description: "Team Two");

        var syncService = testHost.GetRequiredService<SyncService>();

        await syncService.Sync(@"c:\temp\input", new Dictionary<string, string>(), true, testHost.ApiClientOptions);

        testHost.Then.Api.NoPortalTeamsShouldHaveBeenCreated(portalId);
        testHost.Then.Api.NoPortalTeamsShouldHaveBeenUpdated(portalId);
    }

    [Fact]
    public async Task TeamsAreInSyncInDifferentOrder()
    {
        using var testHost = new TestHost.TestHost();

        var portalId = Guid.NewGuid().ToString();

        await testHost.Given.File.AnExistingDevPortal(inputDirectory: @"c:\temp\input", portalName: "default");
        await testHost.Given.File.AnExistingDevPortalAppearance(inputDirectory: @"c:\temp\input", portalName: "default");
        await testHost.Given.File.AnExistingDevPortalAuthSettings(inputDirectory: @"c:\temp\input", portalName: "default");
        await testHost.Given.File.ExistingDevPortalTeams(
            inputDirectory: @"c:\temp\input",
            portalName: "default",
            new Team("Team2", "Team Two"),
            new Team("Team1", "Team One")
        );

        testHost.Given.Api.AnExistingDevPortal(portalId: portalId, name: "default");
        testHost.Given.Api.AnExistingDevPortalTeam(portalId: portalId, name: "Team1", description: "Team One");
        testHost.Given.Api.AnExistingDevPortalTeam(portalId: portalId, name: "Team2", description: "Team Two");

        var syncService = testHost.GetRequiredService<SyncService>();

        await syncService.Sync(@"c:\temp\input", new Dictionary<string, string>(), true, testHost.ApiClientOptions);

        testHost.Then.Api.NoPortalTeamsShouldHaveBeenCreated(portalId);
        testHost.Then.Api.NoPortalTeamsShouldHaveBeenUpdated(portalId);
    }
}
