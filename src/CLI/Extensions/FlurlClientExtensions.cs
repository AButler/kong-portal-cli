using Flurl.Http;
using Kong.Portal.CLI.ApiClient.Models;

namespace Kong.Portal.CLI;

internal static class FlurlClientExtensions
{
    public static async Task<List<T>> GetKongPagedResults<T>(this IFlurlClient client, string url)
    {
        var allItems = new List<T>();

        PagedResponse<T> responsePage;
        var pageNumber = 1;

        do
        {
            var response = await client.Request(url).SetQueryParam("page[number]", pageNumber++).GetAsync();

            responsePage = await response.GetJsonAsync<PagedResponse<T>>();

            allItems.AddRange(responsePage.Data);
        } while (responsePage.Meta.Page.HasMore());

        return allItems;
    }
}
