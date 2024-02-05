using Kong.Portal.CLI.Services.Metadata;

namespace Kong.Portal.CLI.Services;

internal class SourceData(string inputDirectory)
{
    public string InputDirectory { get; } = inputDirectory;

    public List<ApiProductMetadata> ApiProducts { get; } = new();
}
