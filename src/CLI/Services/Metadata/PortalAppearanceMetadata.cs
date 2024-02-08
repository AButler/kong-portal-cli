namespace Kong.Portal.CLI.Services;

internal record PortalAppearanceMetadata(
    string ThemeName,
    bool UseCustomFonts,
    PortalCustomTheme? CustomTheme,
    PortalCustomFonts CustomFonts,
    PortalText Text,
    PortalImages Images
);

internal record PortalCustomTheme(PortalCustomThemeColors Colors);

internal record PortalCustomThemeColors(
    PortalCustomThemeColorsSection Section,
    PortalCustomThemeColorsText Text,
    PortalCustomThemeColorsButton Button
);

internal record PortalCustomThemeColorsSection(string Header, string Body, string Hero, string Accent, string Tertiary, string Stroke, string Footer);

internal record PortalCustomThemeColorsText(
    string Header,
    string Hero,
    string Headings,
    string Primary,
    string Secondary,
    string Accent,
    string Link,
    string Footer
);

internal record PortalCustomThemeColorsButton(string PrimaryFill, string PrimaryText);

internal record PortalCustomFonts(string? Base, string? Code, string? Headings);

internal record PortalText(string? WelcomeMessage, string? PrimaryHeader);

internal record PortalImages(string? Favicon, string? Logo, string? CatalogCover);
