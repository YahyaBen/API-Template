using RabbitMQ.Client;

namespace foodiya.app.Messaging;

public static class RabbitMqTopology
{
    public static async Task DeclareAsync(IChannel channel, string exchange, string queue,
        int maxAttempts, CancellationToken cancellationToken)
    {
        var deadLetterExchange = $"{exchange}.dead";
        var deadLetterQueue = $"{queue}.dead";
        await channel.ExchangeDeclareAsync(exchange, ExchangeType.Topic, durable: true,
            autoDelete: false, cancellationToken: cancellationToken);
        await channel.ExchangeDeclareAsync(deadLetterExchange, ExchangeType.Direct, durable: true,
            autoDelete: false, cancellationToken: cancellationToken);
        await channel.QueueDeclareAsync(deadLetterQueue, durable: true, exclusive: false, autoDelete: false,
            arguments: new Dictionary<string, object?> { ["x-queue-type"] = "quorum" },
            cancellationToken: cancellationToken);
        await channel.QueueBindAsync(deadLetterQueue, deadLetterExchange, queue, cancellationToken: cancellationToken);
        await channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-queue-type"] = "quorum",
                ["x-delivery-limit"] = maxAttempts,
                ["x-dead-letter-exchange"] = deadLetterExchange,
                ["x-dead-letter-routing-key"] = queue,
                ["x-dead-letter-strategy"] = "at-least-once",
                ["x-overflow"] = "reject-publish",
                ["x-max-length"] = 100000
            }, cancellationToken: cancellationToken);
        // This subscriber receives all Identity events. Its first handler
        // provisions profiles on UserRegistered; other types are acknowledged.
        await channel.QueueBindAsync(queue, exchange, "identity.#", cancellationToken: cancellationToken);
    }
}
