using System.Text.Json;
using System.Text.Json.Serialization;
using Flurl.Http;
using Flurl.Http.Configuration;
using Kong.Portal.CLI.Config;
using Microsoft.Extensions.Options;

namespace Kong.Portal.CLI.ApiClient;

internal class KongApiClient
{
    public KongApiClient(IOptions<KongOptions> kongOptions)
    {
        var flurlClient = new FlurlClient(kongOptions.Value.GetKongBaseUri())
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
            .WithOAuthBearerToken(kongOptions.Value.Token);

        ApiProducts = new ApiProductsClient(flurlClient);
        ApiProductVersions = new ApiProductVersionsClient(flurlClient);
        ApiProductDocuments = new ApiProductDocumentsClient(flurlClient);
        Portals = new PortalsClient(flurlClient);
    }

    public ApiProductsClient ApiProducts { get; }

    public ApiProductVersionsClient ApiProductVersions { get; }

    public ApiProductDocumentsClient ApiProductDocuments { get; }

    public PortalsClient Portals { get; }
}
