using System.Text.Json.Serialization;

namespace Kong.Portal.CLI.ApiClient.Models;

internal record ApiProductDocument(
    string Id,
    [property: JsonPropertyName("parent_document_id")] string ParentDocumentId,
    string Slug,
    string Status,
    string Title
);
