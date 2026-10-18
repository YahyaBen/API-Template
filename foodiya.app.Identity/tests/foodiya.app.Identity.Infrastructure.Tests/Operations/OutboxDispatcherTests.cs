using System.Net;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using foodiya.app.Identity.Domain.Entities;
using foodiya.app.Identity.Infrastructure.Operations;
using foodiya.app.Identity.Infrastructure.Options;
using foodiya.app.Identity.Infrastructure.Persistence;
using foodiya.app.Identity.Infrastructure.Tests.Authentication;
using MicrosoftOptions = Microsoft.Extensions.Options.Options;

namespace foodiya.app.Identity.Infrastructure.Tests.Operations;

[TestClass]
public sealed class OutboxDispatcherTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 7, 8, 0, 0, TimeSpan.Zero);
    private readonly TestContext _testContext;

    public OutboxDispatcherTests(TestContext testContext) => _testContext = testContext;

    [TestMethod]
    public async Task DispatchAsync_Success_MarksMessageProcessedAndSignsEnvelope()
    {
        string? body = null;
        string? signature = null;
        using var handler = new DelegateHandler(async request =>
        {
            body = await request.Content!.ReadAsStringAsync(_testContext.CancellationToken);
            signature = request.Headers.GetValues("X-Identity-Signature").Single();
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        });
        await using var harness = await CreateAsync(handler, _testContext.CancellationToken);
        var message = await AddMessageAsync(harness.Db, _testContext.CancellationToken);

        var delivered = await harness.Dispatcher.DispatchAsync(_testContext.CancellationToken);

        harness.Db.ChangeTracker.Clear();
        var stored = await harness.Db.OutboxMessages.SingleAsync(candidate => candidate.Id == message.Id,
            _testContext.CancellationToken);
        Assert.AreEqual(1, delivered);
        Assert.AreEqual(1, stored.Attempts);
        Assert.IsNotNull(stored.ProcessedAtUtc);
        Assert.IsNotNull(body);
        Assert.Contains("UserRegistered", body, StringComparison.Ordinal);
        Assert.StartsWith("sha256=", signature, StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task DispatchAsync_ServerFailure_SchedulesRetryWithoutDeadLettering()
    {
        using var handler = new DelegateHandler(_ =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)));
        await using var harness = await CreateAsync(handler, _testContext.CancellationToken);
        var message = await AddMessageAsync(harness.Db, _testContext.CancellationToken);

        var delivered = await harness.Dispatcher.DispatchAsync(_testContext.CancellationToken);

        harness.Db.ChangeTracker.Clear();
        var stored = await harness.Db.OutboxMessages.SingleAsync(candidate => candidate.Id == message.Id,
            _testContext.CancellationToken);
        Assert.AreEqual(0, delivered);
        Assert.AreEqual(1, stored.Attempts);
        Assert.IsNull(stored.ProcessedAtUtc);
        Assert.IsNull(stored.DeadLetteredAtUtc);
        Assert.IsTrue(stored.NextAttemptAtUtc > Now);
        Assert.IsNull(stored.LeaseId);
    }

    [TestMethod]
    public async Task DispatchAsync_RepeatedFailure_DeadLettersAtConfiguredAttemptLimit()
    {
        using var handler = new DelegateHandler(_ =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)));
        await using var harness = await CreateAsync(handler, _testContext.CancellationToken);
        var message = await AddMessageAsync(harness.Db, _testContext.CancellationToken);

        for (var attempt = 1; attempt <= 3; attempt++)
        {
            await harness.Dispatcher.DispatchAsync(_testContext.CancellationToken);
            harness.Clock.Advance(TimeSpan.FromHours(2));
        }

        harness.Db.ChangeTracker.Clear();
        var stored = await harness.Db.OutboxMessages.SingleAsync(candidate => candidate.Id == message.Id,
            _testContext.CancellationToken);
        Assert.AreEqual(3, stored.Attempts);
        Assert.IsNotNull(stored.DeadLetteredAtUtc);
        Assert.IsNull(stored.NextAttemptAtUtc);
        Assert.IsNull(stored.LeaseId);
    }

    [TestMethod]
    public async Task DispatchAsync_ExpiredLease_ReclaimsAndDeliversMessage()
    {
        using var handler = new DelegateHandler(_ =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent)));
        await using var harness = await CreateAsync(handler, _testContext.CancellationToken);
        var message = await AddMessageAsync(harness.Db, _testContext.CancellationToken);
        message.LeaseId = Guid.NewGuid();
        message.LeaseExpiresAtUtc = Now.AddMinutes(-1);
        await harness.Db.SaveChangesAsync(_testContext.CancellationToken);

        var delivered = await harness.Dispatcher.DispatchAsync(_testContext.CancellationToken);

        Assert.AreEqual(1, delivered);
        harness.Db.ChangeTracker.Clear();
        var stored = await harness.Db.OutboxMessages.SingleAsync(candidate => candidate.Id == message.Id,
            _testContext.CancellationToken);
        Assert.IsNotNull(stored.ProcessedAtUtc);
        Assert.IsNull(stored.LeaseId);
    }

    [TestMethod]
    public async Task DispatchAsync_ActiveLease_DoesNotDuplicateDelivery()
    {
        var deliveries = 0;
        using var handler = new DelegateHandler(_ =>
        {
            deliveries++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
        });
        await using var harness = await CreateAsync(handler, _testContext.CancellationToken);
        var message = await AddMessageAsync(harness.Db, _testContext.CancellationToken);
        message.LeaseId = Guid.NewGuid();
        message.LeaseExpiresAtUtc = Now.AddMinutes(1);
        await harness.Db.SaveChangesAsync(_testContext.CancellationToken);

        var delivered = await harness.Dispatcher.DispatchAsync(_testContext.CancellationToken);

        Assert.AreEqual(0, delivered);
        Assert.AreEqual(0, deliveries);
    }

    [TestMethod]
    public async Task DispatchAsync_SlowBatch_ClaimsLaterMessagesWithFreshLeases()
    {
        TestHarness? current = null;
        var requests = 0;
        using var handler = new DelegateHandler(async _ =>
        {
            Assert.IsNotNull(current);
            var claimed = await current.Db.OutboxMessages.SingleAsync(message => message.LeaseId != null,
                _testContext.CancellationToken);
            Assert.IsTrue(claimed.LeaseExpiresAtUtc > current.Clock.GetUtcNow());
            requests++;
            if (requests == 1) current.Clock.Advance(TimeSpan.FromMinutes(3));
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        });
        await using var harness = await CreateAsync(handler, _testContext.CancellationToken);
        current = harness;
        await AddMessageAsync(harness.Db, _testContext.CancellationToken);
        await AddMessageAsync(harness.Db, _testContext.CancellationToken);
        Assert.AreEqual(2, await harness.Dispatcher.DispatchAsync(_testContext.CancellationToken));
        Assert.AreEqual(2, requests);
    }

    [TestMethod]
    public async Task DispatchAsync_DatabaseFailureAfterDelivery_RetriesSameEventWithoutMarkingItProcessed()
    {
        var deliveredIds = new List<string>();
        using var handler = new DelegateHandler(request =>
        {
            deliveredIds.Add(request.Headers.GetValues("Idempotency-Key").Single());
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
        });
        await using var harness = await CreateAsync(handler, _testContext.CancellationToken, new FailFirstDeliverySave());
        var message = await AddMessageAsync(harness.Db, _testContext.CancellationToken);
        Assert.AreEqual(0, await harness.Dispatcher.DispatchAsync(_testContext.CancellationToken));
        harness.Db.ChangeTracker.Clear();
        var pending = await harness.Db.OutboxMessages.SingleAsync(_testContext.CancellationToken);
        Assert.IsNull(pending.ProcessedAtUtc);
        Assert.AreEqual(1, pending.Attempts);
        Assert.IsNotNull(pending.NextAttemptAtUtc);
        harness.Clock.Advance(TimeSpan.FromMinutes(1));
        Assert.AreEqual(1, await harness.Dispatcher.DispatchAsync(_testContext.CancellationToken));
        harness.Db.ChangeTracker.Clear();
        var completed = await harness.Db.OutboxMessages.SingleAsync(_testContext.CancellationToken);
        Assert.IsNotNull(completed.ProcessedAtUtc);
        Assert.AreEqual(2, completed.Attempts);
        Assert.HasCount(2, deliveredIds);
        Assert.AreEqual(message.Id.ToString("D"), deliveredIds[0]);
        Assert.AreEqual(deliveredIds[0], deliveredIds[1]);
    }

    private sealed class FailFirstDeliverySave : SaveChangesInterceptor
    {
        private bool _failed;
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
            InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (!_failed && eventData.Context!.ChangeTracker.Entries<OutboxMessage>()
                .Any(entry => entry.State == EntityState.Modified && entry.Entity.ProcessedAtUtc is not null))
            {
                _failed = true;
                throw new DbUpdateException("Simulated database failure after broker acceptance.");
            }
            return ValueTask.FromResult(result);
        }
    }

    private static async Task<OutboxMessage> AddMessageAsync(IdentityDataContext db, CancellationToken cancellationToken)
    {
        var message = new OutboxMessage
        {
            OccurredAtUtc = Now.AddMinutes(-1),
            Type = "UserRegistered",
            Payload = "{\"userId\":\"aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee\"}"
        };
        db.OutboxMessages.Add(message);
        await db.SaveChangesAsync(cancellationToken);
        return message;
    }

    private static async Task<TestHarness> CreateAsync(HttpMessageHandler handler, CancellationToken cancellationToken,
        SaveChangesInterceptor? interceptor = null)
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(cancellationToken);
        var services = new ServiceCollection();
        services.AddMetrics();
        services.AddDbContext<IdentityDataContext>(options =>
        {
            options.UseSqlite(connection).ReplaceService<IModelCustomizer, SqliteTestModelCustomizer>();
            if (interceptor is not null) options.AddInterceptors(interceptor);
        });
        var provider = services.BuildServiceProvider();
        var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDataContext>();
        await db.Database.EnsureCreatedAsync(cancellationToken);
        var client = new HttpClient(handler);
        var settings = MicrosoftOptions.Create(new OutboxOptions
        {
            Enabled = true,
            Endpoint = "https://events.example.test/identity",
            SigningSecret = "unit-test-outbox-signing-secret-32-characters",
            LeaseSeconds = 120,
            MaxAttempts = 3,
            BatchSize = 10
        });
        var clock = new TestTimeProvider(Now);
        var telemetry = new IdentityTelemetry(scope.ServiceProvider.GetRequiredService<System.Diagnostics.Metrics.IMeterFactory>());
        var dispatcher = new OutboxDispatcher(db, new WebhookOutboxTransport(new StaticHttpClientFactory(client), settings), settings,
            clock, telemetry, NullLogger<OutboxDispatcher>.Instance);
        return new TestHarness(connection, provider, scope, db, dispatcher, client, clock);
    }

    private sealed class DelegateHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> callback) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken) => callback(request);
    }

    private sealed class StaticHttpClientFactory(HttpClient client) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => client;
    }

    private sealed record TestHarness(SqliteConnection Connection, ServiceProvider Provider,
        AsyncServiceScope Scope, IdentityDataContext Db, OutboxDispatcher Dispatcher, HttpClient Client,
        TestTimeProvider Clock)
        : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await Scope.DisposeAsync();
            await Provider.DisposeAsync();
            await Connection.DisposeAsync();
        }
    }
}
