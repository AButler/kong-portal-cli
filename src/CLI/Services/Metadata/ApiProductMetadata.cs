namespace Kong.Portal.CLI.Services.Metadata;

internal record ApiProductMetadata(string SyncId, string Name, string Description, IReadOnlyCollection<string> Portals, LabelDictionary Labels);
