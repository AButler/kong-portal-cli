namespace CLI.UnitTests;

public class DumpDocumentsTests
{
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

        await dumpService.Dump(outputDirectory, testHost.ApiClientOptions, TestContext.Current.CancellationToken);

        await testHost.Then.DumpedFile.ShouldHaveApiProductDocument(
            outputDirectory: outputDirectory,
            apiProductSyncId: "api-product",
            documentSlug: "authentication",
            fullSlug: "authentication",
            documentTitle: "How to Authenticate",
            documentContents: "# How to Authenticate"
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

        await dumpService.Dump(outputDirectory, testHost.ApiClientOptions, TestContext.Current.CancellationToken);

        await testHost.Then.DumpedFile.ShouldHaveApiProductDocument(
            outputDirectory: outputDirectory,
            apiProductSyncId: "api-product",
            documentSlug: "authentication",
            fullSlug: "authentication",
            documentTitle: "How to Authenticate",
            documentContents: "# How to Authenticate"
        );

        await testHost.Then.DumpedFile.ShouldHaveApiProductDocument(
            outputDirectory: outputDirectory,
            apiProductSyncId: "api-product",
            documentSlug: "faq",
            fullSlug: "authentication/faq",
            documentTitle: "Frequently Asked Questions",
            documentContents: "# Frequently Asked Questions"
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

        await dumpService.Dump(outputDirectory, testHost.ApiClientOptions, TestContext.Current.CancellationToken);

        for (var i = 0; i < 10; i++)
        {
            await testHost.Then.DumpedFile.ShouldHaveApiProductDocument(
                outputDirectory: outputDirectory,
                apiProductSyncId: "api-product",
                documentSlug: $"doc-{i}",
                fullSlug: $"doc-{i}",
                documentTitle: $"Article {i}",
                documentContents: $"# Article {i}\n\n##Contents\n* Item 1"
            );
        }
    }
}
