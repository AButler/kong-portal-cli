namespace Kong.Portal.CLI.Services.Metadata;

internal record ApiProductVersionMetadata(string SyncId, string Name, string PublishStatus, bool Deprecated, string? SpecificationFilename);
