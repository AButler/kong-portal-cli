namespace Kong.Portal.CLI.ApiClient.Models;

internal record ApiProductVersion(string Id, string Name, ApiPublishStatus PublishStatus, bool Deprecated);
