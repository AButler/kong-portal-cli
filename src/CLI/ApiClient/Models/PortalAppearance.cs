namespace Kong.Portal.CLI.ApiClient.Models;

internal record PortalAppearance(
    string ThemeName,
    bool UseCustomFonts,
    PortalAppearanceCustomFonts? CustomFonts,
    PortalAppearanceText? Text,
    PortalAppearanceImages Images
);

internal record PortalAppearanceCustomFonts(string Base, string Code, string Headings);

internal record PortalAppearanceText(PortalAppearanceTextCatalog Catalog);

internal record PortalAppearanceTextCatalog(string? WelcomeMessage, string? PrimaryHeader);

internal record PortalAppearanceImages(PortalAppearanceImage? Favicon, PortalAppearanceImage? Logo, PortalAppearanceImage? CatalogCover);

internal record PortalAppearanceImage(string Data, string Filename);
