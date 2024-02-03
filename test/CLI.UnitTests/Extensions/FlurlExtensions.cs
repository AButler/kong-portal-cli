using Flurl.Http.Content;
using Flurl.Http.Testing;

namespace CLI.UnitTests;

internal static class FlurlExtensions
{
    public static HttpTestSetup RespondWithDynamicJson(
        this HttpTestSetup httpTestSetup,
        Func<object> buildBody,
        int status = 200,
        object? headers = null,
        object? cookies = null,
        bool replaceUnderscoreWithHyphen = true
    )
    {
        return httpTestSetup.RespondWith(
            () =>
            {
                var s = httpTestSetup.Settings.JsonSerializer.Serialize(buildBody());
                return new CapturedJsonContent(s);
            },
            status,
            headers,
            cookies,
            replaceUnderscoreWithHyphen
        );
    }
}
