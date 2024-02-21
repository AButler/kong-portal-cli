namespace Kong.Portal.CLI.ApiClient;

internal class KongApiClientFactory
{
    public KongApiClient CreateClient(KongApiClientOptions options)
    {
        return new KongApiClient(options);
    }
}
