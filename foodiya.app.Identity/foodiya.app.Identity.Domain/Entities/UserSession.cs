namespace foodiya.app.Identity.Domain.Entities;

/// <summary>Represents an authenticated session bound to a device.</summary>
public sealed class UserSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid DeviceId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset ExpiresAtUtc { get; set; }
    public DateTimeOffset? RevokedAtUtc { get; set; }
    public string? RevocationReason { get; set; }
    public Guid? ReplacedBySessionId { get; set; }
    public string? IpAddress { get; set; }

    public bool IsActive(DateTimeOffset now) => RevokedAtUtc is null && now < ExpiresAtUtc;
}
