using System.IO.Abstractions;
using Kong.Portal.CLI.Services.Metadata;

namespace Kong.Portal.CLI.Services;

internal class SourceDirectoryReader(MetadataSerializer metadataSerializer, IFileSystem fileSystem)
{
    public async Task<SourceData> Read(string inputDirectory)
    {
        var sourceData = new SourceData(inputDirectory);

        await ReadApiProducts(sourceData);

        await ReadPortals(sourceData);

        return sourceData;
    }

    private async Task ReadPortals(SourceData sourceData)
    {
        var portalsDirectory = Path.Combine(sourceData.InputDirectory, "portals");
        if (!fileSystem.Directory.Exists(portalsDirectory))
        {
            return;
        }

        var portalFiles = fileSystem.Directory.GetFiles(portalsDirectory, "portal.json", SearchOption.AllDirectories);

        foreach (var portalFile in portalFiles)
        {
            await ReadPortal(sourceData, portalFile);
        }
    }

    private async Task ReadPortal(SourceData sourceData, string portalFile)
    {
        var portalMetadata = await metadataSerializer.DeserializeAsync<PortalMetadata>(portalFile);

        if (portalMetadata == null)
        {
            throw new SyncException($"Cannot read portal: {portalFile}");
        }

        var portalAppearanceFile = Path.Combine(Path.GetDirectoryName(portalFile)!, "appearance.json");
        var portalAppearanceMetadata = await metadataSerializer.DeserializeAsync<PortalAppearanceMetadata>(portalAppearanceFile);

        if (portalAppearanceMetadata == null)
        {
            throw new SyncException($"Cannot read portal appearance: {portalAppearanceFile}");
        }

        sourceData.Portals.Add(portalMetadata);
        sourceData.PortalAppearances.Add(portalMetadata.Name, portalAppearanceMetadata);
    }

    private async Task ReadApiProducts(SourceData sourceData)
    {
        var apiProductDirectory = Path.Combine(sourceData.InputDirectory, "api-products");
        if (!fileSystem.Directory.Exists(apiProductDirectory))
        {
            return;
        }

        var apiProductFiles = fileSystem.Directory.GetFiles(apiProductDirectory, "api-product.json", SearchOption.AllDirectories);

        foreach (var apiProductFile in apiProductFiles)
        {
            await ReadApiProduct(sourceData, apiProductFile);
        }
    }

    private async Task ReadApiProduct(SourceData sourceData, string apiProductFile)
    {
        var apiProductMetadata = await metadataSerializer.DeserializeAsync<ApiProductMetadata>(apiProductFile);

        if (apiProductMetadata == null)
        {
            throw new SyncException($"Cannot read API Product: {apiProductFile}");
        }

        sourceData.ApiProducts.Add(apiProductMetadata);
        sourceData.ApiProductDocuments.Add(apiProductMetadata.SyncId, []);
        sourceData.ApiProductDocumentContents.Add(apiProductMetadata.SyncId, []);
        sourceData.ApiProductVersions.Add(apiProductMetadata.SyncId, []);
        sourceData.ApiProductVersionSpecifications.Add(apiProductMetadata.SyncId, []);

        var versionsDirectory = Path.Combine(Path.GetDirectoryName(apiProductFile)!, "versions");
        if (fileSystem.Directory.Exists(versionsDirectory))
        {
            var versionFiles = fileSystem.Directory.GetFiles(versionsDirectory, "version.json", SearchOption.AllDirectories);

            foreach (var versionFile in versionFiles)
            {
                await ReadApiProductVersion(sourceData, apiProductMetadata, versionFile);
            }
        }

        var documentsDirectory = Path.Combine(Path.GetDirectoryName(apiProductFile)!, "documents");
        if (fileSystem.Directory.Exists(documentsDirectory))
        {
            var documentFiles = fileSystem.Directory.GetFiles(documentsDirectory, "*.json", SearchOption.AllDirectories);

            foreach (var documentFile in documentFiles)
            {
                await ReadDocument(sourceData, apiProductMetadata, documentFile);
            }
        }
    }

    private async Task ReadDocument(SourceData sourceData, ApiProductMetadata apiProductMetadata, string documentFile)
    {
        var apiProductDocumentMetadata = await metadataSerializer.DeserializeAsync<ApiProductDocumentMetadata>(documentFile);

        if (apiProductDocumentMetadata == null)
        {
            throw new SyncException($"Cannot read API Product Document: {documentFile}");
        }

        sourceData.ApiProductDocuments[apiProductMetadata.SyncId].Add(apiProductDocumentMetadata);

        var documentContentsFilename = Path.ChangeExtension(documentFile, ".md");
        var documentContents = await fileSystem.File.ReadAllTextAsync(documentContentsFilename);

        sourceData.ApiProductDocumentContents[apiProductMetadata.SyncId].Add(apiProductDocumentMetadata.FullSlug, documentContents);
    }

    private async Task ReadApiProductVersion(SourceData sourceData, ApiProductMetadata apiProductMetadata, string versionFile)
    {
        var apiProductVersionMetadata = await metadataSerializer.DeserializeAsync<ApiProductVersionMetadata>(versionFile);

        if (apiProductVersionMetadata == null)
        {
            throw new SyncException($"Cannot read API Product Version: {versionFile}");
        }

        sourceData.ApiProductVersions[apiProductMetadata.SyncId].Add(apiProductVersionMetadata);

        if (apiProductVersionMetadata.SpecificationFilename != null)
        {
            var specificationFilename = Path.Combine(
                Path.GetDirectoryName(versionFile)!,
                apiProductVersionMetadata.SpecificationFilename.TrimStart('/')
            );
            var specificationContents = await fileSystem.File.ReadAllTextAsync(specificationFilename);

            sourceData.ApiProductVersionSpecifications[apiProductMetadata.SyncId].Add(apiProductVersionMetadata.SyncId, specificationContents);
        }
        else
        {
            sourceData.ApiProductVersionSpecifications[apiProductMetadata.SyncId].Add(apiProductVersionMetadata.SyncId, null);
        }
    }
}
