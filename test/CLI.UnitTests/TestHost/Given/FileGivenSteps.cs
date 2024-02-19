using System.IO.Abstractions;
using Kong.Portal.CLI;
using Kong.Portal.CLI.Services;
using Kong.Portal.CLI.Services.Metadata;

namespace CLI.UnitTests.TestHost;

internal class FileGivenSteps(IFileSystem fileSystem, MetadataSerializer metadataSerializer)
{
    private readonly SyncIdGenerator _apiProductSyncIdGenerator = new();

    public async Task AnExistingApiProduct(
        string inputDirectory,
        Discretionary<string> name = default,
        Discretionary<string> syncId = default,
        Discretionary<string> description = default,
        IDictionary<string, string>? labels = null
    )
    {
        var productName = name.GetValueOrDefault(Guid.NewGuid().ToString());
        var productSyncId = syncId.GetValueOrDefault(_apiProductSyncIdGenerator.Generate(productName));

        var metadata = new ApiProductMetadata(
            productSyncId,
            productName,
            description.GetValueOrDefault(Guid.NewGuid().ToString()),
            labels.ToLabelDictionary()
        );

        var apiProductDirectory = Path.Combine(inputDirectory, "api-products", productSyncId);
        fileSystem.Directory.EnsureDirectory(apiProductDirectory);

        var metadataFilename = Path.Combine(apiProductDirectory, "api-product.json");
        await metadataSerializer.SerializeAsync(metadataFilename, metadata);
    }
}
