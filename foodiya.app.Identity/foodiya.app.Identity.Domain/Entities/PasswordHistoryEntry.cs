namespace foodiya.app.Identity.Domain.Entities;

/// <summary>Retains a previous versioned password hash to prevent password reuse.</summary>
public sealed class PasswordHistoryEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public required string PasswordHash { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}
