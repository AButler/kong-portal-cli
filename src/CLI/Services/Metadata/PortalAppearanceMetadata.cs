namespace Kong.Portal.CLI.Services;

internal record PortalAppearanceMetadata(
    string ThemeName,
    bool UseCustomFonts,
    PortalCustomThemeMetadata? CustomTheme,
    PortalCustomFontsMetadata CustomFonts,
    PortalTextMetadata Text,
    PortalImagesMetadata Images
);

internal record PortalCustomThemeMetadata(PortalCustomThemeColorsMetadata Colors);

internal record PortalCustomThemeColorsMetadata(
    PortalCustomThemeColorsSectionMetadata Section,
    PortalCustomThemeColorsTextMetadata Text,
    PortalCustomThemeColorsButtonMetadata Button
);

internal record PortalCustomThemeColorsSectionMetadata(
    string Header,
    string Body,
    string Hero,
    string Accent,
    string Tertiary,
    string Stroke,
    string Footer
);

internal record PortalCustomThemeColorsTextMetadata(
    string Header,
    string Hero,
    string Headings,
    string Primary,
    string Secondary,
    string Accent,
    string Link,
    string Footer
);

internal record PortalCustomThemeColorsButtonMetadata(string PrimaryFill, string PrimaryText);

internal record PortalCustomFontsMetadata(string Base, string Code, string Headings);

internal record PortalTextMetadata(string WelcomeMessage, string PrimaryHeader);

internal record PortalImagesMetadata(string? Favicon, string? Logo, string? CatalogCover)
{
    public static PortalImagesMetadata NullValue => new(null, null, null);
}
