using Flurl.Http;
using Kong.Portal.CLI.ApiClient.Models;

namespace Kong.Portal.CLI.ApiClient;

internal class DevPortalsClient(IFlurlClient flurlClient)
{
    public async Task<IReadOnlyCollection<DevPortal>> GetAll()
    {
        return await flurlClient.GetKongPagedResults<DevPortal>("portals");
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
        return await flurlClient.GetKongPagedResults<DevPortalTeam>($"/portals/{portalId}/teams");
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

    public async Task<IReadOnlyCollection<ApiProduct>> GetApiProducts(string portalId)
    {
        return await flurlClient.GetKongPagedResults<ApiProduct>($"portals/{portalId}/products");
    }

    public async Task<IReadOnlyCollection<DevPortalTeamRole>> GetTeamRoles(string portalId, string teamId)
    {
        return await flurlClient.GetKongPagedResults<DevPortalTeamRole>($"portals/{portalId}/teams/{teamId}/assigned-roles");
    }
}
