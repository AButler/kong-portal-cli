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

        await dumpService.Dump(outputDirectory, testHost.ApiClientOptions);

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

        await dumpService.Dump(outputDirectory, testHost.ApiClientOptions);

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
}
