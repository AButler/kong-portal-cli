using System.CommandLine;

namespace Kong.Portal.CLI.Commands;

internal static class GlobalOptions
{
    public static Option<string> TokenOption { get; } = new("--konnect-token", "Token associated with your Konnect account");
    public static Option<string> TokenFileOption { get; } =
        new("--konnect-token-file", "File containing the token associated with your Konnect account");

    public static Option<string> KonnectAddressOption { get; } = new("--konnect-addr", () => KongRegions.UsRegion, "Address of the Konnect endpoint");
}
