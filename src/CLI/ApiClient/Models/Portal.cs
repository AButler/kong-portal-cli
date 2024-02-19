namespace Kong.Portal.CLI.ApiClient.Models;

internal record Portal(
    string Id,
    string Name,
    string? CustomDomain,
    string? CustomClientDomain,
    bool IsPublic,
    bool AutoApproveDevelopers,
    bool AutoApproveApplications,
    bool RbacEnabled
) : IEntity;
