namespace foodiya.app.Identity.Domain.Entities;

/// <summary>Represents a known installation of a client application.</summary>
public sealed class UserDevice
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public required string InstallationIdHash { get; set; }
    public string? Platform { get; set; }
    public string? DeviceName { get; set; }
    public string? OsVersion { get; set; }
    public string? AppVersion { get; set; }
    public string? FirebaseInstallationId { get; set; }
    public DateTimeOffset? FirebaseInstallationUpdatedAtUtc { get; set; }
    public DateTimeOffset FirstSeenAtUtc { get; set; }
    public DateTimeOffset LastSeenAtUtc { get; set; }
    public DateTimeOffset? VerifiedAtUtc { get; set; }
    public DateTimeOffset? RevokedAtUtc { get; set; }
    public string? RevocationReason { get; set; }
    public string? LastIpAddress { get; set; }
}
