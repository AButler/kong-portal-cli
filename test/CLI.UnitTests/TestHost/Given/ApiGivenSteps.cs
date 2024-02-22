using Flurl.Http.Testing;
using Kong.Portal.CLI.ApiClient;

namespace CLI.UnitTests.TestHost;

internal class ApiGivenSteps
{
    private const int MaximumPageSize = 10;

    private readonly string _kongBaseUrl;
    private readonly List<dynamic> _apiProducts = [];
    private readonly List<dynamic> _devPortals = [];
    private readonly Dictionary<string, List<dynamic>> _apiProductDocuments = new();
    private readonly Dictionary<string, List<dynamic>> _apiProductVersions = new();
    private readonly Dictionary<string, dynamic> _devPortalAppearances = new();

    private int _pageSize = 100;

    public ApiGivenSteps(KongApiClientOptions apiClientOptions)
    {
        _kongBaseUrl = $"{apiClientOptions.BaseUrl}/v2/";

        SetupApiProductsApis();
        SetupDevPortalApis();

        SetupIncomingApis();
    }

    public void TheKongApiPageSizeIs(int pageSize)
    {
        _pageSize = pageSize;
    }

    public void AnExistingApiProduct(
        Discretionary<string> productId = default,
        Discretionary<string> name = default,
        Discretionary<string> description = default,
        Discretionary<string[]> portalIds = default,
        IDictionary<string, string>? labels = null
    )
    {
        var id = productId.GetValueOrDefault(Guid.NewGuid().ToString());

        _apiProducts.Add(
            new
            {
                id,
                labels = labels ?? new Dictionary<string, string>(),
                name = name.GetValueOrDefault($"API Product {id}"),
                portal_ids = portalIds.GetValueOrDefault([]),
                description = description.GetValueOrDefault($"Description for API Product {id}")
            }
        );

        _apiProductDocuments[id] = [];
        _apiProductVersions[id] = [];

        HttpTest.Current.ForCallsTo($"{_kongBaseUrl}api-products/{id}").WithVerb("PATCH").RespondWithJson(_apiProducts.Single(p => p.id == id));
        SetupApiProductDocumentsApis(id);
        SetupApiProductVersionApis(id);
    }

    public void AnExistingApiProductDocument(string apiProductId, string slug, string title, string content)
    {
        var id = Guid.NewGuid().ToString();

        _apiProductDocuments[apiProductId]
            .Add(
                new
                {
                    id,
                    title,
                    slug,
                    status = "published"
                }
            );

        HttpTest
            .Current.ForCallsTo($"{_kongBaseUrl}api-products/{apiProductId}/documents/{id}")
            .WithVerb("GET")
            .RespondWithJson(
                new
                {
                    id,
                    title,
                    slug = Path.GetFileName(slug),
                    status = "published",
                    str_md_content = content
                }
            );
    }

    public void AnExistingApiProductVersion(
        string apiProductId,
        string name,
        Discretionary<string> versionId = default,
        Discretionary<string> publishStatus = default,
        Discretionary<bool> deprecated = default,
        Discretionary<string> specificationFilename = default,
        Discretionary<string> specificationContents = default
    )
    {
        var id = versionId.GetValueOrDefault(Guid.NewGuid().ToString());
        var specificationId = Guid.NewGuid().ToString();

        _apiProductVersions[apiProductId]
            .Add(
                new
                {
                    id,
                    name,
                    publish_status = publishStatus.GetValueOrDefault("published"),
                    deprecated = deprecated.GetValueOrDefault(false)
                }
            );

        HttpTest
            .Current.ForCallsTo($"{_kongBaseUrl}api-products/{apiProductId}/product-versions/{id}")
            .WithVerb("GET")
            .RespondWithJson(
                new
                {
                    id,
                    name,
                    publish_status = publishStatus,
                    deprecated
                }
            );

        var specificationResponse =
            specificationFilename.IsSpecified && specificationContents.IsSpecified
                ?
                [
                    new
                    {
                        id = specificationId,
                        name = $"/{specificationFilename.Value}",
                        content = specificationContents.Value
                    }
                ]
                : Array.Empty<object>();

        HttpTest
            .Current.ForCallsTo($"{_kongBaseUrl}api-products/{apiProductId}/product-versions/{id}/specifications")
            .WithVerb("GET")
            .RespondWithJson(new { data = specificationResponse });
    }

