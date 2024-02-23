namespace Kong.Portal.CLI.ApiClient.Models;

internal record ApiProductDocumentUpdate(string? ParentDocumentId, string Slug, ApiPublishStatus Status, string Title, string Content);
