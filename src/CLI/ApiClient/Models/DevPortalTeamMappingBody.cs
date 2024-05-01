namespace Kong.Portal.CLI.ApiClient.Models;

internal record DevPortalTeamMappingBody(IReadOnlyCollection<DevPortalTeamMapping> Data)
{
    public virtual bool Equals(DevPortalTeamMappingBody? other)
    {
        if (other == null)
        {
            return false;
        }

        if (!Data.OrderBy(tm => tm.TeamId).SequenceEqual(other.Data.OrderBy(tm => tm.TeamId)))
        {
            return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        return Data.GetHashCode();
    }
}
