using System.IO.Abstractions;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CLI.UnitTests.TestHost;

public class DumpedFileSteps(IFileSystem fileSystem)
{
    public async Task ShouldHaveApiProduct(string outputDirectory, string name, string description, Dictionary<string, string> labels)
    {
        var apiProductDirectory = Path.Combine(outputDirectory, "api-products", name);
        var apiProductMetadataFile = Path.Combine(apiProductDirectory, "api-product.json");

        DirectoryShouldExist(apiProductDirectory);
        FileShouldExist(apiProductMetadataFile);

        var json = await JsonNode.ParseAsync(fileSystem.File.OpenRead(apiProductMetadataFile));

        json.ShouldHaveStringProperty("name", name);
        json.ShouldHaveStringProperty("description", description);
        json.ShouldHaveMapProperty("labels", labels);
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

        json.ShouldHaveStringProperty("title", documentTitle);
        json.ShouldHaveStringProperty("slug", documentSlug);
        json.ShouldHaveStringProperty("status", "published");
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

        json.ShouldHaveStringProperty("name", name);
        json.ShouldHaveStringProperty("publish_status", publishStatus);
        json.ShouldHaveBooleanProperty("deprecated", deprecated);

        if (specificationFilename != null)
        {
            json.ShouldHaveStringProperty("specification_filename", $"/{specificationFilename}");
        }
        else
        {
            json.ShouldHaveNullProperty("specification_filename");
        }
    }

    public async Task ShouldHavePortal(
        string outputDirectory,
        string name,
        bool isPublic,
        bool rbacEnabled,
        bool autoApproveApplications,
        bool autoApproveDevelopers,
        string? customDomain,
        string? customClientDomain
    )
    {
        var portalDirectory = Path.Combine(outputDirectory, "portals", name);

        DirectoryShouldExist(portalDirectory);

        var metadataFilename = Path.Combine(portalDirectory, "portal.json");
        FileShouldExist(metadataFilename);
        var jsonAsString = fileSystem.File.ReadAllText(metadataFilename);

        var json = await JsonNode.ParseAsync(fileSystem.File.OpenRead(metadataFilename));

        json.ShouldHaveStringProperty("name", name);
        json.ShouldHaveBooleanProperty("is_public", isPublic);
        json.ShouldHaveBooleanProperty("rbac_enabled", rbacEnabled);
        json.ShouldHaveBooleanProperty("auto_approve_developers", autoApproveDevelopers);
        json.ShouldHaveBooleanProperty("auto_approve_applications", autoApproveApplications);

        if (customDomain == null)
        {
            json.ShouldHaveNullProperty("custom_domain");
        }
        else
        {
            json.ShouldHaveStringProperty("custom_domain", customDomain);
        }

        if (customClientDomain == null)
        {
            json.ShouldHaveNullProperty("custom_client_domain");
        }
        else
        {
            json.ShouldHaveStringProperty("custom_client_domain", customClientDomain);
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
