using Flurl.Http;
using Kong.Portal.CLI.ApiClient.Models;

namespace Kong.Portal.CLI.ApiClient;

internal class ApiProductDocumentsClient(IFlurlClient flurlClient)
{
    public async Task<IReadOnlyList<ApiProductDocument>> GetAll(string apiProductId)
    {
        return await flurlClient.GetKongPagedResults<ApiProductDocument>($"api-products/{apiProductId}/documents");
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

    public async Task<ApiProductDocumentBody> Update(string apiProductId, ApiProductDocumentBody apiProductDocument)
    {
        var response = await flurlClient
            .Request($"api-products/{apiProductId}/documents/{apiProductDocument.Id}")
            .PatchJsonAsync(apiProductDocument.ToUpdateModel());

        return await response.GetJsonAsync<ApiProductDocumentBody>();
    }

    public async Task Delete(string apiProductId, string documentId)
    {
        await flurlClient.Request($"api-products/{apiProductId}/documents/{documentId}").DeleteAsync();
    }
}
