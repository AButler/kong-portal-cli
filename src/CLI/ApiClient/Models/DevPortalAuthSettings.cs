namespace Kong.Portal.CLI.ApiClient.Models;

internal record DevPortalAuthSettings(
    bool BasicAuthEnabled,
    bool OidcAuthEnabled,
    bool OidcTeamMappingEnabled,
    bool KonnectMappingEnabled,
    DevPortalOidcConfig? OidcConfig
);

internal record DevPortalOidcConfig(string Issuer, string ClientId, IReadOnlyCollection<string> Scopes, DevPortalClaimMappings ClaimMappings)
{
    public virtual bool Equals(DevPortalOidcConfig? other)
    {
        if (other == null)
        {
            return false;
        }

        if (Issuer != other.Issuer)
        {
            return false;
        }

        if (ClientId != other.ClientId)
        {
            return false;
        }

        if (!Scopes.OrderBy(s => s).SequenceEqual(other.Scopes.OrderBy(s => s)))
        {
            return false;
        }

        if (ClaimMappings != other.ClaimMappings)
        {
            return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Issuer.GetHashCode(), ClientId.GetHashCode(), Scopes.GetHashCode(), ClaimMappings.GetHashCode());
    }
}

internal record DevPortalClaimMappings(string Name, string Email, string Groups);
