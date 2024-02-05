using System.IO.Abstractions;
using System.Text.Json;
using System.Text.Json.Nodes;
using FluentAssertions;

namespace CLI.UnitTests.TestHost;

public class DumpedFileSteps(IFileSystem fileSystem)
{
    public async Task ShouldHaveApiProduct(
        string outputDirectory,
        string apiProductName,
        string apiProductDescription,
        Dictionary<string, string> labels
    )
    {
        var apiProductDirectory = Path.Combine(outputDirectory, "api-products", apiProductName);
        var apiProductMetadataFile = Path.Combine(apiProductDirectory, "api-product.json");

        DirectoryShouldExist(apiProductDirectory);
        FileShouldExist(apiProductMetadataFile);

        var json = await JsonNode.ParseAsync(fileSystem.File.OpenRead(apiProductMetadataFile));

        json.Should().NotBeNull();
        json!["name"].Should().NotBeNull();
        json["name"]!.GetValueKind().Should().Be(JsonValueKind.String);
        json["name"]!.GetValue<string>().Should().Be(apiProductName);
        json["description"].Should().NotBeNull();
        json["description"]!.GetValueKind().Should().Be(JsonValueKind.String);
        json["description"]!.GetValue<string>().Should().Be(apiProductDescription);
        json["labels"].Should().NotBeNull();
        json["labels"]!.GetValueKind().Should().Be(JsonValueKind.Object);

        foreach (var label in labels)
        {
            var labelNode = json["labels"]![label.Key];

            labelNode.Should().NotBeNull();
            labelNode!.GetValueKind().Should().Be(JsonValueKind.String);
            labelNode.GetValue<string>().Should().Be(label.Value);
        }
    }

    public async Task ShouldHaveApiProductDocument(
        string outputDirectory,
        string apiProductName,
        string documentSlug,
        string documentTitle,
        string documentContents
    )
    {
        var documentsDirectory = Path.Combine(outputDirectory, "api-products", apiProductName, "documents");

        var contentDocumentFilename = Path.Combine(documentsDirectory, $"{documentSlug}.md");
        FileShouldHaveContents(contentDocumentFilename, documentContents);

        var metadataFilename = Path.Combine(documentsDirectory, $"{documentSlug}.json");
        FileShouldExist(metadataFilename);
        var json = await JsonNode.ParseAsync(fileSystem.File.OpenRead(metadataFilename));

        json.Should().NotBeNull();
        json!["title"].Should().NotBeNull();
        json["title"]!.GetValueKind().Should().Be(JsonValueKind.String);
        json["title"]!.GetValue<string>().Should().Be(documentTitle);
        json["slug"].Should().NotBeNull();
        json["slug"]!.GetValueKind().Should().Be(JsonValueKind.String);
        json["slug"]!.GetValue<string>().Should().Be(documentSlug);
        json["status"].Should().NotBeNull();
        json["status"]!.GetValueKind().Should().Be(JsonValueKind.String);
        json["status"]!.GetValue<string>().Should().Be("published");
    }

    public async Task ShouldHaveApiProductVersion(
        string outputDirectory,
        string apiProductName,
        string name,
        string publishStatus,
        bool deprecated,
        string? specificationFilename = null,
        string? specificationContents = null
    )
    {
        var versionDirectory = Path.Combine(outputDirectory, "api-products", apiProductName, "versions", name);

        DirectoryShouldExist(versionDirectory);

        if (specificationFilename != null)
        {
            var specificationFullFilename = Path.Combine(versionDirectory, specificationFilename);
            FileShouldHaveContents(specificationFullFilename, specificationContents!);
        }

        var metadataFilename = Path.Combine(versionDirectory, "version.json");
        FileShouldExist(metadataFilename);
        var json = await JsonNode.ParseAsync(fileSystem.File.OpenRead(metadataFilename));

        json.Should().NotBeNull();
        json!["name"].Should().NotBeNull();
        json["name"]!.GetValueKind().Should().Be(JsonValueKind.String);
        json["name"]!.GetValue<string>().Should().Be(name);
        json["publishStatus"].Should().NotBeNull();
        json["publishStatus"]!.GetValueKind().Should().Be(JsonValueKind.String);
        json["publishStatus"]!.GetValue<string>().Should().Be(publishStatus);
        json["deprecated"].Should().NotBeNull();
        json["deprecated"]!.GetValueKind().Should().BeOneOf(JsonValueKind.False, JsonValueKind.True);
        json["deprecated"]!.GetValue<bool>().Should().Be(deprecated);
        if (specificationFilename != null)
        {
            json["specificationFilename"].Should().NotBeNull();
            json["specificationFilename"]!.GetValueKind().Should().Be(JsonValueKind.String);
            json["specificationFilename"]!.GetValue<string>().Should().Be($"/{specificationFilename}");
        }
        else
        {
            json["specificationFilename"].Should().BeNull();
        }
    }

    private void DirectoryShouldExist(string path)
    {
        fileSystem.Directory.Exists(path).Should().BeTrue($"directory does not exist: {path}");
    }

    private void FileShouldExist(string path)
    {
        fileSystem.File.Exists(path).Should().BeTrue($"file does not exist: {path}");
    }

    private void FileShouldHaveContents(string path, string contents)
    {
        FileShouldExist(path);
        fileSystem.File.ReadAllText(path).Should().Be(contents);
    }
}
