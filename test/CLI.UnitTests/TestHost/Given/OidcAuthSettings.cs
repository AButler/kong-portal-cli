namespace CLI.UnitTests.TestHost;

public record OidcAuthSettings(string Issuer, string ClientId, IReadOnlyCollection<string> Scopes, OidcClaimMappings ClaimMappings);
