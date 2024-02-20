using System.IO.Abstractions;
using Kong.Portal.CLI;
using Kong.Portal.CLI.Services;
using Kong.Portal.CLI.Services.Metadata;

namespace CLI.UnitTests.TestHost;

internal class FileGivenSteps(IFileSystem fileSystem, MetadataSerializer metadataSerializer)
{
    private readonly SyncIdGenerator _apiProductSyncIdGenerator = new();
    private readonly SyncIdGenerator _apiProductVersionSyncIdGenerator = new();

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

    public async Task AnExistingApiProductVersion(
        string inputDirectory,
        string apiProductSyncId,
        Discretionary<string> syncId = default,
        Discretionary<string> name = default,
        Discretionary<string> publishStatus = default,
        Discretionary<bool> deprecated = default,
        Discretionary<string?> specificationFilename = default
    )
    {
        var versionName = name.GetValueOrDefault("VERSION");
        var versionSyncId = syncId.GetValueOrDefault(_apiProductVersionSyncIdGenerator.Generate(versionName));

        var metadata = new ApiProductVersionMetadata(
            versionSyncId,
            versionName,
            publishStatus.GetValueOrDefault("published") == "published"
                ? ApiProductVersionMetadataPublishStatus.Published
                : ApiProductVersionMetadataPublishStatus.Unpublished,
            deprecated.GetValueOrDefault(false),
            specificationFilename.GetValueOrDefault(null)
        );

        var apiProductVersionDirectory = Path.Combine(inputDirectory, "api-products", apiProductSyncId, "versions", versionSyncId);
        fileSystem.Directory.EnsureDirectory(apiProductVersionDirectory);

        var metadataFilename = Path.Combine(apiProductVersionDirectory, "version.json");
        await metadataSerializer.SerializeAsync(metadataFilename, metadata);
    }
}
