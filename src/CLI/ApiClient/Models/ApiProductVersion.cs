namespace Kong.Portal.CLI.ApiClient.Models;

internal record ApiProductVersion(string Id, string Name, string PublishStatus, bool Deprecated) : IEntity;
