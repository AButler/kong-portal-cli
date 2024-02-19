using Flurl.Http;
using Kong.Portal.CLI.ApiClient.Models;

namespace Kong.Portal.CLI.ApiClient;

internal class PortalsClient(IFlurlClient flurlClient)
{
    public async Task<List<Models.Portal>> GetAll()
    {
        var allPortals = new List<Models.Portal>();

        PagedResponse<Models.Portal> portalsResponse;
        var pageNumber = 1;

        do
        {
            var response = await flurlClient.Request("portals").SetQueryParam("page[number]", pageNumber++).GetAsync();

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
            var response = await flurlClient.Request($"portals/{portalId}/products").SetQueryParam("page[number]", pageNumber++).GetAsync();

            productsResponse = await response.GetJsonAsync<PagedResponse<ApiProduct>>();

            allProducts.AddRange(productsResponse.Data);
        } while (productsResponse.Meta.Page.HasMore());

        return allProducts;
    }

    public async Task<PortalAppearance> GetAppearance(string portalId)
    {
        var response = await flurlClient.Request($"portals/{portalId}/appearance").GetAsync();
        return await response.GetJsonAsync<PortalAppearance>();
    }
}
