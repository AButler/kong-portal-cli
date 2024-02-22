namespace Kong.Portal.CLI.ApiClient.Models;

internal record DevPortalUpdate(
    string? CustomDomain,
    string? CustomClientDomain,
    bool IsPublic,
    bool AutoApproveDevelopers,
    bool AutoApproveApplications,
    bool RbacEnabled
);