    public void AnExistingDevPortal(
        Discretionary<string> portalId = default,
        Discretionary<string> name = default,
        Discretionary<bool> isPublic = default,
        Discretionary<bool> rbacEnabled = default,
        Discretionary<bool> autoApproveApplications = default,
        Discretionary<bool> autoApproveDevelopers = default,
        Discretionary<string?> customDomain = default,
        Discretionary<string?> customClientDomain = default,
        Discretionary<AppearanceData> appearanceData = default
    )
    {
        var id = portalId.GetValueOrDefault(Guid.NewGuid().ToString());

        _devPortals.Add(
            new
            {
                id,
                name = name.GetValueOrDefault("default"),
                custom_domain = customDomain.GetValueOrDefault(null),
                custom_client_domain = customClientDomain.GetValueOrDefault(null),
                is_public = isPublic.GetValueOrDefault(false),
                auto_approve_developers = autoApproveDevelopers.GetValueOrDefault(false),
                auto_approve_applications = autoApproveApplications.GetValueOrDefault(false),
                rbac_enabled = rbacEnabled.GetValueOrDefault(false)
            }
        );

        AnExistingDevPortalAppearance(id, appearanceData.GetValueOrDefault(new AppearanceData()));

        HttpTest.Current.ForCallsTo($"{_kongBaseUrl}portals/{id}/appearance").WithVerb("GET").RespondWithDynamicJson(() => _devPortalAppearances[id]);
    }

    private void AnExistingDevPortalAppearance(string portalId, AppearanceData appearance)
    {
        var customFontObject =
            !appearance.CustomFontBase.IsSpecified && !appearance.CustomFontCode.IsSpecified && !appearance.CustomFontHeadings.IsSpecified
                ? (object?)null
                : new
                {
                    @base = appearance.CustomFontBase.GetValueOrDefault(null),
                    code = appearance.CustomFontCode.GetValueOrDefault(null),
                    headings = appearance.CustomFontHeadings.GetValueOrDefault(null)
                };

        var textObject =
            !appearance.WelcomeMessage.IsSpecified && !appearance.PrimaryHeader.IsSpecified
                ? (object?)null
                : new
                {
                    catalog = new
                    {
                        welcome_message = appearance.WelcomeMessage.GetValueOrDefault(null),
                        primary_header = appearance.PrimaryHeader.GetValueOrDefault(null)
                    }
                };

        var faviconObject =
            !appearance.FaviconImage.IsSpecified && !appearance.FaviconImageName.IsSpecified
                ? (object?)null
                : new { data = appearance.FaviconImage.GetValueOrDefault(null), filename = appearance.FaviconImageName.GetValueOrDefault(null) };

        var logoObject =
            !appearance.LogoImage.IsSpecified && !appearance.LogoImageName.IsSpecified
                ? (object?)null
                : new { data = appearance.LogoImage.GetValueOrDefault(null), filename = appearance.LogoImageName.GetValueOrDefault(null) };

        var catalogCoverObject =
            !appearance.CatalogCoverImage.IsSpecified && !appearance.CatalogCoverImageName.IsSpecified
                ? (object?)null
                : new
                {
                    data = appearance.CatalogCoverImage.GetValueOrDefault(null),
                    filename = appearance.CatalogCoverImageName.GetValueOrDefault(null)
                };

        _devPortalAppearances.Add(
            portalId,
            new
            {
                theme_name = appearance.ThemeName.GetValueOrDefault("mint_rocket"),
                use_custom_fonts = appearance.UseCustomFonts.GetValueOrDefault(false),
                custom_theme = GetDefaultCustomThemeObject(),
                custom_fonts = customFontObject,
                text = textObject,
                images = new
                {
                    favicon = faviconObject,
                    logo = logoObject,
                    catalog_cover = catalogCoverObject
                }
            }
        );
    }

