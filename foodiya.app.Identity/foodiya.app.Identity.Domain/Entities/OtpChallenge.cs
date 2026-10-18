using foodiya.app.Identity.Domain.Enums;

namespace foodiya.app.Identity.Domain.Entities;

/// <summary>Stores a hashed, limited-use OTP challenge.</summary>
public sealed class OtpChallenge
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public required string NormalizedEmail { get; set; }
    public required string CodeHash { get; set; }
    public OtpPurpose Purpose { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset ExpiresAtUtc { get; set; }
    public DateTimeOffset? ConsumedAtUtc { get; set; }
    public DateTimeOffset? InvalidatedAtUtc { get; set; }
    public int FailedAttempts { get; set; }
    public int MaxAttempts { get; set; }
    public string? DeviceInstallationIdHash { get; set; }
    public string? IpAddress { get; set; }

    public bool IsUsable(DateTimeOffset now) =>
        ConsumedAtUtc is null
        && InvalidatedAtUtc is null
        && FailedAttempts < MaxAttempts
        && now < ExpiresAtUtc;
}
