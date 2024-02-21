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
        var response = await flurlClient.Request("api-products").PostJsonAsync(apiProduct.ToCreateModel());

        var createdApiProduct = await response.GetJsonAsync<ApiProduct>();

        if (apiProduct.PortalIds.Count > 0)
        {
            // Cannot publish API products in the same call as create so have to do a subsequent update
            return await Update(createdApiProduct.Id, apiProduct);
        }

        return createdApiProduct;
    }

    public async Task<ApiProduct> Update(string apiProductId, ApiProduct apiProduct)
    {
        var response = await flurlClient.Request($"api-products/{apiProductId}").PatchJsonAsync(apiProduct.ToUpdateModel());

        return await response.GetJsonAsync<ApiProduct>();
    }

    public async Task Delete(string apiProductId)
    {
        await flurlClient.Request($"api-products/{apiProductId}").DeleteAsync();
    }
}
