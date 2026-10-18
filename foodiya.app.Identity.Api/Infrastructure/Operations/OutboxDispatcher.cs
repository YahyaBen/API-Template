using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using foodiya.app.Identity.Domain.Entities;
using foodiya.app.Identity.Infrastructure.Options;
using foodiya.app.Identity.Infrastructure.Persistence;

namespace foodiya.app.Identity.Infrastructure.Operations;

internal sealed class OutboxDispatcher(
    IdentityDataContext db,
    IOutboxTransport transport,
    IOptions<OutboxOptions> options,
    TimeProvider clock,
    IdentityTelemetry telemetry,
    ILogger<OutboxDispatcher> logger)
{
    public async Task<int> DispatchAsync(CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var now = clock.GetUtcNow();
        var candidateIds = await db.OutboxMessages
            .Where(message => message.ProcessedAtUtc == null
                && message.DeadLetteredAtUtc == null
                && (message.NextAttemptAtUtc == null || message.NextAttemptAtUtc <= now)
                && (message.LeaseExpiresAtUtc == null || message.LeaseExpiresAtUtc < now))
            .OrderBy(message => message.OccurredAtUtc).ThenBy(message => message.Id)
            .Select(message => message.Id).Take(settings.BatchSize).ToArrayAsync(cancellationToken);

        var delivered = 0;
        foreach (var id in candidateIds)
        {
            // Claim only the event about to be delivered. Later events in a
            // slow batch must not sit idle while their leases expire.
            now = clock.GetUtcNow();
            var leaseId = Guid.NewGuid();
            var leaseUntil = now.AddSeconds(settings.LeaseSeconds);
            var claimed = await db.OutboxMessages
                .Where(message => message.Id == id
                    && message.ProcessedAtUtc == null && message.DeadLetteredAtUtc == null
                    && (message.NextAttemptAtUtc == null || message.NextAttemptAtUtc <= now)
                    && (message.LeaseExpiresAtUtc == null || message.LeaseExpiresAtUtc < now))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(message => message.LeaseId, leaseId)
                    .SetProperty(message => message.LeaseExpiresAtUtc, leaseUntil), cancellationToken);
            if (claimed == 0) continue;
            db.ChangeTracker.Clear();
            var message = await db.OutboxMessages.SingleAsync(
                candidate => candidate.Id == id && candidate.LeaseId == leaseId, cancellationToken);
            if (await DeliverAsync(message, settings, cancellationToken))
            {
                delivered++;
            }
        }

        return delivered;
    }

    private async Task<bool> DeliverAsync(
        OutboxMessage message,
        OutboxOptions settings,
        CancellationToken cancellationToken)
    {
        using var activity = IdentityTelemetry.Activities.StartActivity("identity.outbox.dispatch");
        message.Attempts++;
        try
        {
            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            deadline.CancelAfter(TimeSpan.FromSeconds(settings.TimeoutSeconds));
            await transport.DeliverAsync(message, deadline.Token);
            message.ProcessedAtUtc = clock.GetUtcNow();
            message.NextAttemptAtUtc = null;
            message.LeaseId = null;
            message.LeaseExpiresAtUtc = null;
            message.Error = null;
            await db.SaveChangesAsync(cancellationToken);
            telemetry.Record("outbox", "delivered");
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (DbUpdateConcurrencyException)
        {
            LeaseLost(message.Id);
            return false;
        }
        catch (Exception exception)
        {
            message.ProcessedAtUtc = null;
            message.LeaseId = null;
            message.LeaseExpiresAtUtc = null;
            message.Error = $"{exception.GetType().Name}: {exception.Message}"[..Math.Min(1000, exception.GetType().Name.Length + exception.Message.Length + 2)];
            if (message.Attempts >= settings.MaxAttempts)
            {
                message.DeadLetteredAtUtc = clock.GetUtcNow();
                message.NextAttemptAtUtc = null;
            }
            else
            {
                var delaySeconds = Math.Min(3600, 5 * Math.Pow(2, message.Attempts - 1));
                message.NextAttemptAtUtc = clock.GetUtcNow().AddSeconds(delaySeconds);
            }

            try { await db.SaveChangesAsync(cancellationToken); }
            catch (DbUpdateConcurrencyException)
            {
                LeaseLost(message.Id);
                return false;
            }
            logger.LogWarning(exception, "Outbox delivery failed for event {EventId} ({EventType}); attempt {Attempt}.",
                message.Id, message.Type, message.Attempts);
            telemetry.Record("outbox", message.DeadLetteredAtUtc is null ? "retry" : "dead_letter");
            return false;
        }
    }

    private void LeaseLost(Guid eventId)
    {
        db.ChangeTracker.Clear();
        logger.LogInformation("Outbox event {EventId} now belongs to another dispatcher; ignoring stale completion.", eventId);
    }
}
