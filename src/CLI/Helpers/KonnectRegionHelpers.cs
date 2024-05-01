namespace Kong.Portal.CLI;

internal static class KonnectRegionHelpers
{
    public static string FromAddress(string konnectAddress)
    {
        if (konnectAddress.StartsWith(KongRegions.UsRegionAddress))
        {
            return "us";
        }

        if (konnectAddress.StartsWith(KongRegions.EuRegionAddress))
        {
            return "eu";
        }

        if (konnectAddress.StartsWith(KongRegions.AuRegionAddress))
        {
            return "au";
        }

        throw new ArgumentOutOfRangeException(nameof(konnectAddress), $"Unknown region for: {konnectAddress}");
    }
}
