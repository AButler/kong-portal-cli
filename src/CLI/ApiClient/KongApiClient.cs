using System.Text.Json;
using System.Text.Json.Serialization;
using Flurl.Http;
using Flurl.Http.Configuration;

namespace Kong.Portal.CLI.ApiClient;

internal class KongApiClient
{
    public KongApiClient(KongApiClientOptions options)
    {
        var flurlClient = new FlurlClient(options.BaseUrl + "/v2/")
            .WithSettings(c =>
            {
                var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower) }
                };

                c.JsonSerializer = new DefaultJsonSerializer(options);
            })
            .WithHeader("User-Agent", "Kong Portal CLI")
            .WithOAuthBearerToken(options.Token);

        ApiProducts = new ApiProductsClient(flurlClient);
        ApiProductVersions = new ApiProductVersionsClient(flurlClient);
        ApiProductDocuments = new ApiProductDocumentsClient(flurlClient);
        DevPortals = new DevPortalsClient(flurlClient);
    }

    public ApiProductsClient ApiProducts { get; }

    public ApiProductVersionsClient ApiProductVersions { get; }

    public ApiProductDocumentsClient ApiProductDocuments { get; }

    public DevPortalsClient DevPortals { get; }
}
