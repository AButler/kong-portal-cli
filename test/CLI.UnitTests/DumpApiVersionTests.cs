namespace CLI.UnitTests;

public class DumpApiVersionTests
{
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
            outputDirectory: outputDirectory,
            apiProductSyncId: "api-product",
            apiProductVersionSyncId: "v1.0",
            name: "v1.0",
            publishStatus: "published",
            deprecated: false,
            specificationFilename: "api-product-1.0.yml",
            specificationContents: "- openapi: 3.0.0"
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

        await testHost.Then.DumpedFile.ShouldHaveApiProductVersion(
            outputDirectory: outputDirectory,
            apiProductSyncId: "api-product",
            apiProductVersionSyncId: "v1.0",
            name: "v1.0",
            publishStatus: "published",
            deprecated: false
        );
    }
}
