using Flurl.Http.Testing;

namespace CLI.UnitTests.TestHost;

public class GivenSteps
{
    private const int MaximumPageSize = 10;

    private readonly List<dynamic> _apiProducts = [];
    private readonly List<dynamic> _devPortals = [];
    private readonly Dictionary<string, List<dynamic>> _apiProductDocuments = new();
    private readonly Dictionary<string, List<dynamic>> _apiProductVersions = new();

    private int _pageSize = 100;

    public GivenSteps()
    {
        SetupApiProductsApis();
        SetupDevPortalApis();
    }

    public void TheKongApiPageSizeIs(int pageSize)
    {
        _pageSize = pageSize;
    }

    public void AnExistingApiProduct(
        Discretionary<string> productId = default,
        Discretionary<string> name = default,
        Discretionary<string> description = default,
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
                description = description.GetValueOrDefault($"Description for API Product {id}")
            }
        );

        _apiProductDocuments[id] = [];
        _apiProductVersions[id] = [];
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
            .Current.ForCallsTo($"https://eu.api.konghq.com/v2/api-products/{apiProductId}/documents/{id}")
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
            .Current.ForCallsTo($"https://eu.api.konghq.com/v2/api-products/{apiProductId}/product-versions/{id}")
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
            .Current.ForCallsTo($"https://eu.api.konghq.com/v2/api-products/{apiProductId}/product-versions/{id}/specifications")
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
        IReadOnlyCollection<string>? apiProducts = default
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

        var apiProductIds = apiProducts ?? [];

        SetupPagedApi(
            $"https://eu.api.konghq.com/v2/portals/{id}/products",
            () => _apiProducts.Where(v => apiProductIds.Contains((string)v.id)).ToList()
        );
    }

    private void SetupApiProductsApis()
    {
        SetupPagedApi("https://eu.api.konghq.com/v2/api-products", () => _apiProducts);
    }

    private void SetupDevPortalApis()
    {
        SetupPagedApi("https://eu.api.konghq.com/v2/portals", () => _devPortals);
    }

    private void SetupApiProductDocumentsApis(string apiProductId)
    {
        SetupPagedApi($"https://eu.api.konghq.com/v2/api-products/{apiProductId}/documents", () => _apiProductDocuments[apiProductId]);
    }

    private void SetupApiProductVersionApis(string apiProductId)
    {
        SetupPagedApi($"https://eu.api.konghq.com/v2/api-products/{apiProductId}/product-versions", () => _apiProductVersions[apiProductId]);
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
