namespace Kong.Portal.CLI.Services;

internal record PortalAuthSettingsMetadata(
    bool BasicAuthEnabled,
    bool OidcAuthEnabled,
    bool OidcTeamMappingEnabled,
    bool KonnectMappingEnabled,
    PortalOidcConfig? OidcConfig
);

internal record PortalOidcConfig(
    string Issuer,
    string ClientId,
    string ClientSecret,
    IReadOnlyCollection<string> Scopes,
    PortalClaimMappings ClaimMappings
);

internal record PortalClaimMappings(string Name, string Email, string Groups);
