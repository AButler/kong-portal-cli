using System.IO.Abstractions;
using System.Text.Json;
using System.Text.Json.Nodes;
using Kong.Portal.CLI;

namespace CLI.UnitTests.TestHost;

internal class DumpedFileThenSteps(IFileSystem fileSystem)
{
    public async Task ShouldHaveApiProduct(string outputDirectory, string syncId, string name, string description, Dictionary<string, string> labels)
    {
        var apiProductDirectory = Path.Combine(outputDirectory, "api-products", syncId);
        var apiProductMetadataFile = Path.Combine(apiProductDirectory, "api-product.json");

        DirectoryShouldExist(apiProductDirectory);
        FileShouldExist(apiProductMetadataFile);

        var json = await JsonNode.ParseAsync(fileSystem.File.OpenRead(apiProductMetadataFile));

        json.ShouldHaveStringProperty("sync_id", syncId);
        json.ShouldHaveStringProperty("name", name);
        json.ShouldHaveStringProperty("description", description);
        json.ShouldHaveMapProperty("labels", labels.ToNullableValueDictionary());
    }

    public async Task ShouldHaveApiProductDocument(
        string outputDirectory,
        string apiProductSyncId,
        string documentSlug,
        string fullSlug,
        string documentTitle,
        string documentContents
    )
    {
        var documentsDirectory = Path.Combine(outputDirectory, "api-products", apiProductSyncId, "documents");

        var contentDocumentFilename = Path.Combine(documentsDirectory, $"{fullSlug}.md");
        FileShouldHaveContents(contentDocumentFilename, documentContents);

        var metadataFilename = Path.Combine(documentsDirectory, $"{fullSlug}.json");
        FileShouldExist(metadataFilename);
        var json = await JsonNode.ParseAsync(fileSystem.File.OpenRead(metadataFilename));

        json.ShouldHaveStringProperty("title", documentTitle);
        json.ShouldHaveStringProperty("slug", documentSlug);
        json.ShouldHaveStringProperty("full_slug", fullSlug);
        json.ShouldHaveStringProperty("status", "published");
    }

