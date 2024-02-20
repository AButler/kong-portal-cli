using Kong.Portal.CLI.ApiClient.Models;

namespace Kong.Portal.CLI;

internal static class ApiModelExtensions
{
    public static ApiProductUpdate ToUpdateModel(this ApiProduct apiProduct) => new(apiProduct.Name, apiProduct.Description, apiProduct.Labels);
}
