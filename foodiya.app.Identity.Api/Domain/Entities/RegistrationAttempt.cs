namespace foodiya.app.Identity.Domain.Entities;

/// <summary>Records a registration decision for throttling and security auditing.</summary>
public sealed class RegistrationAttempt
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string NormalizedEmail { get; set; }
    public required string IpAddress { get; set; }
    public DateTimeOffset AttemptedAtUtc { get; set; }
    public bool WasSuccessful { get; set; }
    public string? RejectionReason { get; set; }
    public string? UserAgent { get; set; }
}
