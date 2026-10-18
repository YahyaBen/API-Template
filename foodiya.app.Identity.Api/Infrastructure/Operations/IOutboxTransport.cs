using foodiya.app.Identity.Domain.Entities;

namespace foodiya.app.Identity.Infrastructure.Operations;

internal interface IOutboxTransport
{
    Task DeliverAsync(OutboxMessage message, CancellationToken cancellationToken);
}
