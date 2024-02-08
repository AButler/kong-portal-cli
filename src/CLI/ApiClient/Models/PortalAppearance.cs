namespace Kong.Portal.CLI.ApiClient.Models;

internal record PortalAppearance(
    string ThemeName,
    bool UseCustomFonts,
    PortalAppearanceCustomTheme? CustomTheme,
    PortalAppearanceCustomFonts? CustomFonts,
    PortalAppearanceText? Text,
    PortalAppearanceImages Images
);

internal record PortalAppearanceCustomFonts(string Base, string Code, string Headings);

internal record PortalAppearanceText(PortalAppearanceTextCatalog Catalog);

internal record PortalAppearanceTextCatalog(string? WelcomeMessage, string? PrimaryHeader);

internal record PortalAppearanceImages(PortalAppearanceImage? Favicon, PortalAppearanceImage? Logo, PortalAppearanceImage? CatalogCover);

internal record PortalAppearanceImage(string Data, string Filename);

internal record PortalAppearanceCustomTheme(PortalAppearanceCustomThemeColors Colors);

internal record PortalAppearanceCustomThemeColors(
    PortalAppearanceCustomThemeColorsSection Section,
    PortalAppearanceCustomThemeColorsText Text,
    PortalAppearanceCustomThemeColorsButton Button
);

internal record PortalAppearanceCustomThemeColorsSection(
    PortalAppearanceColorValue Header,
    PortalAppearanceColorValue Body,
    PortalAppearanceColorValue Hero,
    PortalAppearanceColorValue Accent,
    PortalAppearanceColorValue Tertiary,
    PortalAppearanceColorValue Stroke,
    PortalAppearanceColorValue Footer
);

internal record PortalAppearanceCustomThemeColorsText(
    PortalAppearanceColorValue Header,
    PortalAppearanceColorValue Hero,
    PortalAppearanceColorValue Headings,
    PortalAppearanceColorValue Primary,
    PortalAppearanceColorValue Secondary,
    PortalAppearanceColorValue Accent,
    PortalAppearanceColorValue Link,
    PortalAppearanceColorValue Footer
);

internal record PortalAppearanceCustomThemeColorsButton(PortalAppearanceColorValue PrimaryFill, PortalAppearanceColorValue PrimaryText);

internal record PortalAppearanceColorValue(string Value, string Description);
