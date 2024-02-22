namespace Kong.Portal.CLI.Services;

internal record ApiProductMetadata(string SyncId, string Name, string Description, IReadOnlyCollection<string> Portals, LabelDictionary Labels);
