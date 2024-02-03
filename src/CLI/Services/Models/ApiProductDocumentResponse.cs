using System.Text.Json.Serialization;

namespace Kong.Portal.CLI.Services.Models;

internal record ApiProductDocumentResponse(
    string Id,
    [property: JsonPropertyName("parent_document_id")] string ParentDocumentId,
    string Slug,
    string Status,
    string Title
);
