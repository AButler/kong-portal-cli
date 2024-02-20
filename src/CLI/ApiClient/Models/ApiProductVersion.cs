namespace Kong.Portal.CLI.ApiClient.Models;

internal record ApiProductVersion(string Id, string Name, ApiVersionPublishStatus PublishStatus, bool Deprecated);

internal enum ApiVersionPublishStatus
{
    Published,
    Unpublished
}
