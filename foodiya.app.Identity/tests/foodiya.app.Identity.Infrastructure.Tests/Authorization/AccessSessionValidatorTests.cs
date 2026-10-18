using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using foodiya.app.Identity.Application.Exceptions;
using foodiya.app.Identity.Domain.Entities;
using foodiya.app.Identity.Domain.Enums;
using foodiya.app.Identity.Infrastructure.Authentication;
using foodiya.app.Identity.Infrastructure.Authorization;
using foodiya.app.Identity.Infrastructure.Identity;
using foodiya.app.Identity.Infrastructure.Persistence;
using foodiya.app.Identity.Infrastructure.Tests.Authentication;

namespace foodiya.app.Identity.Infrastructure.Tests.Authorization;

[TestClass]
public sealed class AccessSessionValidatorTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 9, 4, 12, 0, 0, TimeSpan.Zero);

    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _serviceProvider;
    private readonly TestContext _testContext;
    private readonly TestTimeProvider _timeProvider = new(Now);

    private AsyncServiceScope _scope;
    private IdentityDataContext _dataContext = null!;
    private ApplicationUser _user = null!;
    private UserSession _session = null!;
    private UserDevice _device = null!;
    private AccessSessionValidator _sut = null!;

    public AccessSessionValidatorTests(TestContext testContext)
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

        _user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "active.user",
            Email = "active@example.test",
            FirstName = "Active",
            LastName = "User",
            EmailConfirmed = true,
            Status = AccountStatus.Active,
            CreatedAtUtc = Now.AddDays(-1),
            LockoutEnabled = true
        };
        var userManager = _scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var createResult = await userManager.CreateAsync(_user, "Valid!Password123");
        Assert.IsTrue(createResult.Succeeded, Describe(createResult));

        _device = new UserDevice
        {
            Id = Guid.NewGuid(),
            UserId = _user.Id,
            InstallationIdHash = new string('A', 64),
            FirstSeenAtUtc = Now.AddHours(-1),
            LastSeenAtUtc = Now
        };
        _session = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = _user.Id,
            DeviceId = _device.Id,
            CreatedAtUtc = Now.AddMinutes(-10),
            ExpiresAtUtc = Now.AddDays(1)
        };
        _dataContext.UserDevices.Add(_device);
        _dataContext.UserSessions.Add(_session);
        await _dataContext.SaveChangesAsync(_testContext.CancellationToken);

        _sut = new AccessSessionValidator(_dataContext, _timeProvider);
    }

    [TestCleanup]
    public async Task CleanupAsync()
    {
        await _scope.DisposeAsync();
        await _serviceProvider.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [TestMethod]
    public async Task ValidateAsync_ActiveMatchingSession_ReturnsValidatedContext()
    {
        // Act
        var result = await _sut.ValidateAsync(CreatePrincipal(), _testContext.CancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(_user.Id, result.User.Id);
        Assert.AreEqual(_session.Id, result.Session.Id);
        Assert.AreEqual(_device.Id, result.Device.Id);
    }

    [TestMethod]
    public async Task ValidateAsync_ChangedSecurityStamp_ReturnsNull()
    {
        // Arrange
        var principal = CreatePrincipal();
        _user.SecurityStamp = Guid.NewGuid().ToString("N");
        await _dataContext.SaveChangesAsync(_testContext.CancellationToken);

        // Act
        var result = await _sut.ValidateAsync(principal, _testContext.CancellationToken);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task ValidateAsync_RevokedSession_ReturnsNull()
    {
        // Arrange
        _session.RevokedAtUtc = Now;
        _session.RevocationReason = "Test";
        await _dataContext.SaveChangesAsync(_testContext.CancellationToken);

        // Act
        var result = await _sut.ValidateAsync(CreatePrincipal(), _testContext.CancellationToken);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task ValidateAsync_RevokedDevice_ReturnsNull()
    {
        // Arrange
        _device.RevokedAtUtc = Now;
        _device.RevocationReason = "Test";
        await _dataContext.SaveChangesAsync(_testContext.CancellationToken);

        // Act
        var result = await _sut.ValidateAsync(CreatePrincipal(), _testContext.CancellationToken);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetAsync_AuthenticatedPrincipal_ReusesRequestValidation()
    {
        // Arrange
        var principal = CreatePrincipal();
        var context = new DefaultHttpContext { User = principal };
        context.Features.Set(new AuthenticatedAccessSession(
            principal, new ValidatedAccessSession(_user, _session, _device)));
        var validator = new CountingAccessSessionValidator(_sut);
        var service = CreateCurrentIdentityService(validator, new HttpContextAccessor { HttpContext = context });

        // Act
        var response = await service.GetAsync(principal, _testContext.CancellationToken);

        // Assert
        Assert.AreEqual(_user.Id, response.UserId);
        Assert.AreEqual(_session.Id, response.SessionId);
        Assert.AreEqual(_device.Id, response.DeviceId);
        Assert.AreEqual(0, validator.CallCount);
    }

    [TestMethod]
    public async Task GetAsync_DifferentPrincipal_RejectsCachedIdentity()
    {
        // Arrange
        var context = new DefaultHttpContext { User = CreatePrincipal() };
        context.Features.Set(new AuthenticatedAccessSession(
            context.User, new ValidatedAccessSession(_user, _session, _device)));
        var validator = new CountingAccessSessionValidator(_sut);
        var service = CreateCurrentIdentityService(validator, new HttpContextAccessor { HttpContext = context });
        var otherPrincipal = new ClaimsPrincipal(new ClaimsIdentity("Test"));

        // Act
        var exception = await Assert.ThrowsExactlyAsync<IdentityUnauthorizedException>(() =>
            service.GetAsync(otherPrincipal, _testContext.CancellationToken));

        // Assert
        Assert.AreEqual("ACCESS_SESSION_INVALID", exception.Code);
        Assert.AreEqual(1, validator.CallCount);
    }

    [TestMethod]
    public async Task GetAsync_NewRequestAfterRevocation_RevalidatesAndRejectsSession()
    {
        // Arrange
        var principal = CreatePrincipal();
        var context = new DefaultHttpContext { User = principal };
        context.Features.Set(new AuthenticatedAccessSession(
            principal, new ValidatedAccessSession(_user, _session, _device)));
        var accessor = new HttpContextAccessor { HttpContext = context };
        var validator = new CountingAccessSessionValidator(_sut);
        var service = CreateCurrentIdentityService(validator, accessor);
        await service.GetAsync(principal, _testContext.CancellationToken);
        _session.RevokedAtUtc = Now;
        await _dataContext.SaveChangesAsync(_testContext.CancellationToken);
        accessor.HttpContext = new DefaultHttpContext { User = principal };

        // Act
        var exception = await Assert.ThrowsExactlyAsync<IdentityUnauthorizedException>(() =>
            service.GetAsync(principal, _testContext.CancellationToken));

        // Assert
        Assert.AreEqual("ACCESS_SESSION_INVALID", exception.Code);
        Assert.AreEqual(1, validator.CallCount);
    }

    private CurrentIdentityService CreateCurrentIdentityService(
        IAccessSessionValidator validator, IHttpContextAccessor accessor) => new(
            validator,
            _scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(),
            accessor);

    private sealed class CountingAccessSessionValidator(IAccessSessionValidator inner) : IAccessSessionValidator
    {
        public int CallCount { get; private set; }

        public Task<ValidatedAccessSession?> ValidateAsync(
            ClaimsPrincipal principal, CancellationToken cancellationToken)
        {
            CallCount++;
            return inner.ValidateAsync(principal, cancellationToken);
        }
    }

    private ClaimsPrincipal CreatePrincipal() => new(new ClaimsIdentity(
        [
            new Claim(JwtRegisteredClaimNames.Sub, _user.Id.ToString()),
            new Claim("sid", _session.Id.ToString()),
            new Claim("did", _device.Id.ToString()),
            new Claim("sst", _user.SecurityStamp ?? string.Empty)
        ],
        "Test"));

    private static string Describe(IdentityResult result) => string.Join(
        "; ",
        result.Errors.Select(error => $"{error.Code}: {error.Description}"));
}
