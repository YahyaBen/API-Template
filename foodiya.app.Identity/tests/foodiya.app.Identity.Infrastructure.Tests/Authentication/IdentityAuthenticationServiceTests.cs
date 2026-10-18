using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using foodiya.app.Identity.Application.Exceptions;
using foodiya.app.Identity.Application.Interfaces;
using foodiya.app.Identity.Contracts.Authentication;
using foodiya.app.Identity.Domain.Constants;
using foodiya.app.Identity.Domain.Entities;
using foodiya.app.Identity.Domain.Enums;
using foodiya.app.Identity.Infrastructure.Authentication;
using foodiya.app.Identity.Infrastructure.Identity;
using foodiya.app.Identity.Infrastructure.Options;
using foodiya.app.Identity.Infrastructure.Persistence;
using MicrosoftOptions = Microsoft.Extensions.Options.Options;

namespace foodiya.app.Identity.Infrastructure.Tests.Authentication;

[TestClass]
public sealed class IdentityAuthenticationServiceTests
{
    private const string Password = "Valid!Password123";
    private static readonly DateTimeOffset InitialTime =
        new(2026, 9, 4, 10, 30, 0, TimeSpan.Zero);

    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _serviceProvider;
    private readonly TestContext _testContext;
    private readonly TestTimeProvider _timeProvider = new(InitialTime);
    private readonly AuthenticationTokenOptions _tokenOptions = new()
    {
        Issuer = "foodiya-app-identity-tests",
        Audience = "sandbox-clients",
        RefreshTokenPepper = "unit-test-refresh-token-pepper-32-characters",
        AccessTokenLifetimeMinutes = 15,
        RefreshTokenLifetimeDays = 30
    };

    private AsyncServiceScope _scope;
    private IdentityDataContext _dataContext = null!;
    private UserManager<ApplicationUser> _userManager = null!;
    private TokenCryptographyService _cryptography = null!;
    private StubGoogleIdTokenValidator _googleValidator = null!;
    private IdentityAuthenticationService _sut = null!;

    public IdentityAuthenticationServiceTests(TestContext testContext)
    {
        _testContext = testContext;
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<IdentityDataContext>(options => options
            .UseSqlite(_connection)
            .ReplaceService<IModelCustomizer, SqliteTestModelCustomizer>());
        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<IdentityDataContext>();
        _serviceProvider = services.BuildServiceProvider();
    }

    [TestMethod]
    [DataRow(256)]
    [DataRow(257)]
    public async Task GoogleSignIn_EnforcesEmailContractBeforeCreatingAccount(int emailLength)
    {
        var email = new string('u', 64) + "@" + new string('a', 63) + "." + new string('b', 63) + "." + new string('c', 63);
        if (emailLength == 257) email = email[..^2] + ".xx";
        _googleValidator.Identity = _googleValidator.Identity with { Email = email, EmailVerified = true };
        var request = new GoogleSignInRequest { IdToken = "test-token", DeviceInstallationId = "email-boundary-device" };
        if (emailLength > 256)
        {
            await Assert.ThrowsExactlyAsync<IdentityUnauthorizedException>(() => _sut.SignInWithGoogleAsync(request, null, _testContext.CancellationToken));
            Assert.AreEqual(0, await _dataContext.OutboxMessages.CountAsync(_testContext.CancellationToken));
            Assert.IsNull(await _userManager.FindByEmailAsync(email));
        }
        else
        {
            var response = await _sut.SignInWithGoogleAsync(request, null, _testContext.CancellationToken);
            Assert.AreEqual(email, response.Email);
            var registered = await _dataContext.OutboxMessages.SingleAsync(message => message.Type == "UserRegistered", _testContext.CancellationToken);
            using var payload = JsonDocument.Parse(registered.Payload);
            Assert.AreEqual(email, payload.RootElement.GetProperty("Email").GetString());
        }
    }

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _scope = _serviceProvider.CreateAsyncScope();
        _dataContext = _scope.ServiceProvider.GetRequiredService<IdentityDataContext>();
        await _dataContext.Database.EnsureCreatedAsync(_testContext.CancellationToken);

