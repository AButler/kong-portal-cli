using Kong.Portal.CLI;
using Kong.Portal.CLI.Services;

namespace CLI.UnitTests;

public class DumpServiceTests
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
            outputDirectory,
            "api-product-1",
            "API Product 1",
            "This is API Product 1",
            new Dictionary<string, string> { ["Author"] = "Bob Bobertson", ["Tag"] = "eCommerce" }
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
                [Constants.SyncIdLabel] = "different-sync-id"
            }
        );

        var outputDirectory = @"c:\temp\output";

        await dumpService.Dump(outputDirectory, testHost.ApiClientOptions);

        await testHost.Then.DumpedFile.ShouldHaveApiProduct(
            outputDirectory,
            "different-sync-id",
            "API Product 1",
            "This is API Product 1",
            new Dictionary<string, string> { ["Author"] = "Bob Bobertson", ["Tag"] = "eCommerce" }
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
            outputDirectory,
            "api-product-1",
            "API Product",
            "This is API Product 2",
            new Dictionary<string, string> { ["Author"] = "Bob Bobertson", ["Tag"] = "Frontend" }
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
                outputDirectory,
                $"api-product-{i}",
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
        testHost.Given.Api.AnExistingApiProduct(productId: productId, name: "API Product");
        testHost.Given.Api.AnExistingApiProductDocument(
            apiProductId: productId,
            slug: "authentication",
            title: "How to Authenticate",
            content: "# How to Authenticate"
        );

        var outputDirectory = @"c:\temp\output";

        await dumpService.Dump(outputDirectory, testHost.ApiClientOptions);

        await testHost.Then.DumpedFile.ShouldHaveApiProductDocument(
            outputDirectory,
            "api-product",
            "authentication",
            "authentication",
            "How to Authenticate",
            "# How to Authenticate"
        );
    }

    [Fact]
    public async Task NestedDocumentsAreDumped()
    {
        using var testHost = new TestHost.TestHost();

        var dumpService = testHost.GetRequiredService<DumpService>();

        var productId = Guid.NewGuid().ToString();
        testHost.Given.Api.AnExistingApiProduct(productId: productId, name: "API Product");
        testHost.Given.Api.AnExistingApiProductDocument(
            apiProductId: productId,
            slug: "authentication",
            title: "How to Authenticate",
            content: "# How to Authenticate"
        );
        testHost.Given.Api.AnExistingApiProductDocument(
            apiProductId: productId,
            slug: "authentication/faq",
            title: "Frequently Asked Questions",
            content: "# Frequently Asked Questions"
        );

        var outputDirectory = @"c:\temp\output";

        await dumpService.Dump(outputDirectory, testHost.ApiClientOptions);

        await testHost.Then.DumpedFile.ShouldHaveApiProductDocument(
            outputDirectory,
            "api-product",
            "authentication",
            "authentication",
            "How to Authenticate",
            "# How to Authenticate"
        );

        await testHost.Then.DumpedFile.ShouldHaveApiProductDocument(
            outputDirectory,
            "api-product",
            "faq",
            "authentication/faq",
            "Frequently Asked Questions",
            "# Frequently Asked Questions"
        );
    }

    [Fact]
    public async Task MultipleDocumentsAreDumped()
    {
        using var testHost = new TestHost.TestHost();

        testHost.Given.Api.TheKongApiPageSizeIs(2);

        var dumpService = testHost.GetRequiredService<DumpService>();

        var productId = Guid.NewGuid().ToString();
        testHost.Given.Api.AnExistingApiProduct(productId: productId, name: "API Product");

        for (var i = 0; i < 10; i++)
        {
            testHost.Given.Api.AnExistingApiProductDocument(
                apiProductId: productId,
                slug: $"doc-{i}",
                title: $"Article {i}",
                content: $"# Article {i}\n\n##Contents\n* Item 1"
            );
        }

        var outputDirectory = @"c:\temp\output";

        await dumpService.Dump(outputDirectory, testHost.ApiClientOptions);

        for (var i = 0; i < 10; i++)
        {
            await testHost.Then.DumpedFile.ShouldHaveApiProductDocument(
                outputDirectory,
                "api-product",
                $"doc-{i}",
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
        testHost.Given.Api.AnExistingApiProduct(productId: productId, name: "API Product");
        testHost.Given.Api.AnExistingApiProductVersion(
            apiProductId: productId,
            name: "v1.0",
            publishStatus: "published",
            deprecated: false,
            specificationFilename: "api-product-1.0.yml",
            specificationContents: "- openapi: 3.0.0"
        );

        var outputDirectory = @"c:\temp\output";

        await dumpService.Dump(outputDirectory, testHost.ApiClientOptions);

        await testHost.Then.DumpedFile.ShouldHaveApiProductVersion(
            outputDirectory,
            "api-product",
            "v1.0",
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
        testHost.Given.Api.AnExistingApiProduct(productId: productId, name: "API Product");
        testHost.Given.Api.AnExistingApiProductVersion(apiProductId: productId, name: "v1.0", publishStatus: "published", deprecated: false);

        var outputDirectory = @"c:\temp\output";

        await dumpService.Dump(outputDirectory, testHost.ApiClientOptions);

        await testHost.Then.DumpedFile.ShouldHaveApiProductVersion(outputDirectory, "api-product", "v1.0", "v1.0", "published", false);
    }

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
            customClientDomain: null,
            apiProducts: []
        );

        var outputDirectory = @"c:\temp\output";

        await dumpService.Dump(outputDirectory, testHost.ApiClientOptions);

        await testHost.Then.DumpedFile.ShouldHavePortal(outputDirectory, "default", true, false, false, false, null, null, []);
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
            customClientDomain: null,
            apiProducts: [product1Id, product2Id]
        );

        var outputDirectory = @"c:\temp\output";

        await dumpService.Dump(outputDirectory, testHost.ApiClientOptions);

        await testHost.Then.DumpedFile.ShouldHavePortal(
            outputDirectory,
            "default",
            true,
            false,
            false,
            false,
            null,
            null,
            ["api-product-1", "api-product-2"]
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
            customClientDomain: null,
            apiProducts: [product1Id, product2Id]
        );

        var outputDirectory = @"c:\temp\output";

        await dumpService.Dump(outputDirectory, testHost.ApiClientOptions);

        await testHost.Then.DumpedFile.ShouldHavePortal(
            outputDirectory,
            "default",
            true,
            false,
            false,
            false,
            null,
            null,
            ["api-product", "api-product-2"]
        );
    }

    [Fact]
    public async Task PortalAppearanceIsDumped()
    {
        using var testHost = new TestHost.TestHost();

        var dumpService = testHost.GetRequiredService<DumpService>();

        var portalId = Guid.NewGuid().ToString();

        testHost.Given.Api.AnExistingDevPortal(portalId: portalId, name: "default");

        testHost.Given.Api.AnExistingDevPortalAppearance(
            portalId: portalId,
            themeName: "custom",
            useCustomFonts: true,
            customFontBase: "Inter",
            customFontCode: "Source Code Pro",
            customFontHeadings: "Open Sans",
            welcomeMessage: "Welcome to the DevPortal",
            primaryHeader: "This portal contains all the information you could need",
            faviconImage: Icons.Favicon,
            faviconImageName: "favicon.png",
            logoImage: Icons.Logo,
            logoImageName: "logo.png",
            catalogCoverImage: Icons.CatalogCover,
            catalogCoverImageName: "catalog_cover.png"
        );

        var outputDirectory = @"c:\temp\output";

        await dumpService.Dump(outputDirectory, testHost.ApiClientOptions);

        await testHost.Then.DumpedFile.ShouldHavePortalAppearance(
            outputDirectory,
            "default",
            "custom",
            true,
            "Inter",
            "Source Code Pro",
            "Open Sans",
            "Welcome to the DevPortal",
            "This portal contains all the information you could need",
            Icons.Favicon,
            "favicon.png",
            Icons.Logo,
            "logo.png",
            Icons.CatalogCover,
            "catalog_cover.png"
        );
    }
}
