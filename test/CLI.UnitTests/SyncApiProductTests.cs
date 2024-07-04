using Kong.Portal.CLI;

namespace CLI.UnitTests;

public class SyncApiProductTests
{
    [Fact]
    public async Task NoSourceDataAndNoServerDataDoesNothing()
    {
        using var testHost = new TestHost.TestHost();

        var syncService = testHost.GetRequiredService<SyncService>();

        await syncService.Sync(@"c:\temp\input", testHost.ApiClientOptions);

        testHost.Then.Api.ShouldNotHaveReceivedAnyUpdates();
    }

    [Fact]
    public async Task ApiProductDescriptionChangeUpdates()
    {
        using var testHost = new TestHost.TestHost();

        var productId = Guid.NewGuid().ToString();

        testHost.Given.Api.AnExistingApiProduct(
            productId: productId,
            name: "API Product 1",
            description: "This is API Product 1",
            labels: new Dictionary<string, string> { ["Foo"] = "Bar" }
        );

        await testHost.Given.File.AnExistingApiProduct(
            inputDirectory: @"c:\temp\input",
            name: "API Product 1",
            description: "This is API Product One",
            labels: new Dictionary<string, string> { ["Foo"] = "Bar" }
        );

        var syncService = testHost.GetRequiredService<SyncService>();

        await syncService.Sync(@"c:\temp\input", testHost.ApiClientOptions);

        testHost.Then.Api.ApiProductShouldHaveBeenUpdated(productId);
    }

    [Fact]
    public async Task NewApiProductIsCreated()
    {
        using var testHost = new TestHost.TestHost();

        await testHost.Given.File.AnExistingApiProduct(
            inputDirectory: @"c:\temp\input",
            name: "Api Product 1",
            description: "This is API Product One",
            labels: new Dictionary<string, string> { ["Foo"] = "Bar" }
        );

        var syncService = testHost.GetRequiredService<SyncService>();

        await syncService.Sync(@"c:\temp\input", testHost.ApiClientOptions);

        testHost.Then.Api.ApiProductShouldHaveBeenCreated("api-product-1");
    }

    [Fact]
    public async Task NewApiProductWithVariableIsCreated()
    {
        using var testHost = new TestHost.TestHost();

        await testHost.Given.File.AnExistingApiProduct(
            inputDirectory: @"c:\temp\input",
            syncId: "api-product-${{ var PRODUCT_NUMBER }}",
            name: "Api Product ${{ var PRODUCT_NUMBER }}",
            description: "This is API Product One",
            labels: new Dictionary<string, string> { ["Foo"] = "Bar" }
        );

        var syncService = testHost.GetRequiredService<SyncService>();

        var variables = new Dictionary<string, string> { ["PRODUCT_NUMBER"] = "1" };

        await syncService.Sync(@"c:\temp\input", variables, true, testHost.ApiClientOptions);

        testHost.Then.Api.ApiProductShouldHaveBeenCreated("api-product-1");
    }

    [Fact]
    public async Task ApiProductNameChangeUpdates()
    {
        using var testHost = new TestHost.TestHost();

        var productId = Guid.NewGuid().ToString();

        testHost.Given.Api.AnExistingApiProduct(
            productId: productId,
            name: "API Product 1",
            description: "This is API Product One",
            labels: new Dictionary<string, string> { ["Foo"] = "Bar", [Constants.SyncIdLabel] = "api-product-1" }
        );

        await testHost.Given.File.AnExistingApiProduct(
            inputDirectory: @"c:\temp\input",
            name: "API Product One",
            syncId: "api-product-1",
            description: "This is API Product One",
            labels: new Dictionary<string, string> { ["Foo"] = "Bar" }
        );

        var syncService = testHost.GetRequiredService<SyncService>();

        await syncService.Sync(@"c:\temp\input", testHost.ApiClientOptions);

        testHost.Then.Api.ApiProductShouldHaveBeenUpdated(productId);
    }

    [Fact]
    public async Task ApiProductAndVersionCreated()
    {
        using var testHost = new TestHost.TestHost();

        await testHost.Given.File.AnExistingApiProduct(inputDirectory: @"c:\temp\input", syncId: "api-product-1");

        await testHost.Given.File.AnExistingApiProductVersion(
            inputDirectory: @"c:\temp\input",
            apiProductSyncId: "api-product-1",
            syncId: "1.0.0",
            name: "1.0.0",
            publishStatus: "published",
            deprecated: false
        );

        var syncService = testHost.GetRequiredService<SyncService>();

        await syncService.Sync(@"c:\temp\input", testHost.ApiClientOptions);

        testHost.Then.Api.ApiProductShouldHaveBeenCreated("api-product-1");

        var apiProductId = await testHost.Then.Api.GetApiProductId("api-product-1");
        testHost.Then.Api.ApiProductVersionShouldHaveBeenCreated(apiProductId);
    }

    [Fact]
    public async Task NewApiDocumentAdded()
    {
        using var testHost = new TestHost.TestHost();

        await testHost.Given.File.AnExistingApiProduct(inputDirectory: @"c:\temp\input", syncId: "api-product-1");
        await testHost.Given.File.AnExistingApiProductDocument(
            inputDirectory: @"c:\temp\input",
            apiProductSyncId: "api-product-1",
            title: "New Document",
            slug: "new-doc",
            fullSlug: "new-doc",
            status: "published"
        );

        var syncService = testHost.GetRequiredService<SyncService>();

        await syncService.Sync(@"c:\temp\input", testHost.ApiClientOptions);

        var apiProductId = await testHost.Then.Api.GetApiProductId("api-product-1");
        testHost.Then.Api.ApiProductDocumentShouldHaveBeenCreated(apiProductId);
    }

    [Fact]
    public async Task NewApiDocumentWithVariableAdded()
    {
        using var testHost = new TestHost.TestHost();

        await testHost.Given.File.AnExistingApiProduct(inputDirectory: @"c:\temp\input", syncId: "api-product-1");
        await testHost.Given.File.AnExistingApiProductDocument(
            inputDirectory: @"c:\temp\input",
            apiProductSyncId: "api-product-1",
            title: "New Document",
            slug: "new-doc",
            fullSlug: "new-doc",
            status: "published",
            content: "# Title\n![Image!](${{ var BASE_URL }}/assets/images/sample.jpg \"Sample Image\")\n\nMore info here"
        );

        var syncService = testHost.GetRequiredService<SyncService>();

        var variables = new Dictionary<string, string> { ["BASE_URL"] = "http://foo.com" };

        await syncService.Sync(@"c:\temp\input", variables, true, testHost.ApiClientOptions);

        var apiProductId = await testHost.Then.Api.GetApiProductId("api-product-1");
        testHost.Then.Api.ApiProductDocumentShouldHaveBeenCreated(apiProductId, "http://foo.com/assets/images/sample.jpg");
    }
}