    private void SetupIncomingApis()
    {
        HttpTest
            .Current.ForCallsTo($"{_kongBaseUrl}api-products")
            .WithVerb("POST")
            .RespondWithDynamicJson(
                () =>
                {
                    var productId = Guid.NewGuid().ToString();

                    AnExistingApiProduct(productId: productId);

                    return _apiProducts.Single(p => p.id == productId);
                },
                201
            );
    }

    private void SetupApiProductsApis()
    {
        SetupPagedApi($"{_kongBaseUrl}api-products", () => _apiProducts);
    }

    private void SetupDevPortalApis()
    {
        SetupPagedApi($"{_kongBaseUrl}portals", () => _devPortals);
    }

    private void SetupApiProductDocumentsApis(string apiProductId)
    {
        SetupPagedApi($"{_kongBaseUrl}api-products/{apiProductId}/documents", () => _apiProductDocuments[apiProductId]);
    }

    private void SetupApiProductVersionApis(string apiProductId)
    {
        SetupPagedApi($"{_kongBaseUrl}api-products/{apiProductId}/product-versions", () => _apiProductVersions[apiProductId]);
        HttpTest
            .Current.ForCallsTo($"{_kongBaseUrl}api-products/{apiProductId}/product-versions")
            .WithVerb("POST")
            .RespondWithDynamicJson(
                () =>
                    new
                    {
                        id = Guid.NewGuid().ToString(),
                        name = "DUMMY NAME",
                        publish_status = "unpublished",
                        deprecated = false
                    },
                201
            );
    }

    private void SetupPagedApi(string url, Func<IReadOnlyCollection<dynamic>> results)
    {
        HttpTest
            .Current.ForCallsTo(url)
            .WithVerb("GET")
            .WithoutQueryParam("page[number]")
            .RespondWithDynamicJson(
                () =>
                    new
                    {
                        data = results().Take(_pageSize),
                        meta = new
                        {
                            page = new
                            {
                                total = results().Count,
                                size = _pageSize,
                                number = 1
                            }
                        }
                    }
            );

        for (var page = 1; page < MaximumPageSize; page++)
        {
            var pageNumber = page;
            HttpTest
                .Current.ForCallsTo(url)
                .WithVerb("GET")
                .WithQueryParam("page[number]", page)
                .RespondWithDynamicJson(
                    () =>
                        new
                        {
                            data = results().Skip((pageNumber - 1) * _pageSize).Take(_pageSize),
                            meta = new
                            {
                                page = new
                                {
                                    total = results().Count,
                                    size = _pageSize,
                                    number = pageNumber
                                }
                            }
                        }
                );
        }
    }

    private static dynamic GetDefaultCustomThemeObject()
    {
        return new
        {
            colors = new
            {
                section = new
                {
                    header = new { value = "#F8F8F8" },
                    body = new { value = "#FFFFFF" },
                    hero = new { value = "#F8F8F8" },
                    accent = new { value = "#F8F8F8" },
                    tertiary = new { value = "#FFFFFF" },
                    stroke = new { value = "rgba(0,0,0,0.1)" },
                    footer = new { value = "#07A88D" }
                },
                text = new
                {
                    header = new { value = "rgba(0,0,0,0.8)" },
                    hero = new { value = "#FFFFFF" },
                    headings = new { value = "rgba(0,0,0,0.8)" },
                    primary = new { value = "rgba(0,0,0,0.8)" },
                    secondary = new { value = "rgba(0,0,0,0.8)" },
                    accent = new { value = "#07A88D" },
                    link = new { value = "#07A88D" },
                    footer = new { value = "#FFFFFF" }
                },
                button = new { primary_fill = new { value = "#1155CB" }, primary_text = new { value = "#FFFFFF" } }
            }
        };
    }
}
