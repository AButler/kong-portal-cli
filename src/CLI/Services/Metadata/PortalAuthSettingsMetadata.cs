namespace Kong.Portal.CLI.Services;

internal record PortalAuthSettingsMetadata(
    bool BasicAuthEnabled,
    bool OidcAuthEnabled,
    bool OidcTeamMappingEnabled,
    bool KonnectMappingEnabled,
    PortalOidcConfig? OidcConfig,
    IReadOnlyCollection<PortalAuthTeamMapping>? OidcTeamMappings
);

internal record PortalOidcConfig(
    string Issuer,
    string ClientId,
    string ClientSecret,
    IReadOnlyCollection<string> Scopes,
    PortalClaimMappings ClaimMappings
);

internal record PortalClaimMappings(string Name, string Email, string Groups);

internal record PortalAuthTeamMapping(string Team, IReadOnlyCollection<string> OidcGroups);
