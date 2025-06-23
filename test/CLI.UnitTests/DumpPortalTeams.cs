namespace CLI.UnitTests;

public class DumpPortalTeams
{
    [Fact]
    public async Task NoTeamsAreDumped()
    {
        using var testHost = new TestHost.TestHost();

        var dumpService = testHost.GetRequiredService<DumpService>();

        var portalId = Guid.NewGuid().ToString();

        testHost.Given.Api.AnExistingDevPortal(portalId: portalId, name: "default");

        var outputDirectory = @"c:\temp\output";

        await dumpService.Dump(outputDirectory, testHost.ApiClientOptions, TestContext.Current.CancellationToken);

        await testHost.Then.DumpedFile.ShouldHaveNoPortalTeams(outputDirectory: outputDirectory, portalName: "default");
    }

    [Fact]
    public async Task OneTeamIsDumped()
    {
        using var testHost = new TestHost.TestHost();

        var dumpService = testHost.GetRequiredService<DumpService>();

        var portalId = Guid.NewGuid().ToString();

        testHost.Given.Api.AnExistingDevPortal(portalId: portalId, name: "default");
        testHost.Given.Api.AnExistingDevPortalTeam(portalId: portalId, name: "Team1", description: "Team One");

        var outputDirectory = @"c:\temp\output";

        await dumpService.Dump(outputDirectory, testHost.ApiClientOptions, TestContext.Current.CancellationToken);

        await testHost.Then.DumpedFile.ShouldHavePortalTeam(
            outputDirectory: outputDirectory,
            portalName: "default",
            teamName: "Team1",
            teamDescription: "Team One"
        );
    }

    [Fact]
    public async Task TwoTeamsAreDumped()
    {
        using var testHost = new TestHost.TestHost();

        var dumpService = testHost.GetRequiredService<DumpService>();

        var portalId = Guid.NewGuid().ToString();

        testHost.Given.Api.AnExistingDevPortal(portalId: portalId, name: "default");
        testHost.Given.Api.AnExistingDevPortalTeam(portalId: portalId, name: "Team2", description: "Team Two");
        testHost.Given.Api.AnExistingDevPortalTeam(portalId: portalId, name: "Team1", description: "Team One");

        var outputDirectory = @"c:\temp\output";

        await dumpService.Dump(outputDirectory, testHost.ApiClientOptions, TestContext.Current.CancellationToken);

        await testHost.Then.DumpedFile.ShouldHavePortalTeam(
            outputDirectory: outputDirectory,
            portalName: "default",
            teamName: "Team1",
            teamDescription: "Team One"
        );

        await testHost.Then.DumpedFile.ShouldHavePortalTeam(
            outputDirectory: outputDirectory,
            portalName: "default",
            teamName: "Team2",
            teamDescription: "Team Two"
        );
    }

    [Fact]
    public async Task MultipleTeamsAreDumped()
    {
        using var testHost = new TestHost.TestHost();

        testHost.Given.Api.TheKongApiPageSizeIs(2);

        var dumpService = testHost.GetRequiredService<DumpService>();

        var portalId = Guid.NewGuid().ToString();

        testHost.Given.Api.AnExistingDevPortal(portalId: portalId, name: "default");

        for (var i = 1; i < 10; i++)
        {
            testHost.Given.Api.AnExistingDevPortalTeam(portalId: portalId, name: $"Team{i}", description: $"Members of team {i}");
        }

        var outputDirectory = @"c:\temp\output";

        await dumpService.Dump(outputDirectory, testHost.ApiClientOptions, TestContext.Current.CancellationToken);

        for (var i = 1; i < 10; i++)
        {
            await testHost.Then.DumpedFile.ShouldHavePortalTeam(
                outputDirectory: outputDirectory,
                portalName: "default",
                teamName: $"Team{i}",
                teamDescription: $"Members of team {i}"
            );
        }
    }

    [Fact]
    public async Task TeamIsDumpedWithRole()
    {
        using var testHost = new TestHost.TestHost();

        var dumpService = testHost.GetRequiredService<DumpService>();

        var apiProductId = Guid.NewGuid().ToString();
        var portalId = Guid.NewGuid().ToString();
        var teamId = Guid.NewGuid().ToString();

        testHost.Given.Api.AnExistingApiProduct(productId: apiProductId, name: "API Product 1");
        testHost.Given.Api.AnExistingDevPortal(portalId: portalId, name: "default");
        testHost.Given.Api.AnExistingDevPortalTeam(portalId: portalId, name: "Team1", description: "Team One", teamId: teamId);
        testHost.Given.Api.AnExistingDevPortalTeamRole(portalId: portalId, teamId: teamId, roleName: "API Viewer", entityId: apiProductId);

        var outputDirectory = @"c:\temp\output";

        await dumpService.Dump(outputDirectory, testHost.ApiClientOptions, TestContext.Current.CancellationToken);

        await testHost.Then.DumpedFile.ShouldHavePortalTeam(
            outputDirectory: outputDirectory,
            portalName: "default",
            teamName: "Team1",
            teamDescription: "Team One"
        );

        await testHost.Then.DumpedFile.ShouldHavePortalTeamRole(
            outputDirectory: outputDirectory,
            portalName: "default",
            teamName: "Team1",
            apiProduct: "api-product-1",
            roleNames: ["API Viewer"]
        );
    }

    [Fact]
    public async Task TeamIsDumpedWithRoles()
    {
        using var testHost = new TestHost.TestHost();

        var dumpService = testHost.GetRequiredService<DumpService>();

        var apiProductId = Guid.NewGuid().ToString();
        var portalId = Guid.NewGuid().ToString();
        var teamId = Guid.NewGuid().ToString();

        testHost.Given.Api.AnExistingApiProduct(productId: apiProductId, name: "API Product 1");
        testHost.Given.Api.AnExistingDevPortal(portalId: portalId, name: "default");
        testHost.Given.Api.AnExistingDevPortalTeam(portalId: portalId, name: "Team1", description: "Team One", teamId: teamId);
        testHost.Given.Api.AnExistingDevPortalTeamRole(portalId: portalId, teamId: teamId, roleName: "API Viewer", entityId: apiProductId);
        testHost.Given.Api.AnExistingDevPortalTeamRole(portalId: portalId, teamId: teamId, roleName: "API Consumer", entityId: apiProductId);

        var outputDirectory = @"c:\temp\output";

        await dumpService.Dump(outputDirectory, testHost.ApiClientOptions, TestContext.Current.CancellationToken);

        await testHost.Then.DumpedFile.ShouldHavePortalTeam(
            outputDirectory: outputDirectory,
            portalName: "default",
            teamName: "Team1",
            teamDescription: "Team One"
        );

        await testHost.Then.DumpedFile.ShouldHavePortalTeamRole(
            outputDirectory: outputDirectory,
            portalName: "default",
            teamName: "Team1",
            apiProduct: "api-product-1",
            roleNames: ["API Viewer", "API Consumer"]
        );
    }
}
