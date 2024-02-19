using Kong.Portal.CLI.ApiClient.Models;

namespace Kong.Portal.CLI.Services.Models;

internal class CompareResult
{
    public CompareResult(IReadOnlyCollection<Difference<ApiProduct>> apiProductDifferences)
    {
        ApiProducts = apiProductDifferences;

        AnyChanges = apiProductDifferences.Any(d => d.DifferenceType != DifferenceType.NoChange);
    }

    public bool AnyChanges { get; }

    public IReadOnlyCollection<Difference<ApiProduct>> ApiProducts { get; }
}
