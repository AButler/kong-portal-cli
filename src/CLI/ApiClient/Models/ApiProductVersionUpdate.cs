namespace Kong.Portal.CLI.ApiClient.Models;

internal record ApiProductVersionUpdate(string Name, ApiVersionPublishStatus PublishStatus, bool Deprecated);
