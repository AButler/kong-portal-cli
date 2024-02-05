using System.IO.Abstractions;
using System.Text.Json;
using Kong.Portal.CLI.Services.Metadata;

namespace Kong.Portal.CLI.Services;

internal class SourceDirectoryReader(IFileSystem fileSystem)
{
    public async Task<SourceData> Read(string inputDirectory)
    {
        var sourceData = new SourceData(inputDirectory);

        await ReadApiProducts(sourceData);

        return sourceData;
    }

    private async Task ReadApiProducts(SourceData sourceData)
    {
        var apiProductDirectory = Path.Combine(sourceData.InputDirectory, "api-products");

        var apiProductFiles = fileSystem.Directory.GetFiles(apiProductDirectory, "api-product.json", SearchOption.AllDirectories);

        foreach (var apiProductFile in apiProductFiles)
        {
            var apiProductMetadata = await JsonSerializer.DeserializeAsync<ApiProductMetadata>(
                fileSystem.File.OpenRead(apiProductFile),
                MetadataSerializerSettings.SerializerOptions
            );

            if (apiProductMetadata == null)
            {
                throw new SyncException($"Cannot read API Product: {apiProductFile}");
            }

            sourceData.ApiProducts.Add(apiProductMetadata);
        }
    }
}
