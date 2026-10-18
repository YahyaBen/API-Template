namespace foodiya.app.Identity.Contracts.Roles;

/// <summary>An account and its current role assignments.</summary>
public sealed record UserRolesResponse(
    Guid UserId,
    string Email,
    IReadOnlyCollection<string> Roles,
    DateTimeOffset UpdatedAtUtc);
