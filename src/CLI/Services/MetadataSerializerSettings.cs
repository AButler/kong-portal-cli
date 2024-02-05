using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kong.Portal.CLI.Services;

internal static class MetadataSerializerSettings
{
    public static JsonSerializerOptions SerializerOptions { get; } =
        new(JsonSerializerDefaults.Web)
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never
        };
}
