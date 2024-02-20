using System.IO.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kong.Portal.CLI.Services;

internal class MetadataSerializer(IFileSystem fileSystem)
{
    internal static readonly JsonSerializerOptions SerializerOptions =
        new(JsonSerializerDefaults.Web)
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower) }
        };

    public async Task SerializeAsync<T>(string filename, T value)
    {
        await using var file = fileSystem.File.Create(filename);
        await JsonSerializer.SerializeAsync(file, value, SerializerOptions);
    }

    public async Task<T?> DeserializeAsync<T>(string filename)
    {
        return await JsonSerializer.DeserializeAsync<T>(fileSystem.File.OpenRead(filename), SerializerOptions);
    }
}
