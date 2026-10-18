using foodiya.app.Identity.Domain.Entities;
using foodiya.app.Identity.Infrastructure.Operations;
using foodiya.app.Identity.Infrastructure.Options;
using foodiya.app.Identity.Infrastructure.Persistence;
using foodiya.app.Identity.Integration.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using MicrosoftOptions = Microsoft.Extensions.Options.Options;

namespace foodiya.app.Identity.Integration.Tests.Operations;

[TestClass]
[TestCategory("SqlServer")]
public sealed class OutboxLeaseSqlServerTests(TestContext testContext)
{
    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    [Timeout(60000, CooperativeCancellation = true)]
    public async Task ExpiredWorker_CannotFinalizeReplacementWorkersLease(bool staleDeliveryFails)
    {
        var ct = testContext.CancellationToken;
        await using var database = await SqlServerTestDatabase.CreateAsync(ct);
        await using var first = new IdentityDataContext(database.IdentityOptions);
        await first.Database.EnsureCreatedAsync(ct);
        first.OutboxMessages.Add(new OutboxMessage { Type = "UserRegistered", Payload = "{}", OccurredAtUtc = DateTimeOffset.UtcNow });
        await first.SaveChangesAsync(ct);
        await using var second = new IdentityDataContext(database.IdentityOptions);
        await using var verification = new IdentityDataContext(database.IdentityOptions);
        var firstEntered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondEntered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseFirst = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseSecond = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var clock = new MutableClock(DateTimeOffset.UtcNow);
        var services = new ServiceCollection();
        services.AddMetrics();
        await using var provider = services.BuildServiceProvider();
        var telemetry = new IdentityTelemetry(provider.GetRequiredService<System.Diagnostics.Metrics.IMeterFactory>());
        var settings = MicrosoftOptions.Create(new OutboxOptions { LeaseSeconds = 30, TimeoutSeconds = 20, MaxAttempts = 1 });
        var oldWorker = new OutboxDispatcher(first, new PausedTransport(firstEntered, releaseFirst, staleDeliveryFails),
            settings, clock, telemetry, NullLogger<OutboxDispatcher>.Instance);
        var replacement = new OutboxDispatcher(second, new PausedTransport(secondEntered, releaseSecond, false),
            settings, clock, telemetry, NullLogger<OutboxDispatcher>.Instance);

        var oldTask = oldWorker.DispatchAsync(ct);
        await firstEntered.Task.WaitAsync(ct);
        clock.Advance(TimeSpan.FromMinutes(1));
        var newTask = replacement.DispatchAsync(ct);
        await secondEntered.Task.WaitAsync(ct);
        var owned = await verification.OutboxMessages.AsNoTracking().SingleAsync(ct);
        Assert.IsNotNull(owned.LeaseId);

        releaseFirst.SetResult();
        Assert.AreEqual(0, await oldTask);
        var afterStaleCompletion = await verification.OutboxMessages.AsNoTracking().SingleAsync(ct);
        Assert.AreEqual(owned.LeaseId, afterStaleCompletion.LeaseId);
        Assert.AreEqual(0, afterStaleCompletion.Attempts);
        Assert.IsNull(afterStaleCompletion.ProcessedAtUtc);
        Assert.IsNull(afterStaleCompletion.DeadLetteredAtUtc);
        Assert.IsNull(afterStaleCompletion.Error);

        releaseSecond.SetResult();
        Assert.AreEqual(1, await newTask);
        var complete = await verification.OutboxMessages.AsNoTracking().SingleAsync(ct);
        Assert.AreEqual(1, complete.Attempts);
        Assert.IsNotNull(complete.ProcessedAtUtc);
        Assert.IsNull(complete.LeaseId);
        Assert.IsNull(complete.DeadLetteredAtUtc);
    }

    private sealed class PausedTransport(TaskCompletionSource entered, TaskCompletionSource release, bool fail) : IOutboxTransport
    {
        public async Task DeliverAsync(OutboxMessage message, CancellationToken cancellationToken)
        {
            entered.TrySetResult();
            await release.Task.WaitAsync(cancellationToken);
            if (fail) throw new IOException("Simulated stale delivery failure.");
        }
    }

    private sealed class MutableClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
        public void Advance(TimeSpan elapsed) => now += elapsed;
    }
}
