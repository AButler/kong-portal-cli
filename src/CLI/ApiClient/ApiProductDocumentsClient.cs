using Flurl.Http;
using Kong.Portal.CLI.ApiClient.Models;

namespace Kong.Portal.CLI.ApiClient;

internal class ApiProductDocumentsClient(IFlurlClient flurlClient)
{
    public async Task<IReadOnlyList<ApiProductDocument>> GetAll(string apiProductId)
    {
        var allDocuments = new List<ApiProductDocument>();

        PagedResponse<ApiProductDocument> documentsResponse;
        var pageNumber = 1;

        do
        {
            var response = await flurlClient.Request($"api-products/{apiProductId}/documents").SetQueryParam("page[number]", pageNumber++).GetAsync();

            documentsResponse = await response.GetJsonAsync<PagedResponse<ApiProductDocument>>();

            allDocuments.AddRange(documentsResponse.Data);
        } while (documentsResponse.Meta.Page.HasMore());

        return allDocuments;
    }

    public async Task<ApiProductDocumentBody> GetBody(string apiProductId, string documentId)
    {
        var response = await flurlClient.Request($"api-products/{apiProductId}/documents/{documentId}").GetAsync();

        return await response.GetJsonAsync<ApiProductDocumentBody>();
    }
}
