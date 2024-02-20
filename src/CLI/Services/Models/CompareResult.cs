using Kong.Portal.CLI.ApiClient.Models;

namespace Kong.Portal.CLI.Services.Models;

internal class CompareResult
{
    public CompareResult(
        IReadOnlyCollection<Difference<ApiProduct>> apiProductDifferences,
        IDictionary<string, List<Difference<ApiProductVersion>>> apiProductVersionDifferences
    )
    {
        ApiProducts = apiProductDifferences;
        ApiProductVersions = apiProductVersionDifferences
            .ToDictionary(kvp => kvp.Key, kvp => (IReadOnlyCollection<Difference<ApiProductVersion>>)kvp.Value.ToList().AsReadOnly())
            .AsReadOnly();

        AnyChanges =
            apiProductDifferences.Any(d => d.DifferenceType != DifferenceType.NoChange)
            || apiProductVersionDifferences.Values.Any(v => v.Any(d => d.DifferenceType != DifferenceType.NoChange));
    }

    public bool AnyChanges { get; }
    public IReadOnlyCollection<Difference<ApiProduct>> ApiProducts { get; }
    public IReadOnlyDictionary<string, IReadOnlyCollection<Difference<ApiProductVersion>>> ApiProductVersions { get; }
}
