namespace Kong.Portal.CLI.ApiClient.Models;

internal record ApiProductVersionUpdate(string Name, ApiPublishStatus PublishStatus, bool Deprecated);
