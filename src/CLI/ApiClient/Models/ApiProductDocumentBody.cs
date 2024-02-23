using System.Text.Json.Serialization;

namespace Kong.Portal.CLI.ApiClient.Models;

internal record ApiProductDocumentBody(
    string Id,
    string? ParentDocumentId,
    string Slug,
    [property: JsonIgnore] string FullSlug,
    ApiPublishStatus Status,
    string Title,
    [property: JsonPropertyName("str_md_content")] string MarkdownContent
);
