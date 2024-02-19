using Flurl.Http.Testing;
using Kong.Portal.CLI.Config;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic.CompilerServices;

namespace CLI.UnitTests.TestHost;

internal class ApiThenSteps(IOptions<KongOptions> kongOptions)
{
    private readonly string _kongBaseUri = kongOptions.Value.GetKongBaseUri();

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

    public void ApiProductShouldHaveBeenCreated()
    {
        HttpTest.Current.ShouldHaveCalled($"{_kongBaseUri}api-products").WithVerb(HttpMethod.Post);
    }
}
