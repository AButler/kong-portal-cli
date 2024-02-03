namespace Kong.Portal.CLI.Config;

public class KongOptions
{
    public string Token { get; set; } = "";
    public string Region { get; set; } = "eu";

    public string GetKongBaseUri() => $"https://{Region}.api.konghq.com/v2/";
}
