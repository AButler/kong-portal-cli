using Kong.Portal.CLI.ApiClient.Models;
using Kong.Portal.CLI.Services;

namespace Kong.Portal.CLI;

internal static class MetadataMappingExtensions
{
    public static DevPortal ToApiModel(this PortalMetadata metadata, string? id = null)
    {
        return new DevPortal(
            id ?? $"resolve://portal/{metadata.Name}",
            metadata.Name,
            metadata.CustomDomain,
            metadata.CustomClientDomain,
            metadata.IsPublic,
            metadata.AutoApproveDevelopers,
            metadata.AutoApproveApplications,
            metadata.RbacEnabled
        );
    }

    public static DevPortalAppearance ToApiModel(this PortalAppearanceMetadata metadata, ImageData imageData)
    {
        return new DevPortalAppearance(
            metadata.ThemeName,
            metadata.UseCustomFonts,
            metadata.CustomTheme.ToApiModel(),
            metadata.CustomFonts.ToApiModel(),
            metadata.Text.ToApiModel(),
            metadata.Images.ToApiModel(imageData)
        );
    }

    public static DevPortalAppearanceCustomTheme? ToApiModel(this PortalCustomThemeMetadata? metadata)
    {
        if (metadata == null)
        {
            return null;
        }

        var section = new DevPortalAppearanceCustomThemeColorsSection(
            new PortalAppearanceColorValue(metadata.Colors.Section.Header),
            new PortalAppearanceColorValue(metadata.Colors.Section.Body),
            new PortalAppearanceColorValue(metadata.Colors.Section.Hero),
            new PortalAppearanceColorValue(metadata.Colors.Section.Accent),
            new PortalAppearanceColorValue(metadata.Colors.Section.Tertiary),
            new PortalAppearanceColorValue(metadata.Colors.Section.Stroke),
            new PortalAppearanceColorValue(metadata.Colors.Section.Footer)
        );

        var text = new DevPortalAppearanceCustomThemeColorsText(
            new PortalAppearanceColorValue(metadata.Colors.Text.Header),
            new PortalAppearanceColorValue(metadata.Colors.Text.Hero),
            new PortalAppearanceColorValue(metadata.Colors.Text.Headings),
            new PortalAppearanceColorValue(metadata.Colors.Text.Primary),
            new PortalAppearanceColorValue(metadata.Colors.Text.Secondary),
            new PortalAppearanceColorValue(metadata.Colors.Text.Accent),
            new PortalAppearanceColorValue(metadata.Colors.Text.Link),
            new PortalAppearanceColorValue(metadata.Colors.Text.Footer)
        );

        var button = new PortalAppearanceCustomThemeColorsButton(
            new PortalAppearanceColorValue(metadata.Colors.Button.PrimaryFill),
            new PortalAppearanceColorValue(metadata.Colors.Button.PrimaryText)
        );

        var colors = new DevPortalAppearanceCustomThemeColors(section, text, button);

        return new DevPortalAppearanceCustomTheme(colors);
    }

    public static DevPortalAppearanceCustomFonts? ToApiModel(this PortalCustomFontsMetadata metadata)
    {
        if (metadata.Base == null && metadata.Code == null && metadata.Headings == null)
        {
            return null;
        }

        return new DevPortalAppearanceCustomFonts(metadata.Base ?? "", metadata.Code ?? "", metadata.Headings ?? "");
    }

    public static DevPortalAppearanceText? ToApiModel(this PortalTextMetadata metadata)
    {
        if (metadata.PrimaryHeader == null && metadata.WelcomeMessage == null)
        {
            return null;
        }

        return new DevPortalAppearanceText(new DevPortalAppearanceTextCatalog(metadata.PrimaryHeader, metadata.WelcomeMessage));
    }

    public static DevPortalAppearanceImages ToApiModel(this PortalImagesMetadata metadata, ImageData imageData)
    {
        var favicon = imageData.Favicon == null ? null : new DevPortalAppearanceImage(imageData.Favicon, metadata.Favicon!);
        var logo = imageData.Logo == null ? null : new DevPortalAppearanceImage(imageData.Logo, metadata.Logo!);
        var catalogCover = imageData.CatalogCover == null ? null : new DevPortalAppearanceImage(imageData.CatalogCover, metadata.CatalogCover!);

        return new DevPortalAppearanceImages(favicon, logo, catalogCover);
    }

    public static ApiProduct ToApiModel(this ApiProductMetadata metadata, IReadOnlyDictionary<string, string> portalNameMap, string? id = null)
    {
        var portalIds = new List<string>();

        foreach (var portalName in metadata.Portals)
        {
            portalIds.Add(portalNameMap[portalName]);
        }

        return new ApiProduct(
            id ?? $"resolve://api-product/{metadata.SyncId}",
            metadata.Name,
            metadata.Description,
            portalIds,
            metadata.Labels.WithSyncId(metadata.SyncId)
        );
    }

    public static ApiProductVersion ToApiModel(this ApiProductVersionMetadata metadata, string? id = null)
    {
        return new ApiProductVersion(
            id ?? $"resolve://api-product-version/{metadata.SyncId}",
            metadata.Name,
            metadata.PublishStatus.ToApiModel(),
            metadata.Deprecated
        );
    }

    public static ApiProductSpecification ToApiProductVersionSpecification(
        this ApiProductVersionMetadata metadata,
        string specificationContent,
        string? id = null
    )
    {
        return new ApiProductSpecification(
            id ?? $"resolve://api-product-specification/{metadata.SyncId}",
            metadata.SpecificationFilename!,
            specificationContent
        );
    }

    private static ApiVersionPublishStatus ToApiModel(this ApiProductVersionMetadataPublishStatus publishStatus) =>
        publishStatus switch
        {
            ApiProductVersionMetadataPublishStatus.Published => ApiVersionPublishStatus.Published,
            ApiProductVersionMetadataPublishStatus.Unpublished => ApiVersionPublishStatus.Unpublished,
            _ => throw new ArgumentOutOfRangeException(nameof(publishStatus))
        };

    private static LabelDictionary WithSyncId(this LabelDictionary labels, string syncId)
    {
        var newLabels = labels.Clone();
        newLabels[Constants.SyncIdLabel] = syncId;
        return newLabels;
    }
}
