﻿namespace Kong.Portal.CLI.ApiClient.Models;

internal record ApiProduct(string Id, string Name, string Description, IReadOnlyCollection<string> PortalIds, LabelDictionary Labels)
{
    public virtual bool Equals(ApiProduct? other)
    {
        if (other == null)
        {
            return false;
        }

        if (Id != other.Id)
        {
            return false;
        }

        if (Name != other.Name)
        {
            return false;
        }

        if (Description != other.Description)
        {
            return false;
        }

        if (!PortalIds.OrderBy(s => s).SequenceEqual(other.PortalIds.OrderBy(s => s)))
        {
            return false;
        }

        if (!Labels.Equals(other.Labels))
        {
            return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Name, Description, PortalIds, Labels);
    }
}
