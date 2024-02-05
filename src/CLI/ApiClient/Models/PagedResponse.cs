namespace Kong.Portal.CLI.ApiClient.Models;

internal record PagedResponse<T>(List<T> Data, ApiMetadata Meta);
