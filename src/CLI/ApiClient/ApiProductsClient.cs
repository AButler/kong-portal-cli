using Flurl.Http;
using Kong.Portal.CLI.ApiClient.Models;

namespace Kong.Portal.CLI.ApiClient;

internal class ApiProductsClient(IFlurlClient flurlClient)
{
    public async Task<IReadOnlyList<ApiProduct>> GetAll()
    {
        return await flurlClient.GetKongPagedResults<ApiProduct>("api-products");
    }

    public async Task<ApiProduct> Create(ApiProduct apiProduct)
    {
        var response = await flurlClient.Request("api-products").PostJsonAsync(apiProduct.ToCreateModel());

        return await response.GetJsonAsync<ApiProduct>();
    }

    public async Task<ApiProduct> Update(ApiProduct apiProduct)
    {
        var response = await flurlClient.Request($"api-products/{apiProduct.Id}").PatchJsonAsync(apiProduct.ToUpdateModel());

        return await response.GetJsonAsync<ApiProduct>();
    }

    public async Task Delete(string apiProductId)
    {
        await flurlClient.Request($"api-products/{apiProductId}").DeleteAsync();
    }

    public async Task UpdateAssociations(string apiProductId, IReadOnlyCollection<string> portalIds)
    {
        var body = new ApiProductAssociationUpdate(portalIds);
        await flurlClient.Request($"api-products/{apiProductId}").PatchJsonAsync(body);
    }
}
