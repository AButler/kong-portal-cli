namespace Kong.Portal.CLI.Services;

internal record ApiProductVersionMetadata(
    string SyncId,
    string Name,
    MetadataPublishStatus PublishStatus,
    bool Deprecated,
    string? SpecificationFilename
);
