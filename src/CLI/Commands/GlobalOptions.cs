using System.CommandLine;

namespace Kong.Portal.CLI.Commands;

internal static class GlobalOptions
{
    private static readonly string[] Regions = ["https://us.api.konghq.com", "https://eu.api.konghq.com", "https://au.api.konghq.com"];

    public static Option<string> TokenOption { get; } = new("--konnect-token", "Token associated with your Konnect account") { IsRequired = true };
    public static Option<string> KonnectAddressOption { get; } =
        new Option<string>("--konnect-addr", () => Regions[0], $"Address of the Konnect endpoint") { IsRequired = true }.FromAmong(Regions);
}
