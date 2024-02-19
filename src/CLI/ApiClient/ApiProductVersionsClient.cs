using Flurl.Http;
using Kong.Portal.CLI.ApiClient.Models;

namespace Kong.Portal.CLI.ApiClient;

internal class ApiProductVersionsClient(IFlurlClient flurlClient)
{
    public async Task<IReadOnlyList<ApiProductVersion>> GetAll(string apiProductId)
    {
        var allVersions = new List<ApiProductVersion>();

        PagedResponse<ApiProductVersion> versionsResponse;
        var pageNumber = 1;

        do
        {
            var response = await flurlClient
                .Request($"api-products/{apiProductId}/product-versions")
                .SetQueryParam("page[number]", pageNumber++)
                .GetAsync();

            versionsResponse = await response.GetJsonAsync<PagedResponse<ApiProductVersion>>();

            allVersions.AddRange(versionsResponse.Data);
        } while (versionsResponse.Meta.Page.HasMore());

        return allVersions;
    }

    public async Task<ApiProductSpecification?> GetSpecification(string apiProductId, string productVersionId)
    {
        var response = await flurlClient.Request($"api-products/{apiProductId}/product-versions/{productVersionId}/specifications").GetAsync();

        var specificationsResponse = await response.GetJsonAsync<PagedResponse<ApiProductSpecification>>();

        return specificationsResponse.Data.FirstOrDefault();
    }
}
