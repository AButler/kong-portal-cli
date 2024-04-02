namespace Kong.Portal.CLI.ApiClient.Models;

internal record DevPortalAuthSettings(
    bool BasicAuthEnabled,
    bool OidcAuthEnabled,
    bool OidcTeamMappingEnabled,
    bool KonnectMappingEnabled,
    DevPortalOidcConfig? OidcConfig
);

internal record DevPortalOidcConfig(string Issuer, string ClientId, IReadOnlyCollection<string> Scopes, DevPortalClaimMappings ClaimMappings);

internal record DevPortalClaimMappings(string Name, string Email, string Groups);
