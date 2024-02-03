namespace Kong.Portal.CLI.Services.Models;

internal record ApiProductDocumentsResponse(
    List<ApiProductDocumentResponse> Data,
    ApiMetadata Meta
);
