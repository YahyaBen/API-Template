using System.Diagnostics;
using System.Text.Json;
using Foodiya.Infrastructure.Messaging;
using Foodiya.Infrastructure.Data;
using Foodiya.Infrastructure.Identity;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using foodiya.app.Identity.Application.Interfaces;
using foodiya.app.Identity.Contracts.Registration;
using foodiya.app.Identity.Domain.Entities;
using foodiya.app.Identity.Infrastructure.Identity;
using foodiya.app.Identity.Infrastructure.Operations;
using foodiya.app.Identity.Infrastructure.Options;
using foodiya.app.Identity.Infrastructure.Persistence;
using foodiya.app.Identity.Infrastructure.Registration;
using foodiya.app.Identity.Integration.Tests.TestSupport;
using foodiya.app.Messaging;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using MicrosoftOptions = Microsoft.Extensions.Options.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace foodiya.app.Identity.Integration.Tests.Messaging;

[TestClass]
[TestCategory("RabbitMQ")]
[DoNotParallelize]
public sealed class RabbitMqIntegrationTests(TestContext testContext)
{
    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    [Timeout(60000, CooperativeCancellation = true)]
    public async Task Consumer_MissingInboxMigration_PausesWithoutDeadLetteringAndRecovers(bool missingAtStartup)
    {
        var ct = testContext.CancellationToken;
        var rabbit = GetOptions();
        var queue = $"{rabbit.Exchange}.profiles";
        await using var profiles = new ProfileTestDatabase();
        await profiles.InitializeAsync(ct);
        var services = new ServiceCollection();
        profiles.Register(services);
        services.AddScoped<IdentityProfileEventHandler>();
        await using var provider = services.BuildServiceProvider();
        var scopes = provider.GetRequiredService<IServiceScopeFactory>();
        using var consumer = new IdentityEventsConsumer(scopes, MicrosoftOptions.Create(rabbit),
            MicrosoftOptions.Create(new IdentityEventsOptions { Enabled = true, Queue = queue, MaxAttempts = 2, RetryDelaySeconds = 1 }),
            NullLogger<IdentityEventsConsumer>.Instance);
        await using var connection = await RabbitMqConnectionFactory.Create(rabbit, "Schema readiness test").CreateConnectionAsync(ct);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: ct);
        await RabbitMqTopology.DeclareAsync(channel, rabbit.Exchange, queue, 2, ct);
        await using var db = profiles.CreateContext();
        try
        {
            if (missingAtStartup) await db.Database.ExecuteSqlRawAsync("DROP TABLE IdentityInboxMessage;", ct);
            await consumer.StartAsync(ct);
            if (!missingAtStartup)
            {
                await WaitUntilAsync(() => Task.FromResult(consumer.IsConnected));
                await db.Database.ExecuteSqlRawAsync("DROP TABLE IdentityInboxMessage;", ct);
            }
            await using var publisher = new RabbitMqPublisher(MicrosoftOptions.Create(rabbit));
            await publisher.PublishAsync(IdentityProfileEventHandlerTests.Registered(Guid.NewGuid()), ct);
            await Task.Delay(TimeSpan.FromSeconds(2), ct);
            await WaitUntilAsync(async () => (await channel.QueueDeclarePassiveAsync(queue, ct)).MessageCount == 1);
            Assert.IsFalse(consumer.IsConnected);
            var health = new IdentityConsumerHealthCheck(consumer, scopes);
            Assert.AreEqual(HealthStatus.Unhealthy, (await health.CheckHealthAsync(new HealthCheckContext(), ct)).Status);
            Assert.AreEqual(0u, (await channel.QueueDeclarePassiveAsync($"{queue}.dead", ct)).MessageCount);

            await db.Database.ExecuteSqlRawAsync("CREATE TABLE IdentityInboxMessage (EventId TEXT NOT NULL PRIMARY KEY, EventType TEXT NOT NULL, ProcessedAtUtc TEXT NOT NULL);", ct);
            await WaitUntilAsync(async () => await db.IdentityInboxMessages.CountAsync(ct) == 1);
            Assert.AreEqual(1, await db.AppUsers.CountAsync(ct));
            Assert.AreEqual(HealthStatus.Healthy, (await health.CheckHealthAsync(new HealthCheckContext(), ct)).Status);
        }
        finally
        {
            await consumer.StopAsync(CancellationToken.None);
            await DeleteTopologyAsync(rabbit, queue);
        }
    }

    [TestMethod]
    [Timeout(60000, CooperativeCancellation = true)]
    public async Task RegistrationConfirmed_OutboxPublishedTwice_ProvisionsOneFoodiyaProfile()
    {
        var rabbit = GetOptions();
        var queue = $"{rabbit.Exchange}.profiles";
        await using var profiles = new ProfileTestDatabase();
        await profiles.InitializeAsync(testContext.CancellationToken);
        var services = new ServiceCollection();
        services.AddLogging();
        profiles.Register(services);
        var received = 0;
        services.AddScoped(provider =>
        {
            Interlocked.Increment(ref received);
            return new IdentityProfileEventHandler(provider.GetRequiredService<AutoGeneratedDbContext>(), provider.GetRequiredService<IdentityProfileProvisioner>());
        });
        await using var provider = services.BuildServiceProvider();
        using var consumer = new IdentityEventsConsumer(provider.GetRequiredService<IServiceScopeFactory>(),
            MicrosoftOptions.Create(rabbit), MicrosoftOptions.Create(new IdentityEventsOptions { Enabled = true, Queue = queue, RetryDelaySeconds = 1 }),
            NullLogger<IdentityEventsConsumer>.Instance);
        await consumer.StartAsync(testContext.CancellationToken);
        await using var publisher = new RabbitMqPublisher(MicrosoftOptions.Create(rabbit));
        try
        {
            await WaitUntilAsync(() => Task.FromResult(consumer.IsConnected));
            await using var identity = await IdentityTestDatabase.CreateAsync(testContext.CancellationToken);
            var email = new CaptureEmailSender();
            var registrationOptions = MicrosoftOptions.Create(new RegistrationOptions { OtpPepper = "integration-test-pepper-at-least-32-characters" });
            var registration = new RegistrationService(identity.Users, identity.Db, new HmacOtpCodeService(registrationOptions),
                email, registrationOptions, TimeProvider.System, NullLogger<RegistrationService>.Instance);
            var challenge = await registration.RegisterAsync(new RegisterRequest
            {
                UserName = "rabbit.tester", Email = "rabbit@example.test", Password = "Valid!Password123",
                FirstName = "Rabbit", LastName = "Tester"
            }, testContext.CancellationToken);
            await registration.ConfirmAsync(new ConfirmRegistrationRequest { ChallengeId = challenge.ChallengeId, Code = email.Code },
                testContext.CancellationToken);
            var message = await identity.Db.OutboxMessages.SingleAsync(e => e.Type == "UserRegistered", testContext.CancellationToken);
            var transport = new RabbitMqOutboxTransport(publisher);
            var dispatcher = identity.CreateDispatcher(transport);
            // Also dispatches UserRegistrationStarted; it must not create a profile.
            Assert.AreEqual(2, await dispatcher.DispatchAsync(testContext.CancellationToken));
            await transport.DeliverAsync(message, testContext.CancellationToken);
            await WaitUntilAsync(async () =>
            {
                await using var db = profiles.CreateContext();
                return await db.IdentityInboxMessages.CountAsync(testContext.CancellationToken) == 1
                    && Volatile.Read(ref received) == 3;
            });
            await consumer.StopAsync(testContext.CancellationToken);
            await using var verification = profiles.CreateContext();
            var profile = await verification.AppUsers.SingleAsync(testContext.CancellationToken);
            Assert.AreEqual("rabbit@example.test", profile.Email);
            Assert.AreEqual("Rabbit", profile.FirstName);
            Assert.AreEqual("rabbit.tester", profile.UserName);
            Assert.AreEqual(1, await verification.IdentityInboxMessages.CountAsync(testContext.CancellationToken));
            Assert.AreEqual(2, await identity.Db.OutboxMessages.CountAsync(e => e.ProcessedAtUtc != null, testContext.CancellationToken));
        }
        finally
        {
            await consumer.StopAsync(testContext.CancellationToken);
            await DeleteTopologyAsync(rabbit, queue);
        }
    }

    [TestMethod]
    [Timeout(60000, CooperativeCancellation = true)]
    public async Task DispatchAsync_NoQueueBinding_KeepsEventPendingForRetry()
    {
        var rabbit = GetOptions();
        await using var publisher = new RabbitMqPublisher(MicrosoftOptions.Create(rabbit));
        await using var identity = await IdentityTestDatabase.CreateAsync(testContext.CancellationToken);
        var message = new OutboxMessage { Type = "UserRegistered", OccurredAtUtc = DateTimeOffset.UtcNow, Payload = "{}" };
        identity.Db.OutboxMessages.Add(message);
        await identity.Db.SaveChangesAsync(testContext.CancellationToken);
        var dispatcher = identity.CreateDispatcher(new RabbitMqOutboxTransport(publisher));
        try
        {
            Assert.AreEqual(0, await dispatcher.DispatchAsync(testContext.CancellationToken));
            identity.Db.ChangeTracker.Clear();
            var pending = await identity.Db.OutboxMessages.SingleAsync(testContext.CancellationToken);
            Assert.IsNull(pending.ProcessedAtUtc);
            Assert.IsNotNull(pending.NextAttemptAtUtc);
            Assert.AreEqual(1, pending.Attempts);
        }
        finally { await DeleteTopologyAsync(rabbit, null); }
    }

    [TestMethod]
    [Timeout(60000, CooperativeCancellation = true)]
    public async Task PublishAsync_BrokerUnavailable_ThrowsAndDoesNotClaimDelivery()
    {
        var settings = GetOptions();
        var unavailable = new RabbitMqOptions
        {
            HostName = "127.0.0.1", Port = 1, VirtualHost = settings.VirtualHost,
            UserName = settings.UserName, Password = settings.Password,
            Exchange = settings.Exchange, PublishTimeoutSeconds = 1
        };
        await using var publisher = new RabbitMqPublisher(MicrosoftOptions.Create(unavailable));
        await Assert.ThrowsAsync<BrokerUnreachableException>(() => publisher.PublishAsync(
            IdentityProfileEventHandlerTests.Registered(Guid.NewGuid()), testContext.CancellationToken));
    }

    [TestMethod]
    [Timeout(60000, CooperativeCancellation = true)]
    public async Task Consumer_InvalidSchema_TransfersMessageToDeadLetterQueue()
    {
        var rabbit = GetOptions();
        var queue = $"{rabbit.Exchange}.profiles";
        await using var profiles = new ProfileTestDatabase();
        await profiles.InitializeAsync(testContext.CancellationToken);
        var services = new ServiceCollection();
        profiles.Register(services);
        services.AddScoped<IdentityProfileEventHandler>(_ => throw new InvalidOperationException("Invalid schemas must be rejected before calling the handler."));
        await using var provider = services.BuildServiceProvider();
        using var consumer = new IdentityEventsConsumer(provider.GetRequiredService<IServiceScopeFactory>(),
            MicrosoftOptions.Create(rabbit), MicrosoftOptions.Create(new IdentityEventsOptions { Enabled = true, Queue = queue, MaxAttempts = 2, RetryDelaySeconds = 1 }),
            NullLogger<IdentityEventsConsumer>.Instance);
        await using var connection = await RabbitMqConnectionFactory.Create(rabbit, "Dead-letter integration test").CreateConnectionAsync(testContext.CancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: testContext.CancellationToken);
        await using var publisher = new RabbitMqPublisher(MicrosoftOptions.Create(rabbit));
        try
        {
            await consumer.StartAsync(testContext.CancellationToken);
            await WaitUntilAsync(() => Task.FromResult(consumer.IsConnected));
            // A malformed JSON body is permanently rejected, without a database scope.
            await channel.BasicPublishAsync(rabbit.Exchange, "identity.user.registered.v1", mandatory: true,
                basicProperties: new BasicProperties { Persistent = true, MessageId = "invalid-schema" },
                body: "invalid-json"u8.ToArray(), cancellationToken: testContext.CancellationToken);
            BasicGetResult? failed = null;
            await WaitUntilAsync(async () =>
            {
                failed = await channel.BasicGetAsync($"{queue}.dead", autoAck: true, testContext.CancellationToken);
                return failed is not null;
            });
            Assert.IsNotNull(failed);
            Assert.AreEqual("invalid-schema", failed.BasicProperties.MessageId);
        }
        finally
        {
            await consumer.StopAsync(testContext.CancellationToken);
            await DeleteTopologyAsync(rabbit, queue);
        }
    }

    [TestMethod]
    [Timeout(60000, CooperativeCancellation = true)]
    public async Task Consumer_TransientHandlerFailure_StopsRetryingAtAttemptLimit()
    {
        var rabbit = GetOptions();
        var queue = $"{rabbit.Exchange}.profiles";
        var attempts = 0;
        await using var profiles = new ProfileTestDatabase();
        await profiles.InitializeAsync(testContext.CancellationToken);
        var services = new ServiceCollection();
        profiles.Register(services);
        services.AddScoped<IdentityProfileEventHandler>(_ =>
        {
            Interlocked.Increment(ref attempts);
            throw new InvalidOperationException("Simulated transient database failure.");
        });
        await using var provider = services.BuildServiceProvider();
        using var consumer = new IdentityEventsConsumer(provider.GetRequiredService<IServiceScopeFactory>(),
            MicrosoftOptions.Create(rabbit), MicrosoftOptions.Create(new IdentityEventsOptions { Enabled = true, Queue = queue, MaxAttempts = 2, RetryDelaySeconds = 1 }),
            NullLogger<IdentityEventsConsumer>.Instance);
        await using var publisher = new RabbitMqPublisher(MicrosoftOptions.Create(rabbit));
        await using var connection = await RabbitMqConnectionFactory.Create(rabbit, "Retry limit integration test").CreateConnectionAsync(testContext.CancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: testContext.CancellationToken);
        try
        {
            await consumer.StartAsync(testContext.CancellationToken);
            await WaitUntilAsync(() => Task.FromResult(consumer.IsConnected));
            var envelope = IdentityProfileEventHandlerTests.Registered(Guid.NewGuid());
            await publisher.PublishAsync(envelope, testContext.CancellationToken);
            BasicGetResult? failed = null;
            await WaitUntilAsync(async () =>
            {
                failed = await channel.BasicGetAsync($"{queue}.dead", autoAck: true, testContext.CancellationToken);
                return failed is not null;
            });
            Assert.IsNotNull(failed);
            Assert.AreEqual(envelope.Id.ToString("D"), failed.BasicProperties.MessageId);
            Assert.AreEqual(2, attempts);
        }
        finally
        {
            await consumer.StopAsync(testContext.CancellationToken);
            await DeleteTopologyAsync(rabbit, queue);
        }
    }

    [TestMethod]
    [Timeout(120000, CooperativeCancellation = true)]
    public async Task PublishAsync_BrokerRestarts_PersistentMessageSurvivesAndPublisherReconnects()
    {
        var rabbit = GetOptions();
        if (Environment.GetEnvironmentVariable("RABBITMQ_RESTART_TESTS") != "1")
            Assert.Inconclusive("Set RABBITMQ_RESTART_TESTS=1 only for the dedicated local Compose broker.");
        var queue = $"{rabbit.Exchange}.profiles";
        await using var publisher = new RabbitMqPublisher(MicrosoftOptions.Create(rabbit));
        await using (var connection = await RabbitMqConnectionFactory.Create(rabbit, "Restart topology test").CreateConnectionAsync(testContext.CancellationToken))
        await using (var channel = await connection.CreateChannelAsync(cancellationToken: testContext.CancellationToken))
            await RabbitMqTopology.DeclareAsync(channel, rabbit.Exchange, queue, 5, testContext.CancellationToken);
        var first = IdentityProfileEventHandlerTests.Registered(Guid.NewGuid());
        await publisher.PublishAsync(first, testContext.CancellationToken);
        try
        {
            var directory = new DirectoryInfo(Environment.CurrentDirectory);
            while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, "foodiya.app.RabbitMq"))) directory = directory.Parent;
            Assert.IsNotNull(directory);
            var start = new ProcessStartInfo("docker") { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
            foreach (var argument in new[] { "compose", "--env-file", Path.Combine(directory.FullName, "foodiya.app.RabbitMq", ".env"),
                "-f", Path.Combine(directory.FullName, "foodiya.app.RabbitMq", "docker-compose.yml"), "restart", "rabbitmq" }) start.ArgumentList.Add(argument);
            using var process = Process.Start(start);
            Assert.IsNotNull(process);
            await process.WaitForExitAsync(testContext.CancellationToken);
            Assert.AreEqual(0, process.ExitCode);
            await WaitUntilAsync(async () =>
            {
                try
                {
                    await using var probe = await RabbitMqConnectionFactory.Create(rabbit, "Restart readiness probe").CreateConnectionAsync(testContext.CancellationToken);
                    return probe.IsOpen;
                }
                catch (BrokerUnreachableException) { return false; }
            });
            // Reuse the existing publisher after its connection was interrupted.
            var second = IdentityProfileEventHandlerTests.Registered(Guid.NewGuid());
            await publisher.PublishAsync(second, testContext.CancellationToken);
            await using var connection = await RabbitMqConnectionFactory.Create(rabbit, "Restart verification").CreateConnectionAsync(testContext.CancellationToken);
            await using var channel = await connection.CreateChannelAsync(cancellationToken: testContext.CancellationToken);
            var persisted = await channel.BasicGetAsync(queue, autoAck: true, testContext.CancellationToken);
            var replacement = await channel.BasicGetAsync(queue, autoAck: true, testContext.CancellationToken);
            Assert.IsNotNull(persisted);
            Assert.IsNotNull(replacement);
            Assert.AreEqual(first.Id.ToString("D"), persisted.BasicProperties.MessageId);
            Assert.AreEqual(second.Id.ToString("D"), replacement.BasicProperties.MessageId);
        }
        finally { await DeleteTopologyAsync(rabbit, queue); }
    }

    private RabbitMqOptions GetOptions()
    {
        if (Environment.GetEnvironmentVariable("RABBITMQ_TESTS") != "1")
            Assert.Inconclusive("Set RABBITMQ_TESTS=1 and RabbitMq__ credentials to run live broker tests.");
        var settings = new ConfigurationBuilder().AddEnvironmentVariables().Build().GetSection("RabbitMq").Get<RabbitMqOptions>();
        Assert.IsNotNull(settings);
        return new RabbitMqOptions
        {
            HostName = settings.HostName, Port = settings.Port, VirtualHost = settings.VirtualHost,
            UserName = settings.UserName, Password = settings.Password,
            Exchange = $"foodiya.tests.{Guid.NewGuid():N}", PublishTimeoutSeconds = 5
        };
    }

    private async Task WaitUntilAsync(Func<Task<bool>> predicate)
    {
        for (var attempt = 0; attempt < 300; attempt++)
        {
            testContext.CancellationToken.ThrowIfCancellationRequested();
            if (await predicate()) return;
            await Task.Delay(100, testContext.CancellationToken);
        }
        Assert.Fail("The expected broker or database state was not reached.");
    }

    private async Task DeleteTopologyAsync(RabbitMqOptions rabbit, string? queue)
    {
        await using var connection = await RabbitMqConnectionFactory.Create(rabbit, "Integration test cleanup").CreateConnectionAsync(testContext.CancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: testContext.CancellationToken);
        if (queue is not null)
        {
            await channel.QueueDeleteAsync(queue, cancellationToken: testContext.CancellationToken);
            await channel.QueueDeleteAsync($"{queue}.dead", cancellationToken: testContext.CancellationToken);
            await channel.ExchangeDeleteAsync($"{rabbit.Exchange}.dead", cancellationToken: testContext.CancellationToken);
        }
        await channel.ExchangeDeleteAsync(rabbit.Exchange, cancellationToken: testContext.CancellationToken);
    }

    private sealed class CaptureEmailSender : IIdentityEmailSender
    {
        public string Code { get; private set; } = string.Empty;
        public Task SendRegistrationOtpAsync(string email, IdentityEmailContext context, Guid challengeId, string code, DateTimeOffset expiresAtUtc, CancellationToken cancellationToken)
        { Code = code; return Task.CompletedTask; }
        public Task SendAccountOtpAsync(string email, IdentityEmailContext context, foodiya.app.Identity.Domain.Enums.OtpPurpose purpose, Guid challengeId, string code, DateTimeOffset expiresAtUtc, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task SendWelcomeAsync(string email, IdentityEmailContext context, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task SendAccountDeletionConfirmationAsync(string email, IdentityEmailContext context, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class IdentityTestDatabase(SqliteConnection connection, ServiceProvider provider, AsyncServiceScope scope,
        IdentityDataContext db, UserManager<ApplicationUser> users) : IAsyncDisposable
    {
        public IdentityDataContext Db { get; } = db;
        public UserManager<ApplicationUser> Users { get; } = users;
        public static async Task<IdentityTestDatabase> CreateAsync(CancellationToken cancellationToken)
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync(cancellationToken);
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddMetrics();
            services.AddDbContext<IdentityDataContext>(options => options.UseSqlite(connection).ReplaceService<IModelCustomizer, SqliteTestModelCustomizer>());
            services.AddIdentityCore<ApplicationUser>().AddRoles<IdentityRole<Guid>>().AddEntityFrameworkStores<IdentityDataContext>();
            var provider = services.BuildServiceProvider();
            var scope = provider.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<IdentityDataContext>();
            await db.Database.EnsureCreatedAsync(cancellationToken);
            return new IdentityTestDatabase(connection, provider, scope, db, scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>());
        }
        public OutboxDispatcher CreateDispatcher(IOutboxTransport transport) => new(Db, transport,
            MicrosoftOptions.Create(new OutboxOptions { Enabled = true, Transport = "RabbitMq", TimeoutSeconds = 10 }), TimeProvider.System,
            new IdentityTelemetry(provider.GetRequiredService<System.Diagnostics.Metrics.IMeterFactory>()), NullLogger<OutboxDispatcher>.Instance);
        public async ValueTask DisposeAsync()
        { await scope.DisposeAsync(); await provider.DisposeAsync(); await connection.DisposeAsync(); }
    }
}
