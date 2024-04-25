namespace Kong.Portal.CLI.ApiClient.Models;

internal record ApiProductAssociation(IReadOnlyCollection<string> Portals)
{
    public virtual bool Equals(ApiProductAssociation? other)
    {
        if (other == null)
        {
            return false;
        }

        if (!Portals.OrderBy(id => id).SequenceEqual(other.Portals.OrderBy(id => id)))
        {
            return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        return Portals.GetHashCode();
    }
}