        _userManager = _scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser
        {
            Id = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
            UserName = "security.tester",
            Email = "security@example.test",
            FirstName = "Security",
            LastName = "Tester",
            EmailConfirmed = true,
            Status = AccountStatus.Active,
            CreatedAtUtc = InitialTime.AddDays(-1)
        };
        var createResult = await _userManager.CreateAsync(user, Password);
        Assert.IsTrue(createResult.Succeeded, Describe(createResult));
        var roleResult = await _userManager.AddToRoleAsync(user, RoleNames.User);
        Assert.IsTrue(roleResult.Succeeded, Describe(roleResult));

        _cryptography = new TokenCryptographyService(MicrosoftOptions.Create(_tokenOptions));
        _googleValidator = new StubGoogleIdTokenValidator();
        _sut = new IdentityAuthenticationService(
            _userManager,
            _dataContext,
            new StubAccessTokenService(_timeProvider),
            _cryptography,
            new NoOpLoginTimingProtection(),
            _googleValidator,
            MicrosoftOptions.Create(_tokenOptions),
            _timeProvider);
    }

    [TestCleanup]
    public async Task CleanupAsync()
    {
        await _scope.DisposeAsync();
        await _serviceProvider.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [TestMethod]
    public async Task LoginAsync_ValidCredentials_PersistsOnlyHashedRefreshAndDeviceSecrets()
    {
        // Arrange
        var request = CreateLoginRequest();

        // Act
        var response = await _sut.LoginAsync(
            request,
            " 203.0.113.10 ",
            _testContext.CancellationToken);
        _dataContext.ChangeTracker.Clear();

        // Assert
        var persistedToken = await _dataContext.RefreshTokens.SingleAsync(
            _testContext.CancellationToken);
        var persistedDevice = await _dataContext.UserDevices.SingleAsync(
            _testContext.CancellationToken);
        var persistedSession = await _dataContext.UserSessions.SingleAsync(
            _testContext.CancellationToken);

        Assert.AreNotEqual(response.RefreshToken, persistedToken.TokenHash);
        Assert.AreEqual(
            _cryptography.HashRefreshToken(response.RefreshToken),
            persistedToken.TokenHash);
        Assert.DoesNotContain(response.RefreshToken, persistedToken.TokenHash);
        Assert.AreEqual(
            _cryptography.HashDeviceInstallationId(request.DeviceInstallationId),
            persistedDevice.InstallationIdHash);
        Assert.AreNotEqual(request.DeviceInstallationId, persistedDevice.InstallationIdHash);
        Assert.AreEqual("203.0.113.10", persistedDevice.LastIpAddress);
        Assert.AreEqual("Android", persistedDevice.Platform);
        Assert.AreEqual(response.SessionId, persistedSession.Id);
        Assert.AreEqual(response.DeviceId, persistedSession.DeviceId);
        Assert.AreEqual(InitialTime.AddDays(30), response.RefreshTokenExpiresAtUtc);
        Assert.Contains(RoleNames.User, response.Roles);
    }

    [TestMethod]
    public async Task RefreshAsync_ActiveToken_RotatesTokenWithinSameSession()
    {
        // Arrange
        var login = await _sut.LoginAsync(
            CreateLoginRequest(),
            "203.0.113.10",
            _testContext.CancellationToken);
        _dataContext.ChangeTracker.Clear();
        _timeProvider.Advance(TimeSpan.FromMinutes(5));

        // Act
        var refreshed = await _sut.RefreshAsync(
            new RefreshAccessTokenRequest { RefreshToken = login.RefreshToken },
            "203.0.113.11",
            _testContext.CancellationToken);
        _dataContext.ChangeTracker.Clear();

        // Assert
        var oldHash = _cryptography.HashRefreshToken(login.RefreshToken);
        var replacementHash = _cryptography.HashRefreshToken(refreshed.RefreshToken);
        var oldToken = await _dataContext.RefreshTokens.SingleAsync(
            token => token.TokenHash == oldHash,
            _testContext.CancellationToken);
        var replacement = await _dataContext.RefreshTokens.SingleAsync(
            token => token.TokenHash == replacementHash,
            _testContext.CancellationToken);
        var device = await _dataContext.UserDevices.SingleAsync(
            _testContext.CancellationToken);

        Assert.AreNotEqual(login.RefreshToken, refreshed.RefreshToken);
        Assert.AreEqual(login.SessionId, refreshed.SessionId);
        Assert.AreEqual(login.DeviceId, refreshed.DeviceId);
        Assert.AreEqual(InitialTime.AddMinutes(5), oldToken.RevokedAtUtc);
        Assert.AreEqual("Rotated", oldToken.RevocationReason);
        Assert.AreEqual("203.0.113.11", oldToken.RevokedByIp);
        Assert.AreEqual(replacementHash, oldToken.ReplacedByTokenHash);
        Assert.IsNull(replacement.RevokedAtUtc);
        Assert.AreEqual(oldToken.SessionId, replacement.SessionId);
        Assert.AreEqual(oldToken.ExpiresAtUtc, replacement.ExpiresAtUtc);
        Assert.AreEqual(InitialTime.AddMinutes(5), device.LastSeenAtUtc);
        Assert.AreEqual("203.0.113.11", device.LastIpAddress);
    }

    [TestMethod]
    public async Task RefreshAsync_PreviouslyRotatedToken_RevokesSessionAndReplacementToken()
    {
        // Arrange
        var login = await _sut.LoginAsync(
            CreateLoginRequest(),
            "203.0.113.10",
            _testContext.CancellationToken);
        _dataContext.ChangeTracker.Clear();
        _timeProvider.Advance(TimeSpan.FromMinutes(5));
        var refreshed = await _sut.RefreshAsync(
            new RefreshAccessTokenRequest { RefreshToken = login.RefreshToken },
            "203.0.113.11",
            _testContext.CancellationToken);
        _dataContext.ChangeTracker.Clear();
        _timeProvider.Advance(TimeSpan.FromMinutes(1));

        // Act
        var exception = await Assert.ThrowsExactlyAsync<IdentityUnauthorizedException>(() =>
            _sut.RefreshAsync(
                new RefreshAccessTokenRequest { RefreshToken = login.RefreshToken },
                "203.0.113.99",
                _testContext.CancellationToken));
        _dataContext.ChangeTracker.Clear();

        // Assert
        var session = await _dataContext.UserSessions.SingleAsync(
            _testContext.CancellationToken);
        var replacementHash = _cryptography.HashRefreshToken(refreshed.RefreshToken);
        var replacement = await _dataContext.RefreshTokens.SingleAsync(
            token => token.TokenHash == replacementHash,
            _testContext.CancellationToken);

        Assert.AreEqual("REFRESH_TOKEN_REUSE_DETECTED", exception.Code);
        Assert.AreEqual(InitialTime.AddMinutes(6), session.RevokedAtUtc);
        Assert.AreEqual("RefreshTokenReuseDetected", session.RevocationReason);
        Assert.AreEqual(InitialTime.AddMinutes(6), replacement.RevokedAtUtc);
        Assert.AreEqual("RefreshTokenReuseDetected", replacement.RevocationReason);
        Assert.AreEqual("203.0.113.99", replacement.RevokedByIp);
        Assert.IsFalse(session.IsActive(_timeProvider.GetUtcNow()));
        Assert.IsFalse(replacement.IsActive(_timeProvider.GetUtcNow()));
    }

    [TestMethod]
    public async Task SignInWithGoogleAsync_NewVerifiedIdentity_CreatesLinkedActiveUserAndSession()
    {
        // Arrange
        var request = CreateGoogleRequest();

        // Act
        var response = await _sut.SignInWithGoogleAsync(
            request,
            "203.0.113.20",
            _testContext.CancellationToken);
        _dataContext.ChangeTracker.Clear();

        // Assert
        var user = await _userManager.FindByLoginAsync("Google", _googleValidator.Identity.Subject);
        Assert.IsNotNull(user);
        Assert.AreEqual(response.UserId, user.Id);
        Assert.AreEqual(_googleValidator.Identity.Email, user.Email);
        Assert.IsTrue(user.EmailConfirmed);
        Assert.AreEqual(AccountStatus.Active, user.Status);
        Assert.Contains(RoleNames.User, await _userManager.GetRolesAsync(user));
        Assert.HasCount(1, await _userManager.GetLoginsAsync(user));
        Assert.AreEqual(1, await _dataContext.OutboxMessages.CountAsync(
            message => message.Type == "UserRegistered", _testContext.CancellationToken));

        var session = await _dataContext.UserSessions.SingleAsync(
            candidate => candidate.UserId == user.Id,
            _testContext.CancellationToken);
        var device = await _dataContext.UserDevices.SingleAsync(
            candidate => candidate.Id == session.DeviceId,
            _testContext.CancellationToken);
        var refreshToken = await _dataContext.RefreshTokens.SingleAsync(
            candidate => candidate.SessionId == session.Id,
            _testContext.CancellationToken);
        Assert.AreEqual("203.0.113.20", device.LastIpAddress);
        Assert.AreEqual("Android", device.Platform);
        Assert.AreEqual(
            _cryptography.HashRefreshToken(response.RefreshToken),
            refreshToken.TokenHash);
    }

    [TestMethod]
    public async Task SignInWithGoogleAsync_PreviouslyLinkedSubject_ReusesAccount()
    {
        // Arrange
        var request = CreateGoogleRequest();
        var first = await _sut.SignInWithGoogleAsync(
            request,
            "203.0.113.20",
            _testContext.CancellationToken);
        _dataContext.ChangeTracker.Clear();

        // Act
        var second = await _sut.SignInWithGoogleAsync(
            request,
            "203.0.113.21",
            _testContext.CancellationToken);
        _dataContext.ChangeTracker.Clear();

        // Assert
        Assert.AreEqual(first.UserId, second.UserId);
        Assert.AreEqual(2, await _dataContext.Users.CountAsync(_testContext.CancellationToken));
        Assert.AreEqual(1, await _dataContext.UserLogins.CountAsync(_testContext.CancellationToken));
        Assert.AreEqual(2, await _dataContext.UserSessions.CountAsync(_testContext.CancellationToken));
        Assert.AreEqual(1, await _dataContext.UserDevices.CountAsync(_testContext.CancellationToken));
        Assert.AreEqual(1, await _dataContext.OutboxMessages.CountAsync(
            message => message.Type == "UserRegistered", _testContext.CancellationToken));
    }

    [TestMethod]
    public async Task SignInWithGoogleAsync_UnverifiedEmail_RejectsIdentity()
    {
        // Arrange
        _googleValidator.Identity = _googleValidator.Identity with { EmailVerified = false };

        // Act
        var exception = await Assert.ThrowsExactlyAsync<IdentityUnauthorizedException>(() =>
            _sut.SignInWithGoogleAsync(
                CreateGoogleRequest(),
                "203.0.113.20",
                _testContext.CancellationToken));

        // Assert
        Assert.AreEqual("GOOGLE_EMAIL_NOT_VERIFIED", exception.Code);
        Assert.AreEqual(1, await _dataContext.Users.CountAsync(_testContext.CancellationToken));
        Assert.AreEqual(0, await _dataContext.UserSessions.CountAsync(_testContext.CancellationToken));
    }

    [TestMethod]
    public async Task SignInWithGoogleAsync_NonAuthoritativeExistingEmail_RejectsAutomaticLink()
    {
        // Arrange
        _googleValidator.Identity = _googleValidator.Identity with
        {
            Email = "security@example.test",
            HostedDomain = null
        };

        // Act
        var exception = await Assert.ThrowsExactlyAsync<IdentityConflictException>(() =>
            _sut.SignInWithGoogleAsync(
                CreateGoogleRequest(),
                "203.0.113.20",
                _testContext.CancellationToken));

        // Assert
        Assert.AreEqual("GOOGLE_EMAIL_LINK_REQUIRES_VERIFICATION", exception.Code);
        Assert.AreEqual(0, await _dataContext.UserLogins.CountAsync(_testContext.CancellationToken));
        Assert.AreEqual(0, await _dataContext.UserSessions.CountAsync(_testContext.CancellationToken));
    }

    [TestMethod]
    public async Task SignInWithGoogleAsync_AuthoritativeExistingEmail_LinksAccount()
    {
        // Arrange
        var existingUser = await _userManager.FindByEmailAsync("security@example.test");
        Assert.IsNotNull(existingUser);
        existingUser.Email = "security@gmail.com";
        var updateResult = await _userManager.UpdateAsync(existingUser);
        Assert.IsTrue(updateResult.Succeeded, Describe(updateResult));
        _googleValidator.Identity = _googleValidator.Identity with
        {
            Email = "security@gmail.com",
            HostedDomain = null
        };

        // Act
        var response = await _sut.SignInWithGoogleAsync(
            CreateGoogleRequest(),
            "203.0.113.20",
            _testContext.CancellationToken);
        _dataContext.ChangeTracker.Clear();

        // Assert
        Assert.AreEqual(existingUser.Id, response.UserId);
        Assert.AreEqual(1, await _dataContext.Users.CountAsync(_testContext.CancellationToken));
        Assert.AreEqual(1, await _dataContext.UserLogins.CountAsync(_testContext.CancellationToken));
        Assert.AreEqual(1, await _dataContext.UserSessions.CountAsync(_testContext.CancellationToken));
        var persistedUser = await _userManager.FindByIdAsync(existingUser.Id.ToString());
        Assert.IsNotNull(persistedUser);
        Assert.IsTrue(await _userManager.CheckPasswordAsync(persistedUser, Password));
        Assert.AreEqual(0, await _dataContext.OutboxMessages.CountAsync(
            message => message.Type == "UserRegistered", _testContext.CancellationToken));
    }

    [TestMethod]
    public async Task SignInWithGoogleAsync_PendingAccount_DiscardsUnverifiedCredentialsAndRegistersOnce()
    {
        // Arrange
        var user = await _userManager.FindByEmailAsync("security@example.test");
        Assert.IsNotNull(user);
        user.Email = _googleValidator.Identity.Email;
        user.EmailConfirmed = false;
        user.Status = AccountStatus.Pending;
        var updateResult = await _userManager.UpdateAsync(user);
        Assert.IsTrue(updateResult.Succeeded, Describe(updateResult));
        var originalStamp = user.SecurityStamp;
        var challenge = new OtpChallenge
        {
            UserId = user.Id,
            NormalizedEmail = user.NormalizedEmail!,
            CodeHash = new string('A', 64),
            Purpose = OtpPurpose.Registration,
            CreatedAtUtc = InitialTime,
            ExpiresAtUtc = InitialTime.AddMinutes(10),
            MaxAttempts = 5
        };
        _dataContext.OtpChallenges.Add(challenge);
        await _dataContext.SaveChangesAsync(_testContext.CancellationToken);
        Assert.IsTrue(challenge.IsUsable(InitialTime));
        _dataContext.ChangeTracker.Clear();

        // Act
        var response = await _sut.SignInWithGoogleAsync(
            CreateGoogleRequest(), "203.0.113.20", _testContext.CancellationToken);
        _dataContext.ChangeTracker.Clear();

        // Assert
        var activated = await _userManager.FindByLoginAsync("Google", _googleValidator.Identity.Subject);
        Assert.IsNotNull(activated);
        Assert.AreEqual(user.Id, activated.Id);
        Assert.AreEqual(user.Id, response.UserId);
        Assert.IsTrue(activated.EmailConfirmed);
        Assert.AreEqual(AccountStatus.Active, activated.Status);
        Assert.IsNull(activated.PasswordHash);
        Assert.AreNotEqual(originalStamp, activated.SecurityStamp);
        var persistedChallenge = await _dataContext.OtpChallenges.SingleAsync(
            item => item.Id == challenge.Id, _testContext.CancellationToken);
        Assert.AreEqual(InitialTime, persistedChallenge.InvalidatedAtUtc);
        Assert.IsFalse(persistedChallenge.IsUsable(InitialTime));

        var login = CreateLoginRequest() with { Login = activated.Email! };
        var exception = await Assert.ThrowsExactlyAsync<IdentityUnauthorizedException>(() =>
            _sut.LoginAsync(login, "203.0.113.99", _testContext.CancellationToken));
        Assert.AreEqual("INVALID_CREDENTIALS", exception.Code);

        // A later Google sign-in must not publish a second registration event.
        _dataContext.ChangeTracker.Clear();
        await _sut.SignInWithGoogleAsync(
            CreateGoogleRequest(), "203.0.113.20", _testContext.CancellationToken);
        var registered = Assert.ContainsSingle(await _dataContext.OutboxMessages.AsNoTracking()
            .Where(message => message.Type == "UserRegistered")
            .ToArrayAsync(_testContext.CancellationToken));
        using var payload = JsonDocument.Parse(registered.Payload);
        Assert.AreEqual(user.Id, payload.RootElement.GetProperty("Id").GetGuid());
        Assert.AreEqual(activated.Email, payload.RootElement.GetProperty("Email").GetString());
        Assert.AreEqual("Google", payload.RootElement.GetProperty("Provider").GetString());
    }

    private static LoginRequest CreateLoginRequest() => new()
    {
        Login = "security@example.test",
        Password = Password,
        DeviceInstallationId = "device-installation-security-test",
        Platform = " Android ",
        DeviceName = " Test Phone ",
        OsVersion = " 16 ",
        AppVersion = " 1.0.0 "
    };

    private static GoogleSignInRequest CreateGoogleRequest() => new()
    {
        IdToken = "validated-by-test-double",
        DeviceInstallationId = "device-installation-google-test",
        Platform = " Android ",
        DeviceName = " Google Test Phone ",
        OsVersion = " 16 ",
        AppVersion = " 1.0.0 "
    };

    private static string Describe(IdentityResult result) => string.Join(
        "; ",
        result.Errors.Select(error => $"{error.Code}: {error.Description}"));

    private sealed class StubAccessTokenService(TimeProvider timeProvider) : IAccessTokenService
    {
        public (string Token, DateTimeOffset ExpiresAtUtc) Create(
            ApplicationUser user,
            IReadOnlyCollection<string> roles,
            Guid sessionId,
            Guid deviceId,
            DateTimeOffset sessionExpiresAtUtc) =>
            ($"access-token-{Guid.NewGuid():N}", timeProvider.GetUtcNow().AddMinutes(15));
    }

    private sealed class NoOpLoginTimingProtection : ILoginTimingProtection
    {
        public void VerifyDummyPassword(string suppliedPassword)
        {
        }
    }
}

internal sealed class StubGoogleIdTokenValidator : IGoogleIdTokenValidator
{
    public GoogleIdentity Identity { get; set; } = new(
        "google-subject-123",
        "google.user@gmail.com",
        true,
        "Google",
        "User",
        null);

    public Task<GoogleIdentity> ValidateAsync(
        string idToken,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Identity);
    }
}
