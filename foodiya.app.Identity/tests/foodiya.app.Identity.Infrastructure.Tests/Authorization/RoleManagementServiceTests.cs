using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using foodiya.app.Identity.Application.Exceptions;
using foodiya.app.Identity.Contracts.Roles;
using foodiya.app.Identity.Domain.Constants;
using foodiya.app.Identity.Domain.Entities;
using foodiya.app.Identity.Domain.Enums;
using foodiya.app.Identity.Infrastructure.Authentication;
using foodiya.app.Identity.Infrastructure.Authorization;
using foodiya.app.Identity.Infrastructure.Identity;
using foodiya.app.Identity.Infrastructure.Persistence;
using foodiya.app.Identity.Infrastructure.Tests.Authentication;

namespace foodiya.app.Identity.Infrastructure.Tests.Authorization;

[TestClass]
public sealed class RoleManagementServiceTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 9, 4, 13, 0, 0, TimeSpan.Zero);

    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _serviceProvider;
    private readonly TestContext _testContext;
    private readonly TestTimeProvider _timeProvider = new(Now);

    private AsyncServiceScope _scope;
    private IdentityDataContext _dataContext = null!;
    private UserManager<ApplicationUser> _userManager = null!;
    private ApplicationUser _user = null!;
    private RoleManagementService _sut = null!;

    public RoleManagementServiceTests(TestContext testContext)
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
        _scope = _serviceProvider.CreateAsyncScope();
        _dataContext = _scope.ServiceProvider.GetRequiredService<IdentityDataContext>();
        await _dataContext.Database.EnsureCreatedAsync(_testContext.CancellationToken);
        _userManager = _scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        _user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "role.target",
            Email = "roles@example.test",
            FirstName = "Role",
            LastName = "Target",
            EmailConfirmed = true,
            Status = AccountStatus.Active,
            CreatedAtUtc = Now.AddDays(-1),
            LockoutEnabled = true
        };
        var createResult = await _userManager.CreateAsync(_user, "Valid!Password123");
        Assert.IsTrue(createResult.Succeeded, Describe(createResult));
        var roleResult = await _userManager.AddToRoleAsync(_user, RoleNames.User);
        Assert.IsTrue(roleResult.Succeeded, Describe(roleResult));

        _sut = new RoleManagementService(_userManager, _dataContext, _timeProvider);
    }

    [TestCleanup]
    public async Task CleanupAsync()
    {
        await _scope.DisposeAsync();
        await _serviceProvider.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [TestMethod]
    public async Task ReplaceUserRolesAsync_ChangedRoles_RevokesSessionsAndWritesOutbox()
    {
        // Arrange
        var actorUserId = Guid.NewGuid();
        var originalSecurityStamp = _user.SecurityStamp;
        var device = new UserDevice
        {
            Id = Guid.NewGuid(),
            UserId = _user.Id,
            InstallationIdHash = new string('B', 64),
            FirstSeenAtUtc = Now.AddHours(-1),
            LastSeenAtUtc = Now
        };
        var session = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = _user.Id,
            DeviceId = device.Id,
            CreatedAtUtc = Now.AddMinutes(-30),
            ExpiresAtUtc = Now.AddDays(1)
        };
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = _user.Id,
            SessionId = session.Id,
            TokenHash = new string('C', 64),
            CreatedAtUtc = Now.AddMinutes(-30),
            ExpiresAtUtc = Now.AddDays(1)
        };
        _dataContext.AddRange(device, session, refreshToken);
        await _dataContext.SaveChangesAsync(_testContext.CancellationToken);

        // Act
        var response = await _sut.ReplaceUserRolesAsync(
            actorUserId,
            _user.Id,
            new ReplaceUserRolesRequest { Roles = [" chef ", "USER", "CHEF"] },
            _testContext.CancellationToken);
        _dataContext.ChangeTracker.Clear();

        // Assert
        Assert.AreSequenceEqual(
            new[] { RoleNames.Chef, RoleNames.User },
            response.Roles.ToArray());
        var updatedUser = await _dataContext.Users.SingleAsync(_testContext.CancellationToken);
        var updatedSession = await _dataContext.UserSessions.SingleAsync(_testContext.CancellationToken);
        var updatedToken = await _dataContext.RefreshTokens.SingleAsync(_testContext.CancellationToken);
        var outbox = await _dataContext.OutboxMessages.SingleAsync(_testContext.CancellationToken);
        Assert.AreNotEqual(originalSecurityStamp, updatedUser.SecurityStamp);
        Assert.AreEqual(Now, updatedUser.UpdatedAtUtc);
        Assert.AreEqual(Now, updatedSession.RevokedAtUtc);
        Assert.AreEqual("RolesChanged", updatedSession.RevocationReason);
        Assert.AreEqual(Now, updatedToken.RevokedAtUtc);
        Assert.AreEqual("RolesChanged", updatedToken.RevocationReason);
        Assert.AreEqual("UserRolesChanged", outbox.Type);
        Assert.Contains(actorUserId.ToString(), outbox.Payload);
    }

    [TestMethod]
    public async Task ReplaceUserRolesAsync_UnknownRole_ThrowsValidationException()
    {
        // Act
        var exception = await Assert.ThrowsExactlyAsync<IdentityValidationException>(() =>
            _sut.ReplaceUserRolesAsync(
                Guid.NewGuid(),
                _user.Id,
                new ReplaceUserRolesRequest { Roles = ["OWNER"] },
                _testContext.CancellationToken));

        // Assert
        Assert.AreEqual("ROLE_INVALID", exception.Code);
        var roles = await _userManager.GetRolesAsync(_user);
        Assert.Contains(RoleNames.User, roles);
    }

    [TestMethod]
    public async Task ReplaceUserRolesAsync_FinalSuperAdministrator_DemotionIsRejected()
    {
        // Arrange
        var addResult = await _userManager.AddToRoleAsync(_user, RoleNames.SuperAdmin);
        Assert.IsTrue(addResult.Succeeded, Describe(addResult));

        // Act
        var exception = await Assert.ThrowsExactlyAsync<IdentityConflictException>(() =>
            _sut.ReplaceUserRolesAsync(
                _user.Id,
                _user.Id,
                new ReplaceUserRolesRequest { Roles = [RoleNames.User] },
                _testContext.CancellationToken));

        // Assert
        Assert.AreEqual("LAST_SUPER_ADMIN_REQUIRED", exception.Code);
        Assert.IsTrue(await _userManager.IsInRoleAsync(_user, RoleNames.SuperAdmin));
    }

    [TestMethod]
    public void GetAvailableRoles_ReturnsCanonicalRoleCatalog()
    {
        // Act
        var result = _sut.GetAvailableRoles();

        // Assert
        Assert.AreSequenceEqual(RoleNames.All.ToArray(), result.Roles.ToArray());
    }

    private static string Describe(IdentityResult result) => string.Join(
        "; ",
        result.Errors.Select(error => $"{error.Code}: {error.Description}"));
}
