using CLI.UnitTests.TestHost;

namespace CLI.UnitTests;

public class DumpPortalAuthSettings
{
    [Fact]
    public async Task PortalBasicAuthIsDumped()
    {
        using var testHost = new TestHost.TestHost();

        var dumpService = testHost.GetRequiredService<DumpService>();

        var portalId = Guid.NewGuid().ToString();

        testHost.Given.Api.AnExistingDevPortal(
            portalId: portalId,
            name: "default",
            authSettings: new AuthSettings(
                BasicAuthEnabled: true,
                OidcAuthEnabled: false,
                OidcTeamMappingEnabled: false,
                KonnectMappingEnabled: false,
                OidcConfig: null
            )
        );

        var outputDirectory = @"c:\temp\output";

        await dumpService.Dump(outputDirectory, testHost.ApiClientOptions, TestContext.Current.CancellationToken);

        await testHost.Then.DumpedFile.ShouldHavePortalAuthSettings(
            outputDirectory: outputDirectory,
            portalName: "default",
            basicAuthEnabled: true,
            oidcAuthEnabled: false,
            oidcTeamMappingEnabled: false,
            konnectMappingEnabled: false,
            oidcConfig: null
        );
    }

    [Fact]
    public async Task PortalOidcAuthIsDumped()
    {
        using var testHost = new TestHost.TestHost();

        var dumpService = testHost.GetRequiredService<DumpService>();

        var portalId = Guid.NewGuid().ToString();

        testHost.Given.Api.AnExistingDevPortal(
            portalId: portalId,
            name: "default",
            authSettings: new AuthSettings(
                BasicAuthEnabled: false,
                OidcAuthEnabled: true,
                OidcTeamMappingEnabled: true,
                KonnectMappingEnabled: false,
                OidcConfig: new OidcAuthSettings(
                    "http://my-auth",
                    "MyClientId",
                    "MyClientSecret",
                    ["openid", "profile", "email"],
                    new OidcClaimMappings("name", "email", "groups")
                )
            )
        );

        var outputDirectory = @"c:\temp\output";

        await dumpService.Dump(outputDirectory, testHost.ApiClientOptions, TestContext.Current.CancellationToken);

        await testHost.Then.DumpedFile.ShouldHavePortalAuthSettings(
            outputDirectory: outputDirectory,
            portalName: "default",
            basicAuthEnabled: false,
            oidcAuthEnabled: true,
            oidcTeamMappingEnabled: true,
            konnectMappingEnabled: false,
            oidcConfig: new OidcAuthSettings(
                "http://my-auth",
                "MyClientId",
                "MyClientSecret",
                ["openid", "profile", "email"],
                new OidcClaimMappings("name", "email", "groups")
            )
        );
    }

    [Fact]
    public async Task PortalOidcAuthWithTeamMappingsIsDumped()
    {
        using var testHost = new TestHost.TestHost();

        var dumpService = testHost.GetRequiredService<DumpService>();

        var portalId = Guid.NewGuid().ToString();
        var teamId = Guid.NewGuid().ToString();

        testHost.Given.Api.AnExistingDevPortal(
            portalId: portalId,
            name: "default",
            authSettings: new AuthSettings(
                BasicAuthEnabled: false,
                OidcAuthEnabled: true,
                OidcTeamMappingEnabled: true,
                KonnectMappingEnabled: false,
                OidcConfig: new OidcAuthSettings(
                    "http://my-auth",
                    "MyClientId",
                    "MyClientSecret",
                    ["openid", "profile", "email"],
                    new OidcClaimMappings("name", "email", "groups")
                )
            )
        );
        testHost.Given.Api.AnExistingDevPortalTeam(portalId: portalId, name: "Team 1", description: "Team One", teamId: teamId);

        testHost.Given.Api.AnExistingDevPortalTeamGroupMapping(portalId: portalId, teamId: teamId, groupNames: "Group 1");

        var outputDirectory = @"c:\temp\output";

        await dumpService.Dump(outputDirectory, testHost.ApiClientOptions, TestContext.Current.CancellationToken);

        await testHost.Then.DumpedFile.ShouldHavePortalAuthSettings(
            outputDirectory: outputDirectory,
            portalName: "default",
            basicAuthEnabled: false,
            oidcAuthEnabled: true,
            oidcTeamMappingEnabled: true,
            konnectMappingEnabled: false,
            oidcConfig: new OidcAuthSettings(
                "http://my-auth",
                "MyClientId",
                "MyClientSecret",
                ["openid", "profile", "email"],
                new OidcClaimMappings("name", "email", "groups")
            ),
            oidcTeamMappings: [new OidcTeamMapping(TeamName: "Team 1", GroupNames: ["Group 1"])]
        );
    }

    [Fact]
    public async Task PortalOidcAuthWithMultipleTeamMappingsIsDumped()
    {
        using var testHost = new TestHost.TestHost();

        var dumpService = testHost.GetRequiredService<DumpService>();

        var portalId = Guid.NewGuid().ToString();
        var team1Id = Guid.NewGuid().ToString();
        var team2Id = Guid.NewGuid().ToString();

        testHost.Given.Api.AnExistingDevPortal(
            portalId: portalId,
            name: "default",
            authSettings: new AuthSettings(
                BasicAuthEnabled: false,
                OidcAuthEnabled: true,
                OidcTeamMappingEnabled: true,
                KonnectMappingEnabled: false,
                OidcConfig: new OidcAuthSettings(
                    "http://my-auth",
                    "MyClientId",
                    "MyClientSecret",
                    ["openid", "profile", "email"],
                    new OidcClaimMappings("name", "email", "groups")
                )
            )
        );
        testHost.Given.Api.AnExistingDevPortalTeam(portalId: portalId, name: "Team 1", description: "Team One", teamId: team1Id);
        testHost.Given.Api.AnExistingDevPortalTeam(portalId: portalId, name: "Team 2", description: "Team Two", teamId: team2Id);

        testHost.Given.Api.AnExistingDevPortalTeamGroupMapping(portalId: portalId, teamId: team1Id, "Group 1", "Group 2");
        testHost.Given.Api.AnExistingDevPortalTeamGroupMapping(portalId: portalId, teamId: team2Id, "Group 3");

        var outputDirectory = @"c:\temp\output";

        await dumpService.Dump(outputDirectory, testHost.ApiClientOptions, TestContext.Current.CancellationToken);

        await testHost.Then.DumpedFile.ShouldHavePortalAuthSettings(
            outputDirectory: outputDirectory,
            portalName: "default",
            basicAuthEnabled: false,
            oidcAuthEnabled: true,
            oidcTeamMappingEnabled: true,
            konnectMappingEnabled: false,
            oidcConfig: new OidcAuthSettings(
                "http://my-auth",
                "MyClientId",
                "MyClientSecret",
                ["openid", "profile", "email"],
                new OidcClaimMappings("name", "email", "groups")
            ),
            oidcTeamMappings:
            [
                new OidcTeamMapping(TeamName: "Team 1", GroupNames: ["Group 2", "Group 1"]),
                new OidcTeamMapping(TeamName: "Team 2", GroupNames: ["Group 3"]),
            ]
        );
    }
}
