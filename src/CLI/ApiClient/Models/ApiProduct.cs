using System.Text.Json.Serialization;

namespace Kong.Portal.CLI.ApiClient.Models;

internal record ApiProduct(
    Dictionary<string, string> Labels,
    string Id,
    string Name,
    string Description,
    [property: JsonPropertyName("portal_ids")] List<string> PortalIds,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("updated_at")] DateTimeOffset UpdatedAt,
    [property: JsonPropertyName("version_count")] int VersionCount
);
