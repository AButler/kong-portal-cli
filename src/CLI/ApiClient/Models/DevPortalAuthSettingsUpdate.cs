namespace Kong.Portal.CLI.ApiClient.Models;

internal record DevPortalAuthSettingsUpdate(
    bool BasicAuthEnabled,
    bool OidcAuthEnabled,
    bool OidcTeamMappingEnabled,
    bool KonnectMappingEnabled,
    string? OidcIssuer,
    string? OidcClientId,
    string? OidcClientSecret,
    IReadOnlyCollection<string>? OidcScopes,
    DevPortalClaimMappings? OidcClaimMappings
);
