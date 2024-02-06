using System.Text.Json;
using Flurl.Http;
using Flurl.Http.Configuration;
using Kong.Portal.CLI.ApiClient.Models;
using Kong.Portal.CLI.Config;
using Microsoft.Extensions.Options;

namespace Kong.Portal.CLI.ApiClient;

internal class KongApiClient(IOptions<KongOptions> kongOptions)
{
    private readonly FlurlClient _flurlClient = new FlurlClient(kongOptions.Value.GetKongBaseUri())
        .WithSettings(c =>
        {
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web) { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
            c.JsonSerializer = new DefaultJsonSerializer(options);
        })
        .WithHeader("User-Agent", "Kong Portal CLI")
        .WithOAuthBearerToken(kongOptions.Value.Token);

    public async Task<IReadOnlyList<ApiProduct>> GetApiProducts()
    {
        var allProducts = new List<ApiProduct>();

        PagedResponse<ApiProduct> productsResponse;
        var pageNumber = 1;

        do
        {
            var response = await _flurlClient.Request("api-products").SetQueryParam("page[number]", pageNumber++).GetAsync();

            productsResponse = await response.GetJsonAsync<PagedResponse<ApiProduct>>();

            allProducts.AddRange(productsResponse.Data);
        } while (productsResponse.Meta.Page.HasMore());

        return allProducts;
    }

    public async Task<IReadOnlyList<ApiProductVersion>> GetApiProductVersions(string apiProductId)
    {
        var allVersions = new List<ApiProductVersion>();

        PagedResponse<ApiProductVersion> versionsResponse;
        var pageNumber = 1;

        do
        {
            var response = await _flurlClient
                .Request($"api-products/{apiProductId}/product-versions")
                .SetQueryParam("page[number]", pageNumber++)
                .GetAsync();

            versionsResponse = await response.GetJsonAsync<PagedResponse<ApiProductVersion>>();

            allVersions.AddRange(versionsResponse.Data);
        } while (versionsResponse.Meta.Page.HasMore());

        return allVersions;
    }

    public async Task<IReadOnlyList<ApiProductDocument>> GetApiProductDocuments(string apiProductId)
    {
        var allDocuments = new List<ApiProductDocument>();

        PagedResponse<ApiProductDocument> documentsResponse;
        var pageNumber = 1;

        do
        {
            var response = await _flurlClient
                .Request($"api-products/{apiProductId}/documents")
                .SetQueryParam("page[number]", pageNumber++)
                .GetAsync();

            documentsResponse = await response.GetJsonAsync<PagedResponse<ApiProductDocument>>();

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

        var specificationsResponse = await response.GetJsonAsync<PagedResponse<ApiProductSpecification>>();

        return specificationsResponse.Data.FirstOrDefault();
    }

    public async Task<List<Models.Portal>> GetPortals()
    {
        var allPortals = new List<Models.Portal>();

        PagedResponse<Models.Portal> portalsResponse;
        var pageNumber = 1;

        do
        {
            var response = await _flurlClient.Request("portals").SetQueryParam("page[number]", pageNumber++).GetAsync();

            portalsResponse = await response.GetJsonAsync<PagedResponse<Models.Portal>>();

            allPortals.AddRange(portalsResponse.Data);
        } while (portalsResponse.Meta.Page.HasMore());

        return allPortals;
    }

    public async Task<List<ApiProduct>> GetPortalProducts(string portalId)
    {
        var allProducts = new List<ApiProduct>();

        PagedResponse<ApiProduct> productsResponse;
        var pageNumber = 1;

        do
        {
            var response = await _flurlClient.Request($"portals/{portalId}/products").SetQueryParam("page[number]", pageNumber++).GetAsync();

            productsResponse = await response.GetJsonAsync<PagedResponse<ApiProduct>>();

            allProducts.AddRange(productsResponse.Data);
        } while (productsResponse.Meta.Page.HasMore());

        return allProducts;
    }

    public async Task<PortalAppearance> GetPortalAppearance(string portalId)
    {
        var response = await _flurlClient.Request($"portals/{portalId}/appearance").GetAsync();
        return await response.GetJsonAsync<PortalAppearance>();
    }
}
