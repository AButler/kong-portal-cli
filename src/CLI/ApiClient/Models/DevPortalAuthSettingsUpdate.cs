namespace Kong.Portal.CLI.ApiClient.Models;

internal record DevPortalAuthSettingsUpdate(
    bool BasicAuthEnabled,
    bool OidcAuthEnabled,
    bool OidcTeamMappingEnabled,
    bool KonnectMappingEnabled,
    string? OidcIssuer,
    string? OidcClientId,
    IReadOnlyCollection<string>? OidcScopes,
    DevPortalClaimMappings? OidcClaimMappings
);
