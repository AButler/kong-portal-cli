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

    public async Task<ApiProductVersion> Create(string apiProductId, ApiProductVersion apiProductVersion)
    {
        var response = await flurlClient.Request($"api-products/{apiProductId}/product-versions").PostJsonAsync(apiProductVersion.ToUpdateModel());

        return await response.GetJsonAsync<ApiProductVersion>();
    }

    public async Task<ApiProductVersion> Update(string apiProductId, ApiProductVersion apiProductVersion)
    {
        var response = await flurlClient
            .Request($"api-products/{apiProductId}/product-versions/{apiProductVersion.Id}")
            .PatchJsonAsync(apiProductVersion.ToUpdateModel());

        return await response.GetJsonAsync<ApiProductVersion>();
    }

    public async Task Delete(string apiProductId, string apiProductVersionId)
    {
        await flurlClient.Request($"api-products/{apiProductId}/product-versions/{apiProductVersionId}").DeleteAsync();
    }

    public async Task<ApiProductSpecification?> GetSpecification(string apiProductId, string apiProductVersionId)
    {
        var response = await flurlClient.Request($"api-products/{apiProductId}/product-versions/{apiProductVersionId}/specifications").GetAsync();

        var specificationsResponse = await response.GetJsonAsync<PagedResponse<ApiProductSpecification>>();

        return specificationsResponse.Data.FirstOrDefault();
    }

    public async Task<ApiProductSpecification> CreateSpecification(
        string apiProductId,
        string apiProductVersionId,
        ApiProductSpecification apiProductVersionSpecification
    )
    {
        var response = await flurlClient
            .Request($"api-products/{apiProductId}/product-versions/{apiProductVersionId}/specifications")
            .PostJsonAsync(apiProductVersionSpecification.ToUpdateModel());

        return await response.GetJsonAsync<ApiProductSpecification>();
    }

    public async Task<ApiProductSpecification> UpdateSpecification(
        string apiProductId,
        string apiProductVersionId,
        string specificationId,
        ApiProductSpecification apiProductVersionSpecification
    )
    {
        var response = await flurlClient
            .Request($"api-products/{apiProductId}/product-versions/{apiProductVersionId}/specifications/{specificationId}")
            .PatchJsonAsync(apiProductVersionSpecification.ToUpdateModel());

        return await response.GetJsonAsync<ApiProductSpecification>();
    }

    public async Task DeleteSpecification(string apiProductId, string apiProductVersionId, string specificationId)
    {
        await flurlClient
            .Request($"api-products/{apiProductId}/product-versions/{apiProductVersionId}/specifications/{specificationId}")
            .DeleteAsync();
    }
}
