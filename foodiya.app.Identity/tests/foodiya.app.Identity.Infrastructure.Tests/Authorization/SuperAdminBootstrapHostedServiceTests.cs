using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using foodiya.app.Identity.Domain.Constants;
using foodiya.app.Identity.Infrastructure.Authorization;
using foodiya.app.Identity.Infrastructure.Identity;
using foodiya.app.Identity.Infrastructure.Options;
using foodiya.app.Identity.Infrastructure.Persistence;
using foodiya.app.Identity.Infrastructure.Tests.Authentication;

namespace foodiya.app.Identity.Infrastructure.Tests.Authorization;

[TestClass]
public sealed class SuperAdminBootstrapHostedServiceTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 9, 4, 14, 0, 0, TimeSpan.Zero);

    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _serviceProvider;
    private readonly TestContext _testContext;

    public SuperAdminBootstrapHostedServiceTests(TestContext testContext)
    {
        _testContext = testContext;
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<IdentityDataContext>(options => options
            .UseSqlite(_connection)
            .ReplaceService<IModelCustomizer, SqliteTestModelCustomizer>());
        services.AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<IdentityDataContext>();
        _serviceProvider = services.BuildServiceProvider();
    }

    [TestInitialize]
    public async Task InitializeAsync()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var dataContext = scope.ServiceProvider.GetRequiredService<IdentityDataContext>();
        await dataContext.Database.EnsureCreatedAsync(_testContext.CancellationToken);
    }

    [TestCleanup]
    public async Task CleanupAsync()
    {
        await _serviceProvider.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [TestMethod]
    public async Task StartAsync_EmptyDatabase_CreatesOneIdempotentSuperAdministrator()
    {
        // Arrange
        var options = Microsoft.Extensions.Options.Options.Create(new BootstrapAdminOptions
        {
            Enabled = true,
            Email = "bootstrap@example.test",
            UserName = "bootstrap.admin",
            Password = "Valid!Password123",
            FirstName = "Bootstrap",
            LastName = "Administrator"
        });
        var sut = new SuperAdminBootstrapHostedService(
            _serviceProvider.GetRequiredService<IServiceScopeFactory>(),
            options,
            new TestTimeProvider(Now),
            NullLogger<SuperAdminBootstrapHostedService>.Instance);

        // Act
        await sut.StartAsync(_testContext.CancellationToken);
        await sut.StartAsync(_testContext.CancellationToken);

        // Assert
        await using var scope = _serviceProvider.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var dataContext = scope.ServiceProvider.GetRequiredService<IdentityDataContext>();
        var user = await userManager.FindByEmailAsync("bootstrap@example.test");
        Assert.IsNotNull(user);
        Assert.IsTrue(user.EmailConfirmed);
        Assert.AreEqual(Now, user.CreatedAtUtc);
        var roles = await userManager.GetRolesAsync(user);
        Assert.HasCount(2, roles);
        Assert.Contains(RoleNames.User, roles);
        Assert.Contains(RoleNames.SuperAdmin, roles);
        Assert.AreEqual(1, await dataContext.Users.CountAsync(_testContext.CancellationToken));
        var outbox = await dataContext.OutboxMessages.SingleAsync(_testContext.CancellationToken);
        Assert.AreEqual("SuperAdministratorBootstrapped", outbox.Type);
        Assert.Contains(user.Id.ToString(), outbox.Payload);
    }
}
