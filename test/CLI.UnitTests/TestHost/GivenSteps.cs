using Flurl.Http.Testing;

namespace CLI.UnitTests.TestHost;

public class GivenSteps
{
    private const int MaximumPageSize = 10;

    private readonly List<object> _apiProducts = [];
    private readonly Dictionary<string, List<object>> _apiProductDocuments = new();
    private readonly Dictionary<string, List<object>> _apiProductVersions = new();

    private int _pageSize = 100;

    public GivenSteps()
    {
        SetupApiProductsApis();
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
                    slug,
                    status = "published",
                    str_md_content = content
                }
            );
    }

    private void SetupApiProductsApis()
    {
        SetupPagedApi("https://eu.api.konghq.com/v2/api-products", _apiProducts);
    }

    private void SetupApiProductDocumentsApis(string apiProductId)
    {
        SetupPagedApi($"https://eu.api.konghq.com/v2/api-products/{apiProductId}/documents", _apiProductDocuments[apiProductId]);
    }

    private void SetupApiProductVersionApis(string apiProductId)
    {
        SetupPagedApi($"https://eu.api.konghq.com/v2/api-products/{apiProductId}/product-versions", _apiProductVersions[apiProductId]);
    }

    private void SetupPagedApi(string url, List<object> results)
    {
        HttpTest
            .Current.ForCallsTo(url)
            .WithVerb("GET")
            .WithoutQueryParam("page[number]")
            .RespondWithDynamicJson(
                () =>
                    new
                    {
                        data = results.Take(_pageSize),
                        meta = new
                        {
                            page = new
                            {
                                total = results.Count,
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
                            data = results.Skip((pageNumber - 1) * _pageSize).Take(_pageSize),
                            meta = new
                            {
                                page = new
                                {
                                    total = results.Count,
                                    size = _pageSize,
                                    number = pageNumber
                                }
                            }
                        }
                );
        }
    }
}
