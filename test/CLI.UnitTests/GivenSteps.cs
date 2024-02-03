using Flurl.Http.Testing;

namespace CLI.UnitTests;

public class GivenSteps
{
    private const int MaximumPageSize = 10;

    private readonly List<object> _apiProducts = [];
    private readonly Dictionary<string, List<object>> _apiProductDocuments = new();

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
        string name,
        string description,
        IDictionary<string, string>? labels = null
    )
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
        SetupApiProductDocumentsApis(id);
    }

    private void SetupApiProductsApis()
    {
        HttpTest
            .Current.ForCallsTo("https://eu.api.konghq.com/v2/api-products")
            .WithVerb("GET")
            .WithoutQueryParam("page[number]")
            .RespondWithDynamicJson(
                () =>
                    new
                    {
                        data = _apiProducts.Take(_pageSize),
                        meta = new
                        {
                            page = new
                            {
                                total = _apiProducts.Count,
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
                .Current.ForCallsTo("https://eu.api.konghq.com/v2/api-products")
                .WithVerb("GET")
                .WithQueryParam("page[number]", page)
                .RespondWithDynamicJson(
                    () =>
                        new
                        {
                            data = _apiProducts.Skip((pageNumber - 1) * _pageSize).Take(_pageSize),
                            meta = new
                            {
                                page = new
                                {
                                    total = _apiProducts.Count,
                                    size = _pageSize,
                                    number = pageNumber
                                }
                            }
                        }
                );
        }
    }

    private void SetupApiProductDocumentsApis(string apiProductId)
    {
        HttpTest
            .Current.ForCallsTo(
                $"https://eu.api.konghq.com/v2/api-products/{apiProductId}/documents"
            )
            .WithVerb("GET")
            .WithoutQueryParam("page[number]")
            .RespondWithDynamicJson(
                () =>
                    new
                    {
                        data = _apiProductDocuments[apiProductId].Take(_pageSize),
                        meta = new
                        {
                            page = new
                            {
                                total = _apiProductDocuments[apiProductId].Count,
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
                .Current.ForCallsTo(
                    $"https://eu.api.konghq.com/v2/api-products/{apiProductId}/documents"
                )
                .WithVerb("GET")
                .WithQueryParam("page[number]", page)
                .RespondWithDynamicJson(
                    () =>
                        new
                        {
                            data = _apiProductDocuments[apiProductId]
                                .Skip((pageNumber - 1) * _pageSize)
                                .Take(_pageSize),
                            meta = new
                            {
                                page = new
                                {
                                    total = _apiProductDocuments[apiProductId].Count,
                                    size = _pageSize,
                                    number = pageNumber
                                }
                            }
                        }
                );
        }
    }
}
