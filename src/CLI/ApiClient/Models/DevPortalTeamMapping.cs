namespace Kong.Portal.CLI.ApiClient.Models;

internal record DevPortalTeamMapping(string TeamId, IReadOnlyCollection<string> Groups)
{
    public virtual bool Equals(DevPortalTeamMapping? other)
    {
        if (other == null)
        {
            return false;
        }

        if (TeamId != other.TeamId)
        {
            return false;
        }

        if (!Groups.OrderBy(g => g).SequenceEqual(other.Groups.OrderBy(g => g)))
        {
            return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(TeamId, Groups);
    }
}
