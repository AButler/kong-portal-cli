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
    private readonly Dictionary<string, dynamic> _devPortalAuthSettings = new();
    private readonly Dictionary<string, List<dynamic>> _devPortalTeams = new();
    private readonly Dictionary<string, List<dynamic>> _devPortalProducts = new();
    private readonly Dictionary<string, Dictionary<string, List<dynamic>>> _devPortalTeamRoles = new();

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

        var apiProduct = new
        {
            id,
            labels = labels ?? new Dictionary<string, string>(),
            name = name.GetValueOrDefault($"API Product {id}"),
            portal_ids = portalIds.GetValueOrDefault([]),
            description = description.GetValueOrDefault($"Description for API Product {id}")
        };

        _apiProducts.Add(apiProduct);
        _apiProductDocuments[id] = [];
        _apiProductVersions[id] = [];

        foreach (var portalId in apiProduct.portal_ids)
        {
            if (!_devPortalProducts.ContainsKey(portalId))
            {
                _devPortalProducts.Add(portalId, new List<dynamic>());
            }
            _devPortalProducts[portalId].Add(apiProduct);
        }

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
        Discretionary<AppearanceData> appearanceData = default,
        Discretionary<AuthSettings> authSettings = default
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

        _devPortalTeams.Add(id, []);
        _devPortalTeamRoles.Add(id, new Dictionary<string, List<dynamic>>());
        if (!_devPortalProducts.ContainsKey(id))
        {
            _devPortalProducts.Add(id, new List<dynamic>());
        }

        SetupDevPortalAppearance(id, appearanceData.GetValueOrDefault(new AppearanceData()));

        HttpTest.Current.ForCallsTo($"{_kongBaseUrl}portals/{id}/appearance").WithVerb("GET").RespondWithDynamicJson(() => _devPortalAppearances[id]);

        SetupDevPortalAuthSettings(id, authSettings.GetValueOrDefault(new AuthSettings()));

        HttpTest
            .Current.ForCallsTo($"{_kongBaseUrl}portals/{id}/authentication-settings")
            .WithVerb("GET")
            .RespondWithDynamicJson(() => _devPortalAuthSettings[id]);

        SetupPagedApi($"{_kongBaseUrl}portals/{id}/products", () => _devPortalProducts[id]);

        HttpTest
            .Current.ForCallsTo($"{_kongBaseUrl}portals/{id}/teams")
            .WithVerb("POST")
            .RespondWithDynamicJson(() => new { id = Guid.NewGuid().ToString() }, 201);

        SetupPagedApi($"{_kongBaseUrl}portals/{id}/teams", () => _devPortalTeams[id]);
    }

    public void AnExistingDevPortalTeam(string portalId, string name, string description, string? teamId = null)
    {
        var id = teamId ?? Guid.NewGuid().ToString();

        var team = new
        {
            id = id,
            name = name,
            description = description
        };

        _devPortalTeams[portalId].Add(team);
        _devPortalTeamRoles[portalId][id] = [];

        SetupPagedApi($"{_kongBaseUrl}portals/{portalId}/teams/{id}/assigned-roles", () => _devPortalTeamRoles[portalId][id]);
    }

    public void AnExistingDevPortalTeamRole(string portalId, string teamId, string roleName, string entityId, string? roleId = null)
    {
        var id = roleId ?? Guid.NewGuid().ToString();

        var teamRole = new
        {
            id = id,
            role_name = roleName,
            entity_id = entityId,
            entity_type_name = "Services",
            entity_region = "eu"
        };

        _devPortalTeamRoles[portalId][teamId].Add(teamRole);
    }

    private void SetupDevPortalAuthSettings(string id, AuthSettings authSettings)
    {
        var oidcConfig = authSettings.OidcConfig.GetValueOrDefault(null);

        var oidcConfigObject =
            oidcConfig == null
                ? null
                : new
                {
                    issuer = oidcConfig.Issuer,
                    client_id = oidcConfig.ClientId,
                    scopes = oidcConfig.Scopes,
                    claim_mappings = new
                    {
                        name = oidcConfig.ClaimMappings.Name,
                        email = oidcConfig.ClaimMappings.Email,
                        groups = oidcConfig.ClaimMappings.Groups
                    }
                };

        _devPortalAuthSettings.Add(
            id,
            new
            {
                basic_auth_enabled = authSettings.BasicAuthEnabled.GetValueOrDefault(false),
                oidc_auth_enabled = authSettings.OidcAuthEnabled.GetValueOrDefault(false),
                oidc_team_mapping_enabled = authSettings.OidcTeamMappingEnabled.GetValueOrDefault(false),
                konnect_mapping_enabled = authSettings.KonnectMappingEnabled.GetValueOrDefault(false),
                oidc_config = oidcConfigObject
            }
        );
    }

    private void SetupDevPortalAppearance(string portalId, AppearanceData appearance)
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
                custom_theme = (object?)null,
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
        HttpTest
            .Current.ForCallsTo($"{_kongBaseUrl}api-products/{apiProductId}/documents")
            .WithVerb("POST")
            .RespondWithDynamicJson(() => new { id = Guid.NewGuid().ToString() }, 201);
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
}
