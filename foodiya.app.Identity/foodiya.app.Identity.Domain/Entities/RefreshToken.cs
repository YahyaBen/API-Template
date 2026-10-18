namespace foodiya.app.Identity.Domain.Entities;

/// <summary>Represents a hashed, rotating refresh token bound to a user session.</summary>
public sealed class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid SessionId { get; set; }
    public required string TokenHash { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset ExpiresAtUtc { get; set; }
    public DateTimeOffset? RevokedAtUtc { get; set; }
    public string? ReplacedByTokenHash { get; set; }
    public string? CreatedByIp { get; set; }
    public string? RevokedByIp { get; set; }
    public string? RevocationReason { get; set; }

    public bool IsActive(DateTimeOffset now) => RevokedAtUtc is null && now < ExpiresAtUtc;
}
