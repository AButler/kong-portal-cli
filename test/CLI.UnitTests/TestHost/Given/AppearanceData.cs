namespace CLI.UnitTests.TestHost;

public record AppearanceData(
    Discretionary<string> ThemeName = default,
    Discretionary<bool> UseCustomFonts = default,
    Discretionary<string?> CustomFontBase = default,
    Discretionary<string?> CustomFontCode = default,
    Discretionary<string?> CustomFontHeadings = default,
    Discretionary<string?> WelcomeMessage = default,
    Discretionary<string?> PrimaryHeader = default,
    Discretionary<string?> FaviconImage = default,
    Discretionary<string?> FaviconImageName = default,
    Discretionary<string?> LogoImage = default,
    Discretionary<string?> LogoImageName = default,
    Discretionary<string?> CatalogCoverImage = default,
    Discretionary<string?> CatalogCoverImageName = default
);
