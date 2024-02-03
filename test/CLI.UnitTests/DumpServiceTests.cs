using System.Text.Json;
using System.Text.Json.Nodes;
using FluentAssertions;
using Kong.Portal.CLI.Services;

namespace CLI.UnitTests;

public class DumpServiceTests
{
    [Fact]
    public async Task ApiProductJsonIsDumped()
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

        var apiProductDirectory = Path.Combine(outputDirectory, "api-products", "API Product 1");
        var apiProductMetadataFile = Path.Combine(apiProductDirectory, "api-product.json");

        testHost.Then.DirectoryShouldExist(apiProductDirectory);
        testHost.Then.FileShouldExist(apiProductMetadataFile);

        var json = await JsonNode.ParseAsync(testHost.FileSystem.File.OpenRead(apiProductMetadataFile));

        json.Should().NotBeNull();
        json!["name"].Should().NotBeNull();
        json["name"]!.GetValueKind().Should().Be(JsonValueKind.String);
        json["name"]!.GetValue<string>().Should().Be("API Product 1");
        json["description"].Should().NotBeNull();
        json["description"]!.GetValueKind().Should().Be(JsonValueKind.String);
        json["description"]!.GetValue<string>().Should().Be("This is API Product 1");
        json["labels"].Should().NotBeNull();
        json["labels"]!.GetValueKind().Should().Be(JsonValueKind.Object);
        json["labels"]!["Author"]!.GetValueKind().Should().Be(JsonValueKind.String);
        json["labels"]!["Author"]!.GetValue<string>().Should().Be("Bob Bobertson");
        json["labels"]!["Tag"]!.GetValueKind().Should().Be(JsonValueKind.String);
        json["labels"]!["Tag"]!.GetValue<string>().Should().Be("eCommerce");
    }

    [Fact]
    public async Task MultipleApiProductsJsonAreDumped()
    {
        using var testHost = new TestHost.TestHost();

        testHost.Given.TheKongApiPageSizeIs(2);

        var dumpService = testHost.GetRequiredService<DumpService>();

        for (var i = 1; i < 10; i++)
        {
            testHost.Given.AnExistingApiProduct(name: $"API Product {i}", description: "This is API Product {i}");
        }

        var outputDirectory = @"c:\temp\output";

        await dumpService.Dump(outputDirectory);

        for (var i = 1; i < 10; i++)
        {
            var apiProductDirectory = Path.Combine(outputDirectory, "api-products", $"API Product {i}");
            var apiProductMetadataFile = Path.Combine(apiProductDirectory, "api-product.json");

            testHost.Then.DirectoryShouldExist(apiProductDirectory);
            testHost.Then.FileShouldExist(apiProductMetadataFile);
        }
    }

    [Fact]
    public async Task ApiProductDocumentationIsDumped()
    {
        using var testHost = new TestHost.TestHost();

        var dumpService = testHost.GetRequiredService<DumpService>();

        var productId = Guid.NewGuid().ToString();

        testHost.Given.AnExistingApiProduct(productId: productId, name: "API Product");

        testHost.Given.AnExistingApiProductDocument(productId, "authentication", "How to Authenticate", "# How to Authenticate");

        var outputDirectory = @"c:\temp\output";

        await dumpService.Dump(outputDirectory);

        var documentsDirectory = Path.Combine(outputDirectory, "api-products", "API Product", "documents");

        var authenticationDocument = Path.Combine(documentsDirectory, "authentication.md");
        testHost.Then.FileShouldExist(authenticationDocument);
        testHost.Then.FileContentsShouldBe(authenticationDocument, "# How to Authenticate");

        var authenticationDocumentMetadataFile = Path.Combine(documentsDirectory, "authentication.json");
        testHost.Then.FileShouldExist(authenticationDocumentMetadataFile);
        var json = await JsonNode.ParseAsync(testHost.FileSystem.File.OpenRead(authenticationDocumentMetadataFile));

        json.Should().NotBeNull();
        json!["title"].Should().NotBeNull();
        json["title"]!.GetValueKind().Should().Be(JsonValueKind.String);
        json["title"]!.GetValue<string>().Should().Be("How to Authenticate");
        json["slug"].Should().NotBeNull();
        json["slug"]!.GetValueKind().Should().Be(JsonValueKind.String);
        json["slug"]!.GetValue<string>().Should().Be("authentication");
        json["status"].Should().NotBeNull();
        json["status"]!.GetValueKind().Should().Be(JsonValueKind.String);
        json["status"]!.GetValue<string>().Should().Be("published");
    }
}
