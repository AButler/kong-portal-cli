using System.Text.Json;
using Flurl.Http.Testing;
using Kong.Portal.CLI;
using Kong.Portal.CLI.ApiClient;
using Kong.Portal.CLI.ApiClient.Models;
using Kong.Portal.CLI.Services;

namespace CLI.UnitTests.TestHost;

internal class ApiThenSteps(KongApiClientOptions apiClientOptions)
{
    private readonly string _kongBaseUri = $"{apiClientOptions.BaseUrl}/v2/";

    public void ShouldNotHaveReceivedAnyUpdates()
    {
        var anyUpdates = HttpTest.Current.CallLog.FirstOrDefault(c =>
            c.Request.Url.ToString().StartsWith(_kongBaseUri) && c.Request.Verb != HttpMethod.Get
        );

        if (anyUpdates != null)
        {
            anyUpdates.Should().BeNull("at least one call to update Kong API");
        }
    }

    public void ApiProductShouldHaveBeenUpdated(string productId)
    {
        HttpTest.Current.ShouldHaveCalled($"{_kongBaseUri}api-products/{productId}").WithVerb(HttpMethod.Patch);
    }

    public void ApiProductShouldHaveBeenDeleted(string productId)
    {
        HttpTest.Current.ShouldHaveCalled($"{_kongBaseUri}api-products/{productId}").WithVerb(HttpMethod.Delete);
    }

    public void ApiProductShouldHaveBeenCreated(string syncId)
    {
        var call = HttpTest.Current.CallLog.FirstOrDefault(c =>
            c.Request.Url.ToString() == $"{_kongBaseUri}api-products"
            && c.Request.Verb == HttpMethod.Post
            && Deserialize<ApiProductUpdate>(c.RequestBody)?.Labels[Constants.SyncIdLabel] == syncId
        );

        call.Should().NotBeNull("expected /api-products to be called with a POST");
    }

    public async Task<string> GetApiProductId(string syncId)
    {
        var call = HttpTest.Current.CallLog.FirstOrDefault(c =>
            c.Request.Url.ToString() == $"{_kongBaseUri}api-products"
            && c.Request.Verb == HttpMethod.Post
            && Deserialize<ApiProductUpdate>(c.RequestBody)?.Labels[Constants.SyncIdLabel] == syncId
        );

        var entity = await call!.Response.GetJsonAsync<EntityWithId>();

        return entity.Id;
    }

    public void ApiProductVersionShouldHaveBeenCreated(string apiProductId, string versionName)
    {
        HttpTest.Current.ShouldHaveCalled($"{_kongBaseUri}api-products/{apiProductId}/product-versions").WithVerb(HttpMethod.Post);
    }

    public void PortalShouldHaveBeenUpdated(string portalId)
    {
        HttpTest.Current.ShouldHaveCalled($"{_kongBaseUri}portals/{portalId}").WithVerb(HttpMethod.Patch);
    }

    public void PortalAppearanceShouldHaveBeenUpdated(string portalId)
    {
        HttpTest.Current.ShouldHaveCalled($"{_kongBaseUri}portals/{portalId}/appearance").WithVerb(HttpMethod.Patch);
    }

    private T? Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, MetadataSerializer.SerializerOptions);
    }

    private record EntityWithId(string Id);
}
