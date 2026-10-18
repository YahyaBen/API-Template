using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using foodiya.app.Identity.Domain.Entities;
using foodiya.app.Identity.Domain.Enums;
using foodiya.app.Identity.Infrastructure.Identity;
using foodiya.app.Identity.Infrastructure.Operations;
using foodiya.app.Identity.Infrastructure.Options;
using foodiya.app.Identity.Infrastructure.Persistence;
using foodiya.app.Identity.Infrastructure.Tests.Authentication;
using MicrosoftOptions = Microsoft.Extensions.Options.Options;

namespace foodiya.app.Identity.Infrastructure.Tests.Operations;

[TestClass]
public sealed class IdentityCleanupServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 7, 8, 0, 0, TimeSpan.Zero);
    private readonly TestContext _testContext;

    public IdentityCleanupServiceTests(TestContext testContext) => _testContext = testContext;

    [TestMethod]
    public async Task RunAsync_ExpiredRecords_DeletesBoundedOperationalHistory()
    {
        await using var harness = await CreateAsync();
        var user = CreateUser(AccountStatus.Active);
        var device = new UserDevice
        {
            UserId = user.Id,
            InstallationIdHash = new string('d', 64),
            FirstSeenAtUtc = Now.AddDays(-100),
            LastSeenAtUtc = Now.AddDays(-100)
        };
        var session = new UserSession
        {
            UserId = user.Id,
            DeviceId = device.Id,
            CreatedAtUtc = Now.AddDays(-100),
            ExpiresAtUtc = Now.AddDays(-60)
        };
        harness.Db.AddRange(user, device, session,
            new RefreshToken
            {
                UserId = user.Id,
                SessionId = session.Id,
                TokenHash = new string('t', 64),
                CreatedAtUtc = Now.AddDays(-100),
                ExpiresAtUtc = Now.AddDays(-60)
            },
            new OtpChallenge
            {
                UserId = user.Id,
                NormalizedEmail = user.NormalizedEmail!,
                CodeHash = new string('c', 64),
                Purpose = OtpPurpose.PasswordReset,
                CreatedAtUtc = Now.AddDays(-20),
                ExpiresAtUtc = Now.AddDays(-15),
                MaxAttempts = 5
            },
            new RegistrationAttempt
            {
                NormalizedEmail = user.NormalizedEmail!,
                IpAddress = "127.0.0.1",
                AttemptedAtUtc = Now.AddDays(-60)
            },
            new OutboxMessage
            {
                Type = "DeliveredEvent",
                Payload = "{}",
                OccurredAtUtc = Now.AddDays(-60),
                ProcessedAtUtc = Now.AddDays(-60)
            });
        await harness.Db.SaveChangesAsync(_testContext.CancellationToken);

        var result = await harness.Service.RunAsync(_testContext.CancellationToken);

        Assert.AreEqual(1, result.Sessions);
        Assert.AreEqual(1, result.Otps);
        Assert.AreEqual(1, result.Events);
        Assert.AreEqual(1, result.RegistrationAttempts);
        Assert.AreEqual(0, await harness.Db.UserSessions.CountAsync(_testContext.CancellationToken));
        Assert.AreEqual(0, await harness.Db.RefreshTokens.CountAsync(_testContext.CancellationToken));
        Assert.AreEqual(0, await harness.Db.OtpChallenges.CountAsync(_testContext.CancellationToken));
    }

    [TestMethod]
    public async Task RunAsync_RecentAndPendingRecords_PreservesReuseDetectionAndUndeliveredEvents()
    {
        await using var harness = await CreateAsync();
        var user = CreateUser(AccountStatus.Active);
        var device = new UserDevice
        {
            UserId = user.Id,
            InstallationIdHash = new string('d', 64),
            FirstSeenAtUtc = Now.AddDays(-2),
            LastSeenAtUtc = Now.AddDays(-1)
        };
        var session = new UserSession
        {
            UserId = user.Id,
            DeviceId = device.Id,
            CreatedAtUtc = Now.AddDays(-2),
            ExpiresAtUtc = Now.AddDays(1)
        };
        harness.Db.AddRange(user, device, session,
            new RefreshToken
            {
                UserId = user.Id,
                SessionId = session.Id,
                TokenHash = new string('t', 64),
                CreatedAtUtc = Now.AddDays(-2),
                ExpiresAtUtc = Now.AddDays(1),
                RevokedAtUtc = Now.AddDays(-1)
            },
            new OutboxMessage
            {
                Type = "PendingEvent",
                Payload = "{}",
                OccurredAtUtc = Now.AddDays(-60)
            });
        await harness.Db.SaveChangesAsync(_testContext.CancellationToken);

        await harness.Service.RunAsync(_testContext.CancellationToken);

        Assert.AreEqual(1, await harness.Db.UserSessions.CountAsync(_testContext.CancellationToken));
        Assert.AreEqual(1, await harness.Db.RefreshTokens.CountAsync(_testContext.CancellationToken));
        Assert.AreEqual(1, await harness.Db.OutboxMessages.CountAsync(_testContext.CancellationToken));
    }

    [TestMethod]
    public async Task RunAsync_OldDeletedAccount_PurgesTombstone()
    {
        await using var harness = await CreateAsync();
        var user = CreateUser(AccountStatus.Deleted);
        user.DeletedAtUtc = Now.AddDays(-120);
        harness.Db.Users.Add(user);
        await harness.Db.SaveChangesAsync(_testContext.CancellationToken);

        var result = await harness.Service.RunAsync(_testContext.CancellationToken);

        Assert.AreEqual(1, result.DeletedAccounts);
        Assert.AreEqual(0, await harness.Db.Users.CountAsync(_testContext.CancellationToken));
    }

    private static ApplicationUser CreateUser(AccountStatus status) => new()
    {
        Id = Guid.NewGuid(),
        UserName = $"user-{Guid.NewGuid():N}",
        NormalizedUserName = $"USER-{Guid.NewGuid():N}",
        Email = $"{Guid.NewGuid():N}@example.test",
        NormalizedEmail = $"{Guid.NewGuid():N}@EXAMPLE.TEST",
        FirstName = "Test",
        LastName = "User",
        Status = status,
        CreatedAtUtc = Now.AddDays(-150),
        SecurityStamp = Guid.NewGuid().ToString("N"),
        ConcurrencyStamp = Guid.NewGuid().ToString("N")
    };

    private static async Task<TestHarness> CreateAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var services = new ServiceCollection();
        services.AddMetrics();
        services.AddDbContext<IdentityDataContext>(options => options.UseSqlite(connection)
            .ReplaceService<IModelCustomizer, SqliteTestModelCustomizer>());
        var provider = services.BuildServiceProvider();
        var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDataContext>();
        await db.Database.EnsureCreatedAsync();
        var telemetry = new IdentityTelemetry(scope.ServiceProvider
            .GetRequiredService<System.Diagnostics.Metrics.IMeterFactory>());
        var service = new IdentityCleanupService(db, MicrosoftOptions.Create(new CleanupOptions
        {
            BatchSize = 100,
            SessionRetentionDays = 30,
            OtpRetentionDays = 7,
            ProcessedEventRetentionDays = 30,
            RegistrationAttemptRetentionDays = 30,
            DeletedAccountRetentionDays = 90
        }), new TestTimeProvider(Now), NullLogger<IdentityCleanupService>.Instance, telemetry);
        return new TestHarness(connection, provider, scope, db, service);
    }

    private sealed record TestHarness(SqliteConnection Connection, ServiceProvider Provider,
        AsyncServiceScope Scope, IdentityDataContext Db, IdentityCleanupService Service) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            await Scope.DisposeAsync();
            await Provider.DisposeAsync();
            await Connection.DisposeAsync();
        }
    }
}
