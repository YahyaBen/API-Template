namespace foodiya.app.Identity.Domain.Entities;

/// <summary>Stores an integration event until it is delivered successfully.</summary>
public sealed class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset OccurredAtUtc { get; set; }
    public required string Type { get; set; }
    public required string Payload { get; set; }
    public DateTimeOffset? ProcessedAtUtc { get; set; }
    public string? Error { get; set; }
    public int Attempts { get; set; }
    public DateTimeOffset? NextAttemptAtUtc { get; set; }
    public Guid? LeaseId { get; set; }
    public DateTimeOffset? LeaseExpiresAtUtc { get; set; }
    public DateTimeOffset? DeadLetteredAtUtc { get; set; }
}
