namespace Kong.Portal.CLI.Services;

internal record ApiProductVersionMetadata(
    string SyncId,
    string Name,
    ApiProductVersionMetadataPublishStatus PublishStatus,
    bool Deprecated,
    string? SpecificationFilename
);

internal enum ApiProductVersionMetadataPublishStatus
{
    Published,
    Unpublished
}
