namespace foodiya.app.Identity.Contracts.Roles;

/// <summary>Roles supported by this identity service.</summary>
public sealed record RoleCatalogResponse(IReadOnlyCollection<string> Roles);
