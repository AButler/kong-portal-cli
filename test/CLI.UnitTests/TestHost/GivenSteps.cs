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

    public void AnExistingApiProduct(string name, string description, IDictionary<string, string>? labels = null)
    {
        var id = Guid.NewGuid().ToString();

        _apiProducts.Add(
            new
            {
                id,
                labels = labels ?? new Dictionary<string, string>(),
                name,
                description
            }
        );

        _apiProductDocuments[id] = [];
        _apiProductVersions[id] = [];
        SetupApiProductDocumentsApis(id);
        SetupApiProductVersionApis(id);
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
