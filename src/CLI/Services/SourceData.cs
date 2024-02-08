using Kong.Portal.CLI.Services.Metadata;

namespace Kong.Portal.CLI.Services;

internal class SourceData(string inputDirectory)
{
    public string InputDirectory { get; } = inputDirectory;

    public List<ApiProductMetadata> ApiProducts { get; } = new();
    public Dictionary<string, ApiProductVersionMetadata> ApiProductVersions { get; } = new();
    public Dictionary<string, Dictionary<string, string>> ApiProductVersionSpecifications { get; } = new();
    public Dictionary<string, List<ApiProductDocumentMetadata>> ApiProductDocuments { get; } = new();
    public Dictionary<string, Dictionary<string, string>> ApiProductDocumentContents { get; } = new();
    public List<PortalMetadata> Portals { get; } = new();
    public Dictionary<string, PortalAppearanceMetadata> PortalAppearances { get; } = new();
}
