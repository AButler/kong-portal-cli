namespace Kong.Portal.CLI.Services.Metadata;

internal record PortalMetadata(
    string Name,
    string? CustomDomain,
    string? CustomClientDomain,
    bool IsPublic,
    bool AutoApproveDevelopers,
    bool AutoApproveApplications,
    bool RbacEnabled,
    IEnumerable<string> Products
);
