namespace Kong.Portal.CLI.ApiClient.Models;

internal record ApiProductUpdate(string Name, string Description, IReadOnlyCollection<string> PortalIds, LabelDictionary Labels);
