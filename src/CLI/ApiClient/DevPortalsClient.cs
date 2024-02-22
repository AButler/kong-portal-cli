using Flurl.Http;
using Kong.Portal.CLI.ApiClient.Models;

namespace Kong.Portal.CLI.ApiClient;

internal class DevPortalsClient(IFlurlClient flurlClient)
{
    public async Task<List<DevPortal>> GetAll()
    {
        var allPortals = new List<DevPortal>();

        PagedResponse<DevPortal> portalsResponse;
        var pageNumber = 1;

        do
        {
            var response = await flurlClient.Request("portals").SetQueryParam("page[number]", pageNumber++).GetAsync();

            portalsResponse = await response.GetJsonAsync<PagedResponse<DevPortal>>();

            allPortals.AddRange(portalsResponse.Data);
        } while (portalsResponse.Meta.Page.HasMore());

        return allPortals;
    }

    public async Task<DevPortal> Update(string portalId, DevPortal devPortal)
    {
        var response = await flurlClient.Request($"portals/{portalId}").PatchJsonAsync(devPortal.ToUpdateModel());

        return await response.GetJsonAsync<DevPortal>();
    }

    public async Task<DevPortalAppearance> GetAppearance(string portalId)
    {
        var response = await flurlClient.Request($"portals/{portalId}/appearance").GetAsync();
        return await response.GetJsonAsync<DevPortalAppearance>();
    }

    public async Task<DevPortalAppearance> UpdateAppearance(string portalId, DevPortalAppearance devPortalAppearance)
    {
        var response = await flurlClient.Request($"portals/{portalId}/appearance").PatchJsonAsync(devPortalAppearance);

        return await response.GetJsonAsync<DevPortalAppearance>();
    }
}
