namespace Kong.Portal.CLI.ApiClient.Models;

internal record DevPortalAppearance(
    string ThemeName,
    bool UseCustomFonts,
    DevPortalAppearanceCustomTheme? CustomTheme,
    DevPortalAppearanceCustomFonts? CustomFonts,
    DevPortalAppearanceText? Text,
    DevPortalAppearanceImages? Images
);

internal record DevPortalAppearanceCustomFonts(string Base, string Code, string Headings);

internal record DevPortalAppearanceText(DevPortalAppearanceTextCatalog Catalog);

internal record DevPortalAppearanceTextCatalog(string WelcomeMessage, string PrimaryHeader);

internal record DevPortalAppearanceImages(DevPortalAppearanceImage? Favicon, DevPortalAppearanceImage? Logo, DevPortalAppearanceImage? CatalogCover);

internal record DevPortalAppearanceImage(string Data, string Filename);

internal record DevPortalAppearanceCustomTheme(DevPortalAppearanceCustomThemeColors Colors);

internal record DevPortalAppearanceCustomThemeColors(
    DevPortalAppearanceCustomThemeColorsSection Section,
    DevPortalAppearanceCustomThemeColorsText Text,
    PortalAppearanceCustomThemeColorsButton Button
);

internal record DevPortalAppearanceCustomThemeColorsSection(
    PortalAppearanceColorValue Header,
    PortalAppearanceColorValue Body,
    PortalAppearanceColorValue Hero,
    PortalAppearanceColorValue Accent,
    PortalAppearanceColorValue Tertiary,
    PortalAppearanceColorValue Stroke,
    PortalAppearanceColorValue Footer
);

internal record DevPortalAppearanceCustomThemeColorsText(
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

internal record PortalAppearanceColorValue(string Value);
