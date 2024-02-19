using Flurl.Http;
using Kong.Portal.CLI.ApiClient.Models;

namespace Kong.Portal.CLI.ApiClient;

internal class ApiProductsClient(IFlurlClient flurlClient)
{
    public async Task<IReadOnlyList<ApiProduct>> GetAll()
    {
        var allProducts = new List<ApiProduct>();

        PagedResponse<ApiProduct> productsResponse;
        var pageNumber = 1;

        do
        {
            var response = await flurlClient.Request("api-products").SetQueryParam("page[number]", pageNumber++).GetAsync();

            productsResponse = await response.GetJsonAsync<PagedResponse<ApiProduct>>();

            allProducts.AddRange(productsResponse.Data);
        } while (productsResponse.Meta.Page.HasMore());

        return allProducts;
    }

    public async Task<ApiProduct> Create(ApiProduct apiProduct)
    {
        var response = await flurlClient.Request("api-products").PostJsonAsync(apiProduct);

        return await response.GetJsonAsync<ApiProduct>();
    }

    public async Task<ApiProduct> Update(string apiProductId, ApiProduct apiProduct)
    {
        var response = await flurlClient.Request($"api-products/{Uri.EscapeDataString(apiProductId)}").PatchJsonAsync(apiProduct);

        return await response.GetJsonAsync<ApiProduct>();
    }

    public async Task Delete(string apiProductId)
    {
        await flurlClient.Request($"api-products/{Uri.EscapeDataString(apiProductId)}").DeleteAsync();
    }
}
