using System.Text.Json;
using System.Text.Json.Serialization;
using Flurl.Http;
using Flurl.Http.Configuration;
using Pastel;

namespace Kong.Portal.CLI.ApiClient;

internal class KongApiClient
{
    public KongApiClient(KongApiClientOptions options)
    {
        var flurlClient = new FlurlClient(options.BaseUrl + "/v2/")
            .WithSettings(c =>
            {
                var serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower) }
                };

                c.JsonSerializer = new DefaultJsonSerializer(serializerOptions);
            })
            .WithHeader("User-Agent", "Kong Portal CLI")
            .WithOAuthBearerToken(options.Token)
            .OnError(async call =>
            {
                if (options.DebugLoggingEnabled)
                {
                    var request = call.Request;
                    var response = call.Response;
                    Console.WriteLine(
                        response == null
                            ? "[HTTP ERROR: NO RESPONSE]".Pastel(ConsoleColor.Red)
                            : $"[HTTP ERROR {response.StatusCode}] {request.Verb} {request.Url}".Pastel(ConsoleColor.Red)
                    );

                    Console.WriteLine("Request:");
                    Console.WriteLine(call.RequestBody);

                    if (response != null)
                    {
                        Console.WriteLine("Response:");
                        Console.WriteLine(await response.GetStringAsync());
                    }

                    if (call.Exception != null)
                    {
                        Console.WriteLine("Exception:");
                        Console.WriteLine(call.Exception);
                    }
                }
            });

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
