namespace Kong.Portal.CLI.Services;

internal record PortalAppearanceMetadata(
    string ThemeName,
    bool UseCustomFonts,
    PortalCustomTheme? CustomTheme,
    PortalCustomFonts CustomFonts,
    PortalText Text,
    PortalImages Images
);

internal record PortalCustomTheme();

internal record PortalCustomFonts(string? Base, string? Code, string? Headings);

internal record PortalText(string? WelcomeMessage, string? PrimaryHeader);

internal record PortalImages(string? Favicon, string? Logo, string? CatalogCover);