    public async Task ShouldHaveApiProductVersion(
        string outputDirectory,
        string apiProductSyncId,
        string apiProductVersionSyncId,
        string name,
        string publishStatus,
        bool deprecated,
        string? specificationFilename = null,
        string? specificationContents = null
    )
    {
        var versionDirectory = Path.Combine(outputDirectory, "api-products", apiProductSyncId, "versions", apiProductVersionSyncId);

        DirectoryShouldExist(versionDirectory);

        if (specificationFilename != null)
        {
            var specificationFullFilename = Path.Combine(versionDirectory, specificationFilename);
            FileShouldHaveContents(specificationFullFilename, specificationContents!);
        }

        var metadataFilename = Path.Combine(versionDirectory, "version.json");
        FileShouldExist(metadataFilename);
        var json = await JsonNode.ParseAsync(fileSystem.File.OpenRead(metadataFilename));

        json.ShouldHaveStringProperty("sync_id", apiProductVersionSyncId);
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
        string portalName,
        bool isPublic,
        bool rbacEnabled,
        bool autoApproveApplications,
        bool autoApproveDevelopers,
        string? customDomain,
        string? customClientDomain
    )
    {
        var portalDirectory = Path.Combine(outputDirectory, "portals", portalName);

        DirectoryShouldExist(portalDirectory);

        var metadataFilename = Path.Combine(portalDirectory, "portal.json");
        FileShouldExist(metadataFilename);

        var json = await JsonNode.ParseAsync(fileSystem.File.OpenRead(metadataFilename));

        json.ShouldHaveStringProperty("name", portalName);
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

    public async Task ShouldHavePortalAppearance(
        string outputDirectory,
        string portalName,
        string themeName,
        bool useCustomFonts,
        string? customFontBase,
        string? customFontCode,
        string? customFontHeadings,
        string? welcomeMessage,
        string? primaryHeader,
        string? faviconImage,
        string? faviconImageName,
        string? logoImage,
        string? logoImageName,
        string? catalogCoverImage,
        string? catalogCoverImageName
    )
    {
        var portalDirectory = Path.Combine(outputDirectory, "portals", portalName);
        DirectoryShouldExist(portalDirectory);

        var metadataFilename = Path.Combine(portalDirectory, "appearance.json");
        FileShouldExist(metadataFilename);

        var json = await JsonNode.ParseAsync(fileSystem.File.OpenRead(metadataFilename));

        json.ShouldHaveStringProperty("theme_name", themeName);
        json.ShouldHaveBooleanProperty("use_custom_fonts", useCustomFonts);

        json.ShouldHaveMapProperty(
            "custom_fonts",
            new Dictionary<string, string?>
            {
                ["base"] = customFontBase,
                ["code"] = customFontCode,
                ["headings"] = customFontHeadings
            }
        );

        json.ShouldHaveMapProperty(
            "text",
            new Dictionary<string, string?> { ["welcome_message"] = welcomeMessage, ["primary_header"] = primaryHeader }
        );

        json.ShouldHaveMapProperty(
            "images",
            new Dictionary<string, string?>
            {
                ["favicon"] = faviconImageName,
                ["logo"] = logoImageName,
                ["catalog_cover"] = catalogCoverImageName
            }
        );

        if (faviconImage != null && faviconImageName != null)
        {
            var imageFilename = Path.Combine(portalDirectory, faviconImageName);
            FileShouldExist(imageFilename);
            FileShouldHaveContents(imageFilename, DataUriHelpers.GetData(faviconImage));
        }

        if (logoImage != null && logoImageName != null)
        {
            var imageFilename = Path.Combine(portalDirectory, logoImageName);
            FileShouldExist(imageFilename);
            FileShouldHaveContents(imageFilename, DataUriHelpers.GetData(logoImage));
        }

        if (catalogCoverImage != null && catalogCoverImageName != null)
        {
            var imageFilename = Path.Combine(portalDirectory, catalogCoverImageName);
            FileShouldExist(imageFilename);
            FileShouldHaveContents(imageFilename, DataUriHelpers.GetData(catalogCoverImage));
        }
    }

    public async Task ShouldHavePortalAuthSettings(
        string outputDirectory,
        string portalName,
        bool basicAuthEnabled,
        bool oidcAuthEnabled,
        bool oidcTeamMappingEnabled,
        bool konnectMappingEnabled,
        OidcAuthSettings? oidcConfig
    )
    {
        var portalDirectory = Path.Combine(outputDirectory, "portals", portalName);
        DirectoryShouldExist(portalDirectory);

        var metadataFilename = Path.Combine(portalDirectory, "authentication-settings.json");
        FileShouldExist(metadataFilename);

        var json = await JsonNode.ParseAsync(fileSystem.File.OpenRead(metadataFilename));

        json.ShouldHaveBooleanProperty("basic_auth_enabled", basicAuthEnabled);
        json.ShouldHaveBooleanProperty("oidc_auth_enabled", oidcAuthEnabled);
        json.ShouldHaveBooleanProperty("oidc_team_mapping_enabled", oidcTeamMappingEnabled);
        json.ShouldHaveBooleanProperty("konnect_mapping_enabled", konnectMappingEnabled);

        if (oidcConfig == null)
        {
            json.ShouldHaveNullProperty("oidc_config");
        }
        else
        {
            json.ShouldHaveObjectProperty("oidc_config");
            var oidcJson = json!["oidc_config"];

            oidcJson.ShouldHaveStringProperty("issuer", oidcConfig.Issuer);
            oidcJson.ShouldHaveStringProperty("client_id", oidcConfig.ClientId);
            oidcJson.ShouldHaveStringArrayProperty("scopes", oidcConfig.Scopes);
            oidcJson.ShouldHaveMapProperty(
                "claim_mappings",
                new Dictionary<string, string?>
                {
                    ["name"] = oidcConfig.ClaimMappings.Name,
                    ["email"] = oidcConfig.ClaimMappings.Email,
                    ["groups"] = oidcConfig.ClaimMappings.Groups
                }
            );
        }
    }

    public async Task ShouldHavePortalProduct(string outputDirectory, string portalName, string apiProductSyncId)
    {
        var portalDirectory = Path.Combine(outputDirectory, "portals", portalName);
        DirectoryShouldExist(portalDirectory);

        var metadataFilename = Path.Combine(portalDirectory, "api-products.json");
        FileShouldExist(metadataFilename);

        var json = await JsonNode.ParseAsync(fileSystem.File.OpenRead(metadataFilename));

        json.ShouldHaveArrayProperty("api_products");

        var apiProductsJson = (JsonArray)json!["api_products"]!;

        var apiProducts = apiProductsJson.GetValues<string>();

        apiProducts.Should().Contain(apiProductSyncId);
    }

    public async Task ShouldHaveNoPortalTeams(string outputDirectory, string portalName)
    {
        var portalDirectory = Path.Combine(outputDirectory, "portals", portalName);
        DirectoryShouldExist(portalDirectory);

        var metadataFilename = Path.Combine(portalDirectory, "teams.json");
        FileShouldExist(metadataFilename);

        var json = await JsonNode.ParseAsync(fileSystem.File.OpenRead(metadataFilename));

        json.ShouldHaveArrayPropertyWithLength("teams", 0);
    }

    public async Task ShouldHavePortalTeam(string outputDirectory, string portalName, string teamName, string teamDescription)
    {
        var portalDirectory = Path.Combine(outputDirectory, "portals", portalName);
        DirectoryShouldExist(portalDirectory);

        var metadataFilename = Path.Combine(portalDirectory, "teams.json");
        FileShouldExist(metadataFilename);

        var json = await JsonNode.ParseAsync(fileSystem.File.OpenRead(metadataFilename));

        json.ShouldHaveArrayProperty("teams");

        var teams = (JsonArray)json!["teams"]!;

        foreach (var node in teams)
        {
            if (node == null || node.GetValueKind() != JsonValueKind.Object)
            {
                continue;
            }

            var nameNode = node["name"];
            if (nameNode == null || nameNode.GetValueKind() != JsonValueKind.String || nameNode.GetValue<string>() != teamName)
            {
                continue;
            }

            node.ShouldHaveStringProperty("description", teamDescription);
            return;
        }

        Assert.Fail("Team not found");
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

    private void FileShouldHaveContents(string path, byte[] contents)
    {
        FileShouldExist(path);

        var actualContents = Convert.ToBase64String(fileSystem.File.ReadAllBytes(path));
        var expectedContents = Convert.ToBase64String(contents);

        actualContents.Should().Be(expectedContents);
    }
}
