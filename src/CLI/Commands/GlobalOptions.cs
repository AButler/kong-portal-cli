using System.CommandLine;

namespace Kong.Portal.CLI.Commands;

internal static class GlobalOptions
{
    public static Option<string> TokenOption { get; } =
        new("--konnect-token") { Description = "Token associated with your Konnect account", Recursive = true };

    public static Option<FileInfo> TokenFileOption { get; } =
        new("--konnect-token-file") { Description = "File containing the token associated with your Konnect account", Recursive = true };

    public static Option<string> KonnectAddressOption { get; } =
        new("--konnect-addr")
        {
            Description = "Address of the Konnect endpoint",
            DefaultValueFactory = _ => KongRegions.UsRegionAddress,
            Recursive = true,
        };

    public static Option<bool> Debug { get; } =
        new("--debug")
        {
            Description = "Debug",
            Hidden = true,
            Recursive = true,
        };
}
