using Flurl.Http;
using Kong.Portal.CLI.ApiClient.Models;
using Kong.Portal.CLI.Config;
using Microsoft.Extensions.Options;

namespace Kong.Portal.CLI.ApiClient;

internal class KongApiClient(IOptions<KongOptions> kongOptions)
{
    private readonly FlurlClient _flurlClient = new FlurlClient(kongOptions.Value.GetKongBaseUri())
        .WithHeader("User-Agent", "Kong Portal CLI")
        .WithOAuthBearerToken(kongOptions.Value.Token);

    public async Task<IReadOnlyList<ApiProduct>> GetApiProducts()
    {
        var allProducts = new List<ApiProduct>();

        ApiProductsResponse productsResponse;
        var pageNumber = 1;

        do
        {
            var response = await _flurlClient.Request("api-products").SetQueryParam("page[number]", pageNumber++).GetAsync();

            productsResponse = await response.GetJsonAsync<ApiProductsResponse>();

            allProducts.AddRange(productsResponse.Data);
        } while (productsResponse.Meta.Page.HasMore());

        return allProducts;
    }

    public async Task<IReadOnlyList<ApiProductVersion>> GetApiProductVersions(string apiProductId)
    {
        var allVersions = new List<ApiProductVersion>();

        ApiProductVersionsResponse versionsResponse;
        var pageNumber = 1;

        do
        {
            var response = await _flurlClient
                .Request($"api-products/{apiProductId}/product-versions")
                .SetQueryParam("page[number]", pageNumber++)
                .GetAsync();

            versionsResponse = await response.GetJsonAsync<ApiProductVersionsResponse>();

            allVersions.AddRange(versionsResponse.Data);
        } while (versionsResponse.Meta.Page.HasMore());

        return allVersions;
    }

    public async Task<IReadOnlyList<ApiProductDocument>> GetApiProductDocuments(string apiProductId)
    {
        var allDocuments = new List<ApiProductDocument>();

        ApiProductDocumentsResponse documentsResponse;
        var pageNumber = 1;

        do
        {
            var response = await _flurlClient
                .Request($"api-products/{apiProductId}/documents")
                .SetQueryParam("page[number]", pageNumber++)
                .GetAsync();

            documentsResponse = await response.GetJsonAsync<ApiProductDocumentsResponse>();

            allDocuments.AddRange(documentsResponse.Data);
        } while (documentsResponse.Meta.Page.HasMore());

        return allDocuments;
    }

    public async Task<ApiProductDocumentBody> GetApiProductDocumentBody(string apiProductId, string documentId)
    {
        var response = await _flurlClient.Request($"api-products/{apiProductId}/documents/{documentId}").GetAsync();

        return await response.GetJsonAsync<ApiProductDocumentBody>();
    }

    public async Task<ApiProductSpecification?> GetApiProductSpecification(string apiProductId, string productVersionId)
    {
        var response = await _flurlClient.Request($"api-products/{apiProductId}/product-versions/{productVersionId}/specifications").GetAsync();

        var specificationsResponse = await response.GetJsonAsync<ApiProductSpecificationsResponse>();

        return specificationsResponse.Data.FirstOrDefault();
    }
}
