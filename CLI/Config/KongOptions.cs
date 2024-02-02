namespace Kong.Portal.CLI.Config;

public class KongOptions
{
    public string Token { get; set; } = "";
    public string Region { get; set; } = "eu";

    public Uri GetKongBaseUri() => new($"https://{Region}.api.konghq.com/v2/");
}
