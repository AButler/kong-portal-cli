namespace CLI.UnitTests.TestHost;

public record AuthSettings(
    Discretionary<bool> BasicAuthEnabled = default,
    Discretionary<bool> OidcAuthEnabled = default,
    Discretionary<bool> OidcTeamMappingEnabled = default,
    Discretionary<bool> KonnectMappingEnabled = default,
    Discretionary<OidcAuthSettings?> OidcConfig = default
);
