namespace Kong.Portal.CLI.Services;

internal record PortalTeamsMetadata(IReadOnlyCollection<PortalTeamMetadata> Teams);

internal record PortalTeamMetadata(string Name, string Description, IReadOnlyCollection<PortalTeamApiProduct> ApiProducts);

internal record PortalTeamApiProduct(string ApiProduct, IReadOnlyCollection<string> Roles);
