using System.Text.Json.Serialization;

namespace Kong.Portal.CLI.ApiClient.Models;

internal record ApiProductDocumentBody(
    string Id,
    string Slug,
    string Status,
    string Title,
    [property: JsonPropertyName("str_md_content")] string MarkdownContent
);
