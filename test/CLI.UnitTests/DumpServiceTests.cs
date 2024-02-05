using Kong.Portal.CLI.Services;

namespace CLI.UnitTests;

public class DumpServiceTests
{
    [Fact]
    public async Task ApiProductIsDumped()
    {
        using var testHost = new TestHost.TestHost();

        var dumpService = testHost.GetRequiredService<DumpService>();

        testHost.Given.AnExistingApiProduct(
            name: "API Product 1",
            description: "This is API Product 1",
            labels: new Dictionary<string, string> { ["Author"] = "Bob Bobertson", ["Tag"] = "eCommerce" }
        );

        var outputDirectory = @"c:\temp\output";

        await dumpService.Dump(outputDirectory);

        await testHost.Then.DumpedFile.ShouldHaveApiProduct(
            outputDirectory,
            "API Product 1",
            "This is API Product 1",
            new Dictionary<string, string> { ["Author"] = "Bob Bobertson", ["Tag"] = "eCommerce" }
        );
    }

    [Fact]
    public async Task MultipleApiProductsAreDumped()
    {
        using var testHost = new TestHost.TestHost();

        testHost.Given.TheKongApiPageSizeIs(2);

        var dumpService = testHost.GetRequiredService<DumpService>();

        for (var i = 1; i < 10; i++)
        {
            testHost.Given.AnExistingApiProduct(name: $"API Product {i}", description: $"This is API Product {i}");
        }

        var outputDirectory = @"c:\temp\output";

        await dumpService.Dump(outputDirectory);

        for (var i = 1; i < 10; i++)
        {
            await testHost.Then.DumpedFile.ShouldHaveApiProduct(
                outputDirectory,
                $"API Product {i}",
                $"This is API Product {i}",
                new Dictionary<string, string>()
            );
        }
    }

    [Fact]
    public async Task DocumentIsDumped()
    {
        using var testHost = new TestHost.TestHost();

        var dumpService = testHost.GetRequiredService<DumpService>();

        var productId = Guid.NewGuid().ToString();
        testHost.Given.AnExistingApiProduct(productId: productId, name: "API Product");
        testHost.Given.AnExistingApiProductDocument(
            apiProductId: productId,
            slug: "authentication",
            title: "How to Authenticate",
            content: "# How to Authenticate"
        );

        var outputDirectory = @"c:\temp\output";

        await dumpService.Dump(outputDirectory);

        await testHost.Then.DumpedFile.ShouldHaveApiProductDocument(
            outputDirectory,
            "API Product",
            "authentication",
            "How to Authenticate",
            "# How to Authenticate"
        );
    }

    [Fact]
    public async Task MultipleDocumentsAreDumped()
    {
        using var testHost = new TestHost.TestHost();

        testHost.Given.TheKongApiPageSizeIs(2);

        var dumpService = testHost.GetRequiredService<DumpService>();

        var productId = Guid.NewGuid().ToString();
        testHost.Given.AnExistingApiProduct(productId: productId, name: "API Product");

        for (var i = 0; i < 10; i++)
        {
            testHost.Given.AnExistingApiProductDocument(
                apiProductId: productId,
                slug: $"doc-{i}",
                title: $"Article {i}",
                content: $"# Article {i}\n\n##Contents\n* Item 1"
            );
        }

        var outputDirectory = @"c:\temp\output";

        await dumpService.Dump(outputDirectory);

        for (var i = 0; i < 10; i++)
        {
            await testHost.Then.DumpedFile.ShouldHaveApiProductDocument(
                outputDirectory,
                "API Product",
                $"doc-{i}",
                $"Article {i}",
                $"# Article {i}\n\n##Contents\n* Item 1"
            );
        }
    }

    [Fact]
    public async Task VersionWithSpecificationIsDumped()
    {
        using var testHost = new TestHost.TestHost();

        var dumpService = testHost.GetRequiredService<DumpService>();

        var productId = Guid.NewGuid().ToString();
        testHost.Given.AnExistingApiProduct(productId: productId, name: "API Product");
        testHost.Given.AnExistingApiProductVersion(
            apiProductId: productId,
            name: "v1.0",
            publishStatus: "published",
            deprecated: false,
            specificationFilename: "api-product-1.0.yml",
            specificationContents: "- openapi: 3.0.0"
        );

        var outputDirectory = @"c:\temp\output";

        await dumpService.Dump(outputDirectory);

        await testHost.Then.DumpedFile.ShouldHaveApiProductVersion(
            outputDirectory,
            "API Product",
            "v1.0",
            "published",
            false,
            "api-product-1.0.yml",
            "- openapi: 3.0.0"
        );
    }

    [Fact]
    public async Task VersionWithoutSpecificationIsDumped()
    {
        using var testHost = new TestHost.TestHost();

        var dumpService = testHost.GetRequiredService<DumpService>();

        var productId = Guid.NewGuid().ToString();
        testHost.Given.AnExistingApiProduct(productId: productId, name: "API Product");
        testHost.Given.AnExistingApiProductVersion(apiProductId: productId, name: "v1.0", publishStatus: "published", deprecated: false);

        var outputDirectory = @"c:\temp\output";

        await dumpService.Dump(outputDirectory);

        await testHost.Then.DumpedFile.ShouldHaveApiProductVersion(outputDirectory, "API Product", "v1.0", "published", false);
    }

    [Fact]
    public async Task PortalSettingsAreDumped()
    {
        using var testHost = new TestHost.TestHost();

        var dumpService = testHost.GetRequiredService<DumpService>();

        var portalId = Guid.NewGuid().ToString();
        testHost.Given.AnExistingDevPortal(
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

        await dumpService.Dump(outputDirectory);

        await testHost.Then.DumpedFile.ShouldHavePortal(outputDirectory, "default", true, false, false, false, null, null);
    }
}
