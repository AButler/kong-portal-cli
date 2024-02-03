using System.Text.Json.Serialization;

namespace Kong.Portal.CLI.ApiClient.Models;

internal record ApiProductDocumentBody(
    string Id,
    [property: JsonPropertyName("parent_document_id")] string ParentDocumentId,
    string Slug,
    string Status,
    string Title,
    [property: JsonPropertyName("str_md_content")] string MarkdownContent
);
