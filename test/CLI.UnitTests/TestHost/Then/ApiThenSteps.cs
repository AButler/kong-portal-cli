﻿using System.Text;
using System.Text.Json;
using Flurl.Http.Testing;
using Kong.Portal.CLI;
using Kong.Portal.CLI.ApiClient;
using Kong.Portal.CLI.ApiClient.Models;

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

    public void ApiProductShouldNotHaveBeenUpdated(string productId)
    {
        var log = HttpTest.Current.CallLog.FirstOrDefault(l =>
            l.Request.Url == $"{_kongBaseUri}api-products/{productId}" && l.Request.Verb == HttpMethod.Patch
        );

        log.Should().BeNull();
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

    public void ApiProductVersionShouldHaveBeenCreated(string apiProductId)
    {
        HttpTest.Current.ShouldHaveCalled($"{_kongBaseUri}api-products/{apiProductId}/product-versions").WithVerb(HttpMethod.Post);
    }

    public void ApiProductDocumentShouldHaveBeenCreated(string apiProductId, string? documentContentsContaining = null)
    {
        var call = HttpTest.Current.CallLog.FirstOrDefault(c =>
            c.Request.Url.ToString() == $"{_kongBaseUri}api-products/{apiProductId}/documents" && c.Request.Verb == HttpMethod.Post
        );

        call.Should().NotBeNull();

        if (documentContentsContaining != null)
        {
            var request = Deserialize<ApiProductDocumentUpdate>(call!.RequestBody);
            request.Should().NotBeNull();

            var content = Encoding.UTF8.GetString(Convert.FromBase64String(request!.Content));
            content.Should().Contain(documentContentsContaining);
        }
    }

    public void PortalShouldHaveBeenUpdated(string portalId)
    {
        HttpTest.Current.ShouldHaveCalled($"{_kongBaseUri}portals/{portalId}").WithVerb(HttpMethod.Patch);
    }

    public void PortalAppearanceShouldHaveBeenUpdated(string portalId)
    {
        HttpTest.Current.ShouldHaveCalled($"{_kongBaseUri}portals/{portalId}/appearance").WithVerb(HttpMethod.Patch);
    }

    public async Task<string> GetPortalTeamId(string portalId, string teamName)
    {
        var call = HttpTest.Current.CallLog.FirstOrDefault(c =>
            c.Request.Url.ToString() == $"{_kongBaseUri}portals/{portalId}/teams"
            && c.Request.Verb == HttpMethod.Post
            && Deserialize<DevPortalTeamUpdate>(c.RequestBody)?.Name == teamName
        );

        var entity = await call!.Response.GetJsonAsync<EntityWithId>();

        return entity.Id;
    }

    public void PortalAuthSettingsShouldHaveBeenUpdated(string portalId)
    {
        HttpTest.Current.ShouldHaveCalled($"{_kongBaseUri}portals/{portalId}/authentication-settings").WithVerb(HttpMethod.Patch);
    }

    public void PortalTeamShouldHaveBeenCreated(string portalId)
    {
        HttpTest.Current.ShouldHaveCalled($"{_kongBaseUri}portals/{portalId}/teams").WithVerb(HttpMethod.Post);
    }

    public void PortalTeamShouldHaveBeenUpdated(string portalId)
    {
        HttpTest.Current.ShouldHaveCalled($"{_kongBaseUri}portals/{portalId}/teams/*").WithVerb(HttpMethod.Patch);
    }

    public void NoPortalTeamsShouldHaveBeenCreated(string portalId)
    {
        var call = HttpTest.Current.CallLog.FirstOrDefault(c =>
            c.Request.Url.ToString() == $"{_kongBaseUri}portals/{portalId}/teams" && c.Request.Verb == HttpMethod.Post
        );

        call.Should().BeNull();
    }

    public void NoPortalTeamsShouldHaveBeenUpdated(string portalId)
    {
        var call = HttpTest.Current.CallLog.FirstOrDefault(c =>
            c.Request.Url.ToString() == $"{_kongBaseUri}portals/{portalId}/teams" && c.Request.Verb == HttpMethod.Patch
        );

        call.Should().BeNull();
    }

    public void PortalTeamRoleShouldHaveBeenAssigned(string portalId, string teamId)
    {
        HttpTest.Current.ShouldHaveCalled($"{_kongBaseUri}portals/{portalId}/teams/{teamId}/assigned-roles").WithVerb(HttpMethod.Post);
    }

    public void PortalTeamRoleShouldHaveBeenRemoved(string portalId, string teamId, string roleId)
    {
        HttpTest.Current.ShouldHaveCalled($"{_kongBaseUri}portals/{portalId}/teams/{teamId}/assigned-roles/{roleId}").WithVerb(HttpMethod.Delete);
    }

    public void PortalTeamMappingsShouldHaveBeenUpdated(string portalId)
    {
        HttpTest.Current.ShouldHaveCalled($"{_kongBaseUri}portals/{portalId}/identity-provider/team-group-mappings").WithVerb(HttpMethod.Patch);
    }

    private T? Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, MetadataSerializer.SerializerOptions);
    }

    private record EntityWithId(string Id);
}
