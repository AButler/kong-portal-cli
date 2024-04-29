namespace CLI.UnitTests;

public class SyncPortalTeamsTests
{
    [Fact]
    public async Task NoTeamsAreSynced()
    {
        using var testHost = new TestHost.TestHost();

        var portalId = Guid.NewGuid().ToString();

        await testHost.Given.File.AnExistingDevPortal(inputDirectory: @"c:\temp\input", portalName: "default");

        testHost.Given.Api.AnExistingDevPortal(portalId: portalId, name: "default");

        var syncService = testHost.GetRequiredService<SyncService>();

        await syncService.Sync(@"c:\temp\input", testHost.ApiClientOptions);

        testHost.Then.Api.NoPortalTeamsShouldHaveBeenCreated(portalId);
    }

    [Fact]
    public async Task TeamIsCreated()
    {
        using var testHost = new TestHost.TestHost();

        var portalId = Guid.NewGuid().ToString();

        await testHost.Given.File.AnExistingDevPortal(inputDirectory: @"c:\temp\input", portalName: "default");
        await testHost.Given.File.AnExistingDevPortalTeam(
            inputDirectory: @"c:\temp\input",
            portalName: "default",
            name: "Team1",
            description: "Team One"
        );

        testHost.Given.Api.AnExistingDevPortal(portalId: portalId, name: "default");

        var syncService = testHost.GetRequiredService<SyncService>();

        await syncService.Sync(@"c:\temp\input", testHost.ApiClientOptions);

        testHost.Then.Api.PortalTeamShouldHaveBeenCreated(portalId);
    }

    [Fact]
    public async Task TeamIsUpdated()
    {
        using var testHost = new TestHost.TestHost();

        var portalId = Guid.NewGuid().ToString();

        await testHost.Given.File.AnExistingDevPortal(inputDirectory: @"c:\temp\input", portalName: "default");
        await testHost.Given.File.AnExistingDevPortalTeam(
            inputDirectory: @"c:\temp\input",
            portalName: "default",
            name: "Team1",
            description: "Updated description"
        );

        testHost.Given.Api.AnExistingDevPortal(portalId: portalId, name: "default");
        testHost.Given.Api.AnExistingDevPortalTeam(portalId: portalId, name: "Team1", description: "Team One");

        var syncService = testHost.GetRequiredService<SyncService>();

        await syncService.Sync(@"c:\temp\input", testHost.ApiClientOptions);

        testHost.Then.Api.PortalTeamShouldHaveBeenUpdated(portalId);
    }

    [Fact]
    public async Task TeamsAreInSync()
    {
        using var testHost = new TestHost.TestHost();

        var portalId = Guid.NewGuid().ToString();

        await testHost.Given.File.AnExistingDevPortal(inputDirectory: @"c:\temp\input", portalName: "default");
        await testHost.Given.File.AnExistingDevPortalTeam(
            inputDirectory: @"c:\temp\input",
            portalName: "default",
            name: "Team1",
            description: "Team One"
        );
        await testHost.Given.File.AnExistingDevPortalTeam(
            inputDirectory: @"c:\temp\input",
            portalName: "default",
            name: "Team2",
            description: "Team Two"
        );

        testHost.Given.Api.AnExistingDevPortal(portalId: portalId, name: "default");
        testHost.Given.Api.AnExistingDevPortalTeam(portalId: portalId, name: "Team1", description: "Team One");
        testHost.Given.Api.AnExistingDevPortalTeam(portalId: portalId, name: "Team2", description: "Team Two");

        var syncService = testHost.GetRequiredService<SyncService>();

        await syncService.Sync(@"c:\temp\input", testHost.ApiClientOptions);

        testHost.Then.Api.NoPortalTeamsShouldHaveBeenCreated(portalId);
        testHost.Then.Api.NoPortalTeamsShouldHaveBeenUpdated(portalId);
    }

    [Fact]
    public async Task TeamsAreInSyncInDifferentOrder()
    {
        using var testHost = new TestHost.TestHost();

        var portalId = Guid.NewGuid().ToString();

        await testHost.Given.File.AnExistingDevPortal(inputDirectory: @"c:\temp\input", portalName: "default");
        await testHost.Given.File.AnExistingDevPortalTeam(
            inputDirectory: @"c:\temp\input",
            portalName: "default",
            name: "Team2",
            description: "Team Two"
        );
        await testHost.Given.File.AnExistingDevPortalTeam(
            inputDirectory: @"c:\temp\input",
            portalName: "default",
            name: "Team1",
            description: "Team One"
        );

        testHost.Given.Api.AnExistingDevPortal(portalId: portalId, name: "default");
        testHost.Given.Api.AnExistingDevPortalTeam(portalId: portalId, name: "Team1", description: "Team One");
        testHost.Given.Api.AnExistingDevPortalTeam(portalId: portalId, name: "Team2", description: "Team Two");

        var syncService = testHost.GetRequiredService<SyncService>();

        await syncService.Sync(@"c:\temp\input", testHost.ApiClientOptions);

        testHost.Then.Api.NoPortalTeamsShouldHaveBeenCreated(portalId);
        testHost.Then.Api.NoPortalTeamsShouldHaveBeenUpdated(portalId);
    }

