namespace Kong.Portal.CLI.Services.Metadata;

internal record ApiPortalMetadata(
    string Name,
    string? CustomDomain,
    string? CustomClientDomain,
    bool IsPublic,
    bool AutoApproveDevelopers,
    bool AutoApproveApplications,
    bool RbacEnabled,
    IEnumerable<string> Products
);
