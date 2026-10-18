using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace foodiya.app.Messaging;

public sealed class RabbitMqPublisher(IOptions<RabbitMqOptions> options) : IAsyncDisposable
{
    public static readonly ActivitySource Activities = new("foodiya.app.Messaging");
    private readonly SemaphoreSlim _gate = new(1, 1);
    private IConnection? _connection;
    private IChannel? _channel;
    private bool _disposed;

    public async Task PublishAsync(IntegrationEventEnvelope envelope, CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var settings = options.Value;
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(TimeSpan.FromSeconds(settings.PublishTimeoutSeconds));
        await _gate.WaitAsync(deadline.Token);
        try
        {
            using var activity = Activities.StartActivity("rabbitmq.publish", ActivityKind.Producer);
            activity?.SetTag("messaging.system", "rabbitmq");
            activity?.SetTag("messaging.destination.name", settings.Exchange);
            if (_channel?.IsOpen != true)
            {
                await ResetAsync();
                _connection = await RabbitMqConnectionFactory.Create(settings, "Identity outbox publisher")
                    .CreateConnectionAsync(deadline.Token);
                _channel = await _connection.CreateChannelAsync(new CreateChannelOptions(
                    publisherConfirmationsEnabled: true,
                    publisherConfirmationTrackingEnabled: true), deadline.Token);
                await _channel.ExchangeDeclareAsync(settings.Exchange, ExchangeType.Topic,
                    durable: true, autoDelete: false, cancellationToken: deadline.Token);
            }

            var properties = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json",
                MessageId = envelope.Id.ToString("D"),
                Type = envelope.Type
            };
            if (activity is not null)
            {
                properties.Headers = new Dictionary<string, object?>
                {
                    ["traceparent"] = activity.Id
                };
            }
            // Confirmation tracking makes this await fail on a broker nack or
            // an unroutable mandatory return, as well as a connection failure.
            await _channel.BasicPublishAsync(settings.Exchange, IdentityEventRouting.GetRoutingKey(envelope.Type),
                mandatory: true, basicProperties: properties,
                body: JsonSerializer.SerializeToUtf8Bytes(envelope, IntegrationEventEnvelope.JsonOptions),
                cancellationToken: deadline.Token);
        }
        catch (PublishException) when (_channel?.IsOpen == true)
        {
            // A mandatory return or broker nack does not invalidate the channel.
            // Keep the long-lived connection while the outbox schedules a retry.
            throw;
        }
        catch
        {
            // Delivery is uncertain: the outbox retries with the same event ID.
            await ResetAsync();
            throw;
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task ResetAsync()
    {
        if (_channel is not null)
        {
            await _channel.DisposeAsync();
            _channel = null;
        }
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _gate.WaitAsync();
        try
        {
            _disposed = true;
            await ResetAsync();
        }
        finally
        {
            _gate.Release();
        }
        _gate.Dispose();
    }
}
