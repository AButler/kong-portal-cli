namespace Kong.Portal.CLI.ApiClient.Models;

internal record ApiProduct(string Id, string Name, string Description, Dictionary<string, string> Labels);
