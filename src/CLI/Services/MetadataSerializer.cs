using System.IO.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kong.Portal.CLI.Services;

internal class MetadataSerializer(IFileSystem fileSystem, VariableHelper variableHelper)
{
    internal static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower) },
    };

    public async Task SerializeAsync<T>(string filename, T value, CancellationToken cancellationToken = default)
    {
        await using var file = fileSystem.File.Create(filename);
        await JsonSerializer.SerializeAsync(file, value, SerializerOptions, cancellationToken);
    }

    public async Task<T?> DeserializeAsync<T>(
        string filename,
        IReadOnlyDictionary<string, string> variables,
        CancellationToken cancellationToken = default
    )
    {
        var json = await fileSystem.File.ReadAllTextAsync(filename, cancellationToken);
        var replacedJson = variableHelper.Replace(json, variables);
        return JsonSerializer.Deserialize<T>(replacedJson, SerializerOptions);
    }
}
