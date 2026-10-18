using System.Text.Json;
using foodiya.app.Identity.Domain.Entities;
using foodiya.app.Messaging;

namespace foodiya.app.Identity.Infrastructure.Operations;

internal sealed class RabbitMqOutboxTransport(RabbitMqPublisher publisher) : IOutboxTransport
{
    public async Task DeliverAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        using var payload = JsonDocument.Parse(message.Payload);
        await publisher.PublishAsync(new IntegrationEventEnvelope(message.Id, message.Type,
            message.OccurredAtUtc, payload.RootElement.Clone()), cancellationToken);
    }
}
