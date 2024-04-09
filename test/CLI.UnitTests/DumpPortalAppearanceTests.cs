using CLI.UnitTests.TestHost;

namespace CLI.UnitTests;

public class DumpPortalAppearanceTests
{
    [Fact]
    public async Task PortalAppearanceIsDumped()
    {
        using var testHost = new TestHost.TestHost();

        var dumpService = testHost.GetRequiredService<DumpService>();

        var portalId = Guid.NewGuid().ToString();

        testHost.Given.Api.AnExistingDevPortal(
            portalId: portalId,
            name: "default",
            appearanceData: new AppearanceData(
                ThemeName: "custom",
                UseCustomFonts: true,
                CustomFontBase: "Inter",
                CustomFontCode: "Source Code Pro",
                CustomFontHeadings: "Open Sans",
                WelcomeMessage: "Welcome to the DevPortal",
                PrimaryHeader: "This portal contains all the information you could need",
                FaviconImage: Icons.Favicon,
                FaviconImageName: "favicon.png",
                LogoImage: Icons.Logo,
                LogoImageName: "logo.png",
                CatalogCoverImage: Icons.CatalogCover,
                CatalogCoverImageName: "catalog_cover.png"
            )
        );

        var outputDirectory = @"c:\temp\output";

        await dumpService.Dump(outputDirectory, testHost.ApiClientOptions);

        await testHost.Then.DumpedFile.ShouldHavePortalAppearance(
            outputDirectory: outputDirectory,
            portalName: "default",
            themeName: "custom",
            useCustomFonts: true,
            customFontBase: "Inter",
            customFontCode: "Source Code Pro",
            customFontHeadings: "Open Sans",
            welcomeMessage: "Welcome to the DevPortal",
            primaryHeader: "This portal contains all the information you could need",
            faviconImage: Icons.Favicon,
            faviconImageName: "favicon.png",
            logoImage: Icons.Logo,
            logoImageName: "logo.png",
            catalogCoverImage: Icons.CatalogCover,
            catalogCoverImageName: "catalog_cover.png"
        );
    }
}
