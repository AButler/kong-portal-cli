﻿using Kong.Portal.CLI.ApiClient.Models;
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
            new DevPortalAppearanceCustomFonts(metadata.CustomFonts.Base, metadata.CustomFonts.Code, metadata.CustomFonts.Headings),
            new DevPortalAppearanceText(new DevPortalAppearanceTextCatalog(metadata.Text.PrimaryHeader, metadata.Text.WelcomeMessage)),
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

    private static DevPortalAppearanceImages ToApiModel(this PortalImagesMetadata metadata, ImageData imageData)
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

    public static ApiProductDocumentBody ToApiModel(this ApiProductDocumentMetadata metadata, string contents, string? id = null)
    {
        var parentSlug = GetParentSlug(metadata.FullSlug);

        return new ApiProductDocumentBody(
            id ?? $"resolve://api-product-document/{metadata.FullSlug}",
            parentSlug == null ? null : $"resolve://api-product-document/{parentSlug}",
            metadata.Slug,
            metadata.FullSlug,
            metadata.Status.ToApiModel(),
            metadata.Title,
            contents
        );
    }

    private static ApiPublishStatus ToApiModel(this MetadataPublishStatus publishStatus) =>
        publishStatus switch
        {
            MetadataPublishStatus.Published => ApiPublishStatus.Published,
            MetadataPublishStatus.Unpublished => ApiPublishStatus.Unpublished,
            _ => throw new ArgumentOutOfRangeException(nameof(publishStatus))
        };

    private static LabelDictionary WithSyncId(this LabelDictionary labels, string syncId)
    {
        var newLabels = labels.Clone();
        newLabels[Constants.SyncIdLabel] = syncId;
        return newLabels;
    }

    private static string? GetParentSlug(string slug)
    {
        var parentPath = Path.GetDirectoryName(slug)!.Replace(@"\", "/");

        return string.IsNullOrEmpty(parentPath) ? null : parentPath;
    }
}
