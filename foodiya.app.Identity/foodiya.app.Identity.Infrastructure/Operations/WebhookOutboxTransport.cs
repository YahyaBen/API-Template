using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using foodiya.app.Identity.Domain.Entities;
using foodiya.app.Identity.Infrastructure.Options;

namespace foodiya.app.Identity.Infrastructure.Operations;

internal sealed class WebhookOutboxTransport(IHttpClientFactory httpClientFactory,
    IOptions<OutboxOptions> options) : IOutboxTransport
{
    public async Task DeliverAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        using var payload = JsonDocument.Parse(message.Payload);
        var envelope = new { message.Id, message.Type, message.OccurredAtUtc, Payload = payload.RootElement.Clone() };
        var json = JsonSerializer.Serialize(envelope, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        using var request = new HttpRequestMessage(HttpMethod.Post, options.Value.Endpoint)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        var signature = Convert.ToHexString(HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(options.Value.SigningSecret), Encoding.UTF8.GetBytes(json))).ToLowerInvariant();
        request.Headers.TryAddWithoutValidation("Idempotency-Key", message.Id.ToString("D"));
        request.Headers.TryAddWithoutValidation("X-Identity-Signature", $"sha256={signature}");
        using var response = await httpClientFactory.CreateClient("IdentityOutbox")
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
