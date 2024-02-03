using System.Text.Json.Serialization;

namespace Kong.Portal.CLI.Services.Models;

internal record ApiProductVersion(string Id, string Name, [property: JsonPropertyName("publish_status")] string PublishStatus, bool Deprecated);
