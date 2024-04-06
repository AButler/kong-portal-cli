namespace Kong.Portal.CLI.Services;

internal class SourceData(string inputDirectory, IReadOnlyDictionary<string, string> variables)
{
    public string InputDirectory { get; } = inputDirectory;
    public IReadOnlyDictionary<string, string> Variables { get; } = variables;

    public List<ApiProductMetadata> ApiProducts { get; } = new();
    public Dictionary<string, List<ApiProductVersionMetadata>> ApiProductVersions { get; } = new();
    public Dictionary<string, Dictionary<string, string?>> ApiProductVersionSpecifications { get; } = new();
    public Dictionary<string, List<ApiProductDocumentMetadata>> ApiProductDocuments { get; } = new();
    public Dictionary<string, Dictionary<string, string>> ApiProductDocumentContents { get; } = new();
    public List<PortalMetadata> Portals { get; } = new();
    public Dictionary<string, PortalAppearanceMetadata> PortalAppearances { get; } = new();
    public Dictionary<string, PortalAuthSettingsMetadata> PortalAuthSettings { get; } = new();
    public Dictionary<string, ImageData> PortalAppearanceImageData { get; } = new();
}
