namespace CLI.UnitTests;

public class DumpPortalSettingsTests
{
    [Fact]
    public async Task PortalSettingsAreDumped()
    {
        using var testHost = new TestHost.TestHost();

        var dumpService = testHost.GetRequiredService<DumpService>();

        var portalId = Guid.NewGuid().ToString();
        testHost.Given.Api.AnExistingDevPortal(
            portalId: portalId,
            name: "default",
            isPublic: true,
            rbacEnabled: false,
            autoApproveApplications: false,
            autoApproveDevelopers: false,
            customDomain: null,
            customClientDomain: null
        );

        var outputDirectory = @"c:\temp\output";

        await dumpService.Dump(outputDirectory, testHost.ApiClientOptions);

        await testHost.Then.DumpedFile.ShouldHavePortal(
            outputDirectory: outputDirectory,
            name: "default",
            isPublic: true,
            rbacEnabled: false,
            autoApproveApplications: false,
            autoApproveDevelopers: false,
            customDomain: null,
            customClientDomain: null
        );
    }

    [Fact]
    public async Task PortalProductsAreDumped()
    {
        using var testHost = new TestHost.TestHost();

        var dumpService = testHost.GetRequiredService<DumpService>();

        var product1Id = Guid.NewGuid().ToString();
        var product2Id = Guid.NewGuid().ToString();
        var portalId = Guid.NewGuid().ToString();

        testHost.Given.Api.AnExistingApiProduct(productId: product1Id, name: "API Product 1");
        testHost.Given.Api.AnExistingApiProduct(productId: product2Id, name: "API Product 2");
        testHost.Given.Api.AnExistingDevPortal(
            portalId: portalId,
            name: "default",
            isPublic: true,
            rbacEnabled: false,
            autoApproveApplications: false,
            autoApproveDevelopers: false,
            customDomain: null,
            customClientDomain: null
        );

        var outputDirectory = @"c:\temp\output";

        await dumpService.Dump(outputDirectory, testHost.ApiClientOptions);

        await testHost.Then.DumpedFile.ShouldHavePortal(
            outputDirectory: outputDirectory,
            name: "default",
            isPublic: true,
            rbacEnabled: false,
            autoApproveApplications: false,
            autoApproveDevelopers: false,
            customDomain: null,
            customClientDomain: null
        );
    }

    [Fact]
    public async Task PortalProductsWithSameNameAreDumped()
    {
        using var testHost = new TestHost.TestHost();

        var dumpService = testHost.GetRequiredService<DumpService>();

        var product1Id = Guid.NewGuid().ToString();
        var product2Id = Guid.NewGuid().ToString();
        var portalId = Guid.NewGuid().ToString();

        testHost.Given.Api.AnExistingApiProduct(productId: product1Id, name: "API Product");
        testHost.Given.Api.AnExistingApiProduct(name: "API Product");
        testHost.Given.Api.AnExistingApiProduct(productId: product2Id, name: "API Product");
        testHost.Given.Api.AnExistingApiProduct(name: "API Product");
        testHost.Given.Api.AnExistingDevPortal(
            portalId: portalId,
            name: "default",
            isPublic: true,
            rbacEnabled: false,
            autoApproveApplications: false,
            autoApproveDevelopers: false,
            customDomain: null,
            customClientDomain: null
        );

        var outputDirectory = @"c:\temp\output";

        await dumpService.Dump(outputDirectory, testHost.ApiClientOptions);

        await testHost.Then.DumpedFile.ShouldHavePortal(
            outputDirectory: outputDirectory,
            name: "default",
            isPublic: true,
            rbacEnabled: false,
            autoApproveApplications: false,
            autoApproveDevelopers: false,
            customDomain: null,
            customClientDomain: null
        );
    }
}
