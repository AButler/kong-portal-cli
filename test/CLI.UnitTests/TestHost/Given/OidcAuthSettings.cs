namespace CLI.UnitTests.TestHost;

public record OidcAuthSettings(
    string Issuer,
    string ClientId,
    string ClientSecret,
    IReadOnlyCollection<string> Scopes,
    OidcClaimMappings ClaimMappings
);
