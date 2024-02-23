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

    public async Task<ApiProductDocumentBody> Create(string apiProductId, ApiProductDocumentBody apiProductDocument)
    {
        var response = await flurlClient.Request($"api-products/{apiProductId}/documents").PostJsonAsync(apiProductDocument.ToUpdateModel());

        return await response.GetJsonAsync<ApiProductDocumentBody>();
    }

    public async Task<ApiProductDocumentBody> Update(string apiProductId, string documentId, ApiProductDocumentBody apiProductDocument)
    {
        var response = await flurlClient
            .Request($"api-products/{apiProductId}/documents/{documentId}")
            .PatchJsonAsync(apiProductDocument.ToUpdateModel());

        return await response.GetJsonAsync<ApiProductDocumentBody>();
    }

    public async Task Delete(string apiProductId, string documentId)
    {
        await flurlClient.Request($"api-products/{apiProductId}/documents/{documentId}").DeleteAsync();
    }
}
