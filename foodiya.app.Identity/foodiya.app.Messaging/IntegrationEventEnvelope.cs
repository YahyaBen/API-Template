using System.Text.Json;

namespace foodiya.app.Messaging;

public sealed record IntegrationEventEnvelope(
    Guid Id,
    string Type,
    DateTimeOffset OccurredAtUtc,
    JsonElement Payload,
    int SchemaVersion = 1,
    string Source = "foodiya.app.Identity")
{
    public static JsonSerializerOptions JsonOptions { get; } = new(JsonSerializerDefaults.Web);
}

public sealed record UserRegisteredEventPayload(
    Guid Id,
    string Email,
    string? UserName = null,
    string? FirstName = null,
    string? LastName = null);
