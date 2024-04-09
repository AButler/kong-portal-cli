namespace Kong.Portal.CLI.Services;

internal record PortalTeamsMetadata(IReadOnlyCollection<PortalTeamMetadata> Teams);

internal record PortalTeamMetadata(string Name, string Description);
