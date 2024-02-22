using Kong.Portal.CLI.ApiClient.Models;

namespace Kong.Portal.CLI.Services.Models;

internal class CompareResult
{
    public CompareResult(
        List<Difference<ApiProduct>> apiProductDifferences,
        Dictionary<string, List<Difference<ApiProductVersion>>> apiProductVersionDifferences,
        Dictionary<string, Dictionary<string, Difference<ApiProductSpecification>>> apiProductVersionSpecifications,
        List<Difference<DevPortal>> portals,
        Dictionary<string, Difference<DevPortalAppearance>> portalAppearances
    )
    {
        ApiProducts = apiProductDifferences.AsReadOnly();

        ApiProductVersions = apiProductVersionDifferences
            .ToDictionary(kvp => kvp.Key, kvp => (IReadOnlyCollection<Difference<ApiProductVersion>>)kvp.Value.ToList().AsReadOnly())
            .AsReadOnly();

        ApiProductVersionSpecifications = apiProductVersionSpecifications
            .ToDictionary(
                kvp => kvp.Key,
                kvp =>
                    (IReadOnlyDictionary<string, Difference<ApiProductSpecification>>)
                        kvp.Value.ToDictionary(kvp2 => kvp2.Key, kvp2 => kvp2.Value).AsReadOnly()
            )
            .AsReadOnly();

        Portals = portals.AsReadOnly();

        PortalAppearances = portalAppearances.ToDictionary(kvp => kvp.Key, kvp => kvp.Value).AsReadOnly();

        AnyChanges =
            apiProductDifferences.Any(d => d.DifferenceType != DifferenceType.NoChange)
            || apiProductVersionDifferences.Values.Any(v => v.Any(d => d.DifferenceType != DifferenceType.NoChange))
            || apiProductVersionSpecifications.Values.Any(v => v.Values.Any(d => d.DifferenceType != DifferenceType.NoChange))
            || portals.Any(d => d.DifferenceType != DifferenceType.NoChange)
            || portalAppearances.Values.Any(d => d.DifferenceType != DifferenceType.NoChange);
    }

    public IReadOnlyCollection<string> GetValidationErrors()
    {
        var errors = new List<string>();

        if (Portals.Any(p => p.DifferenceType == DifferenceType.Add))
        {
            errors.Add("Cannot add Portals");
        }

        if (Portals.Any(p => p.DifferenceType == DifferenceType.Delete))
        {
            errors.Add("Cannot delete Portals");
        }

        return errors;
    }

    public bool AnyChanges { get; }

    public IReadOnlyCollection<Difference<ApiProduct>> ApiProducts { get; }

    public IReadOnlyDictionary<string, IReadOnlyCollection<Difference<ApiProductVersion>>> ApiProductVersions { get; }

    public IReadOnlyDictionary<string, IReadOnlyDictionary<string, Difference<ApiProductSpecification>>> ApiProductVersionSpecifications { get; }

    public IReadOnlyCollection<Difference<DevPortal>> Portals { get; }

    public IReadOnlyDictionary<string, Difference<DevPortalAppearance>> PortalAppearances { get; }
}
