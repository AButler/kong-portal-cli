namespace CLI.UnitTests;

public class DumpPortalApiProducts
{
    [Fact]
    public async Task PortalSettingsAreDumped()
    {
        using var testHost = new TestHost.TestHost();

        var dumpService = testHost.GetRequiredService<DumpService>();

        var portalId = Guid.NewGuid().ToString();
        testHost.Given.Api.AnExistingDevPortal(portalId: portalId, name: "default");
        testHost.Given.Api.AnExistingApiProduct(
            name: "API Product 1",
            description: "This is API Product 1",
            labels: new Dictionary<string, string> { ["Author"] = "Bob Bobertson", ["Tag"] = "eCommerce" },
            portalIds: new[] { portalId }
        );

        var outputDirectory = @"c:\temp\output";

        await dumpService.Dump(outputDirectory, testHost.ApiClientOptions, TestContext.Current.CancellationToken);

        await testHost.Then.DumpedFile.ShouldHavePortalProduct(
            outputDirectory: outputDirectory,
            portalName: "default",
            apiProductSyncId: "api-product-1"
        );
    }
}
