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

    public async Task<DevPortalAuthSettings> GetAuthSettings(string portalId)
    {
        var response = await flurlClient.Request($"portals/{portalId}/authentication-settings").GetAsync();

        return await response.GetJsonAsync<DevPortalAuthSettings>();
    }

    public async Task<DevPortalAuthSettings> UpdateAuthSettings(string portalId, DevPortalAuthSettings devPortalAuthSettings)
    {
        var response = await flurlClient.Request($"portals/{portalId}/authentication-settings").PatchJsonAsync(devPortalAuthSettings.ToUpdateModel());

        return await response.GetJsonAsync<DevPortalAuthSettings>();
    }

    public async Task<IReadOnlyList<DevPortalTeam>> GetTeams(string portalId)
    {
        var allTeams = new List<DevPortalTeam>();

        PagedResponse<DevPortalTeam> teamsResponse;
        var pageNumber = 1;

        do
        {
            var response = await flurlClient.Request($"/portals/{portalId}/teams").SetQueryParam("page[number]", pageNumber++).GetAsync();

            teamsResponse = await response.GetJsonAsync<PagedResponse<DevPortalTeam>>();

            allTeams.AddRange(teamsResponse.Data);
        } while (teamsResponse.Meta.Page.HasMore());

        return allTeams;
    }

    public async Task<DevPortalTeam> CreateTeam(string portalId, DevPortalTeam team)
    {
        var response = await flurlClient.Request($"/portals/{portalId}/teams").PostJsonAsync(team.ToUpdateModel());

        return await response.GetJsonAsync<DevPortalTeam>();
    }

    public async Task<DevPortalTeam> UpdateTeam(string portalId, DevPortalTeam team)
    {
        var response = await flurlClient.Request($"/portals/{portalId}/teams/{team.Id}").PatchJsonAsync(team.ToUpdateModel());

        return await response.GetJsonAsync<DevPortalTeam>();
    }

    public async Task DeleteTeam(string portalId, string teamId)
    {
        await flurlClient.Request($"/portals/{portalId}/teams/{teamId}").DeleteAsync();
    }
}
