namespace Foodiya.Domain.Models;

public sealed class IdentityInboxMessage
{
    public Guid EventId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTime ProcessedAtUtc { get; set; }
}
