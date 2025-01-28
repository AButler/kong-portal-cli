using Kong.Portal.CLI;

namespace CLI.UnitTests;

public class DumpApiProductsTests
{
    [Fact]
    public async Task ApiProductIsDumped()
    {
        using var testHost = new TestHost.TestHost();

        var dumpService = testHost.GetRequiredService<DumpService>();

        testHost.Given.Api.AnExistingApiProduct(
            name: "API Product 1",
            description: "This is API Product 1",
            labels: new Dictionary<string, string> { ["Author"] = "Bob Bobertson", ["Tag"] = "eCommerce" }
        );

        var outputDirectory = @"c:\temp\output";

        await dumpService.Dump(outputDirectory, testHost.ApiClientOptions);

        await testHost.Then.DumpedFile.ShouldHaveApiProduct(
            outputDirectory: outputDirectory,
            syncId: "api-product-1",
            name: "API Product 1",
            description: "This is API Product 1",
            labels: new Dictionary<string, string> { ["Author"] = "Bob Bobertson", ["Tag"] = "eCommerce" }
        );
    }

    [Fact]
    public async Task ApiProductPublishedToPortalIsDumped()
    {
        using var testHost = new TestHost.TestHost();

        var dumpService = testHost.GetRequiredService<DumpService>();

        var portalId = Guid.NewGuid().ToString();

        testHost.Given.Api.AnExistingDevPortal(portalId: portalId, name: "default");

        testHost.Given.Api.AnExistingApiProduct(
            name: "API Product 1",
            description: "This is API Product 1",
            portalIds: new[] { portalId },
            labels: new Dictionary<string, string> { ["Author"] = "Bob Bobertson", ["Tag"] = "eCommerce" }
        );

        var outputDirectory = @"c:\temp\output";

        await dumpService.Dump(outputDirectory, testHost.ApiClientOptions);

        await testHost.Then.DumpedFile.ShouldHaveApiProduct(
            outputDirectory: outputDirectory,
            syncId: "api-product-1",
            name: "API Product 1",
            description: "This is API Product 1",
            labels: new Dictionary<string, string> { ["Author"] = "Bob Bobertson", ["Tag"] = "eCommerce" }
        );
    }

    [Fact]
    public async Task ApiProductWithSyncIdLabelIsDumped()
    {
        using var testHost = new TestHost.TestHost();

        var dumpService = testHost.GetRequiredService<DumpService>();

        testHost.Given.Api.AnExistingApiProduct(
            name: "API Product 1",
            description: "This is API Product 1",
            labels: new Dictionary<string, string>
            {
                ["Author"] = "Bob Bobertson",
                ["Tag"] = "eCommerce",
                [Constants.SyncIdLabel] = "different-sync-id",
            }
        );

        var outputDirectory = @"c:\temp\output";

        await dumpService.Dump(outputDirectory, testHost.ApiClientOptions);

        await testHost.Then.DumpedFile.ShouldHaveApiProduct(
            outputDirectory: outputDirectory,
            syncId: "different-sync-id",
            name: "API Product 1",
            description: "This is API Product 1",
            labels: new Dictionary<string, string> { ["Author"] = "Bob Bobertson", ["Tag"] = "eCommerce" }
        );
    }

    [Fact]
    public async Task TwoApiProductsWithSameNameAreDumped()
    {
        using var testHost = new TestHost.TestHost();

        var dumpService = testHost.GetRequiredService<DumpService>();

        testHost.Given.Api.AnExistingApiProduct(
            name: "API Product",
            description: "This is API Product 1",
            labels: new Dictionary<string, string> { ["Author"] = "Bob Bobertson", ["Tag"] = "eCommerce" }
        );

        testHost.Given.Api.AnExistingApiProduct(
            name: "API Product",
            description: "This is API Product 2",
            labels: new Dictionary<string, string> { ["Author"] = "Bob Bobertson", ["Tag"] = "Frontend" }
        );

        var outputDirectory = @"c:\temp\output";

        await dumpService.Dump(outputDirectory, testHost.ApiClientOptions);

        await testHost.Then.DumpedFile.ShouldHaveApiProduct(
            outputDirectory,
            "api-product",
            "API Product",
            "This is API Product 1",
            new Dictionary<string, string> { ["Author"] = "Bob Bobertson", ["Tag"] = "eCommerce" }
        );

        await testHost.Then.DumpedFile.ShouldHaveApiProduct(
            outputDirectory: outputDirectory,
            syncId: "api-product-1",
            name: "API Product",
            description: "This is API Product 2",
            labels: new Dictionary<string, string> { ["Author"] = "Bob Bobertson", ["Tag"] = "Frontend" }
        );
    }

    [Fact]
    public async Task MultipleApiProductsAreDumped()
    {
        using var testHost = new TestHost.TestHost();

        testHost.Given.Api.TheKongApiPageSizeIs(2);

        var dumpService = testHost.GetRequiredService<DumpService>();

        for (var i = 1; i < 10; i++)
        {
            testHost.Given.Api.AnExistingApiProduct(name: $"API Product {i}", description: $"This is API Product {i}");
        }

        var outputDirectory = @"c:\temp\output";

        await dumpService.Dump(outputDirectory, testHost.ApiClientOptions);

        for (var i = 1; i < 10; i++)
        {
            await testHost.Then.DumpedFile.ShouldHaveApiProduct(
                outputDirectory: outputDirectory,
                syncId: $"api-product-{i}",
                name: $"API Product {i}",
                description: $"This is API Product {i}",
                labels: new Dictionary<string, string>()
            );
        }
    }
}
