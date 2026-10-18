namespace foodiya.app.Identity.Contracts.Users;

/// <summary>The authenticated account and current device-session information.</summary>
public sealed record CurrentIdentityResponse(
    Guid UserId,
    string UserName,
    string Email,
    bool EmailConfirmed,
    string FirstName,
    string LastName,
    string PreferredLanguage,
    string AccountStatus,
    IReadOnlyCollection<string> Roles,
    Guid SessionId,
    DateTimeOffset SessionCreatedAtUtc,
    DateTimeOffset SessionExpiresAtUtc,
    Guid DeviceId,
    string? Platform,
    string? DeviceName,
    string? OsVersion,
    string? AppVersion);