    [Fact]
    public async Task TeamWithRoleIsCreated()
    {
        using var testHost = new TestHost.TestHost();

        var portalId = Guid.NewGuid().ToString();
        var teamId = Guid.NewGuid().ToString();

        await testHost.Given.File.AnExistingApiProduct(inputDirectory: @"c:\temp\input", "API Product 1");
        await testHost.Given.File.AnExistingDevPortal(inputDirectory: @"c:\temp\input", portalName: "default");
        await testHost.Given.File.AnExistingDevPortalTeam(
            inputDirectory: @"c:\temp\input",
            portalName: "default",
            name: "Team1",
            description: "Team One"
        );
        await testHost.Given.File.AnExistingDevPortalTeamRole(
            inputDirectory: @"c:\temp\input",
            portalName: "default",
            teamName: "Team1",
            apiProduct: "api-product-1",
            role: "API Viewer"
        );

        testHost.Given.Api.AnExistingApiProduct(name: "API Product 1");
        testHost.Given.Api.AnExistingDevPortal(portalId: portalId, name: "default");

        var syncService = testHost.GetRequiredService<SyncService>();

        await syncService.Sync(@"c:\temp\input", testHost.ApiClientOptions);

        testHost.Then.Api.PortalTeamRoleShouldHaveBeenAssigned(portalId, teamId);
    }

    [Fact]
    public async Task TeamIsUpdatedWithRoleAdded()
    {
        using var testHost = new TestHost.TestHost();

        var portalId = Guid.NewGuid().ToString();
        var teamId = Guid.NewGuid().ToString();

        await testHost.Given.File.AnExistingApiProduct(inputDirectory: @"c:\temp\input", "API Product 1");
        await testHost.Given.File.AnExistingDevPortal(inputDirectory: @"c:\temp\input", portalName: "default");
        await testHost.Given.File.AnExistingDevPortalTeam(
            inputDirectory: @"c:\temp\input",
            portalName: "default",
            name: "Team1",
            description: "Team One"
        );
        await testHost.Given.File.AnExistingDevPortalTeamRole(
            inputDirectory: @"c:\temp\input",
            portalName: "default",
            teamName: "Team1",
            apiProduct: "api-product-1",
            role: "API Viewer"
        );

        testHost.Given.Api.AnExistingApiProduct(name: "API Product 1");
        testHost.Given.Api.AnExistingDevPortal(portalId: portalId, name: "default");
        testHost.Given.Api.AnExistingDevPortalTeam(portalId: portalId, teamId: teamId, name: "Team1", description: "Team 1");

        var syncService = testHost.GetRequiredService<SyncService>();

        await syncService.Sync(@"c:\temp\input", testHost.ApiClientOptions);

        testHost.Then.Api.PortalTeamRoleShouldHaveBeenAssigned(portalId, teamId);
    }

    [Fact]
    public async Task TeamIsUpdatedWithRoleRemoved()
    {
        using var testHost = new TestHost.TestHost();

        var productId = Guid.NewGuid().ToString();
        var portalId = Guid.NewGuid().ToString();
        var teamId = Guid.NewGuid().ToString();
        var roleId = Guid.NewGuid().ToString();

        await testHost.Given.File.AnExistingApiProduct(inputDirectory: @"c:\temp\input", "API Product 1");
        await testHost.Given.File.AnExistingDevPortal(inputDirectory: @"c:\temp\input", portalName: "default");
        await testHost.Given.File.AnExistingDevPortalTeam(
            inputDirectory: @"c:\temp\input",
            portalName: "default",
            name: "Team1",
            description: "Team One"
        );

        testHost.Given.Api.AnExistingApiProduct(productId: productId, name: "API Product 1");
        testHost.Given.Api.AnExistingDevPortal(portalId: portalId, name: "default");
        testHost.Given.Api.AnExistingDevPortalTeam(portalId: portalId, teamId: teamId, name: "Team1", description: "Team 1");
        testHost.Given.Api.AnExistingDevPortalTeamRole(
            portalId: portalId,
            teamId: teamId,
            roleId: roleId,
            roleName: "API Viewer",
            entityId: productId
        );

        var syncService = testHost.GetRequiredService<SyncService>();

        await syncService.Sync(@"c:\temp\input", testHost.ApiClientOptions);

        testHost.Then.Api.PortalTeamRoleShouldHaveBeenRemoved(portalId, teamId, roleId);
    }
}
