using System.Data.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using foodiya.app.Identity.Application.Exceptions;
using foodiya.app.Identity.Application.Interfaces;
using foodiya.app.Identity.Contracts.Accounts;
using foodiya.app.Identity.Contracts.Authentication;
using foodiya.app.Identity.Domain.Constants;
using foodiya.app.Identity.Domain.Entities;
using foodiya.app.Identity.Domain.Enums;
using foodiya.app.Identity.Infrastructure.Accounts;
using foodiya.app.Identity.Infrastructure.Authentication;
using foodiya.app.Identity.Infrastructure.Authorization;
using foodiya.app.Identity.Infrastructure.Identity;
using foodiya.app.Identity.Infrastructure.Options;
using foodiya.app.Identity.Infrastructure.Persistence;
using foodiya.app.Identity.Infrastructure.Registration;
using foodiya.app.Identity.Infrastructure.Tests.Authentication;
using MicrosoftOptions = Microsoft.Extensions.Options.Options;

namespace foodiya.app.Identity.Infrastructure.Tests.Accounts;

[TestClass]
public sealed class AccountLifecycleServiceTests
{
    private const string Password = "Initial!Password123";
    private const string NewPassword = "Replacement!Password456";
    private const string Email = "lifecycle@example.test";
    private readonly TestContext _testContext;
    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _services;
    private readonly TestTimeProvider _clock = new(DateTimeOffset.UtcNow);
    private readonly CapturingEmailSender _emailSender = new();
    private readonly CredentialRaceInterceptor _credentialRace = new();
    private readonly RegistrationOptions _otpOptions = new() { OtpPepper = "test-pepper-at-least-thirty-two-characters" };
    private AsyncServiceScope _scope;
    private IdentityDataContext _db = null!;
    private UserManager<ApplicationUser> _users = null!;
    private AccountLifecycleService _sut = null!;
    private IdentityAuthenticationService _auth = null!;
    private AccessSessionValidator _access = null!;
    private Guid _userId;
    private CancellationToken Ct => _testContext.CancellationToken;

    public AccountLifecycleServiceTests(TestContext testContext)
    {
        _testContext = testContext;
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<IdentityDataContext>(options => options.UseSqlite(_connection)
            .AddInterceptors(_credentialRace)
            .ReplaceService<IModelCustomizer, SqliteTestModelCustomizer>());
        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
        }).AddRoles<IdentityRole<Guid>>().AddEntityFrameworkStores<IdentityDataContext>();
        _services = services.BuildServiceProvider();
    }

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _scope = _services.CreateAsyncScope();
        _db = _scope.ServiceProvider.GetRequiredService<IdentityDataContext>();
        _users = _scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        await _db.Database.EnsureCreatedAsync(Ct);
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "lifecycle.user",
            Email = Email,
            FirstName = "Lifecycle",
            LastName = "User",
            EmailConfirmed = true,
            Status = AccountStatus.Active,
            CreatedAtUtc = _clock.GetUtcNow(),
            LockoutEnabled = true
        };
        Assert.IsTrue((await _users.CreateAsync(user, Password)).Succeeded);
        Assert.IsTrue((await _users.AddToRoleAsync(user, RoleNames.User)).Succeeded);
        _userId = user.Id;
        _access = new AccessSessionValidator(_db, _clock);
        _sut = new AccountLifecycleService(_users, _db, _access,
            new HmacOtpCodeService(MicrosoftOptions.Create(_otpOptions)), _emailSender,
            MicrosoftOptions.Create(_otpOptions), MicrosoftOptions.Create(new AccountLifecycleOptions()),
            _clock, NullLogger<AccountLifecycleService>.Instance);
        var tokenOptions = MicrosoftOptions.Create(new AuthenticationTokenOptions
        {
            Issuer = "lifecycle-tests",
            Audience = "lifecycle-client",
            RefreshTokenPepper = "test-refresh-pepper-at-least-thirty-two-characters"
        });
        _auth = new IdentityAuthenticationService(_users, _db,
            new JwtAccessTokenService(
                tokenOptions,
                new RsaSigningKeyProvider(
                    tokenOptions,
                    NullLogger<RsaSigningKeyProvider>.Instance,
                    new TestHostEnvironment()),
                _clock),
            new TokenCryptographyService(tokenOptions),
            new LoginTimingProtection(), new StubGoogleIdTokenValidator(), tokenOptions, _clock);
        _db.ChangeTracker.Clear();
    }

    [TestCleanup]
    public async Task CleanupAsync()
    {
        await _scope.DisposeAsync();
        await _services.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [TestMethod]
    public async Task LoginAsync_CredentialsChangedBeforeSessionTransaction_RejectsStalePasswordCheck()
    {
        _credentialRace.Armed = true;
        await Assert.ThrowsExactlyAsync<IdentityUnauthorizedException>(() => LoginAsync());
        Assert.IsFalse(_credentialRace.Armed);
        Assert.AreEqual(0, await _db.UserSessions.CountAsync(Ct));
        Assert.AreEqual(0, await _db.RefreshTokens.CountAsync(Ct));
    }

    [TestMethod]
    public async Task ResetPasswordAsync_HistoryLimit_RetainsFivePreviousHashes()
    {
        for (var i = 1; i <= 6; i++)
        {
            var request = await RecoveryAsync();
            await _sut.ResetPasswordAsync(request with { NewPassword = $"Distinct!Password{i}999" }, Ct);
            _clock.Advance(TimeSpan.FromMinutes(1));
        }

        _db.ChangeTracker.Clear();
        Assert.AreEqual(5, await _db.PasswordHistory.CountAsync(Ct));
        var reuse = await RecoveryAsync();
        var error = await Assert.ThrowsExactlyAsync<IdentityValidationException>(() =>
            _sut.ResetPasswordAsync(reuse with { NewPassword = "Distinct!Password1999" }, Ct));
        Assert.AreEqual("PASSWORD_REUSED", error.Code);
        _db.ChangeTracker.Clear();
        // The original password is outside the configured five-previous-password window now.
        await _sut.ResetPasswordAsync(reuse with { NewPassword = Password }, Ct);
        Assert.IsTrue(await _users.CheckPasswordAsync(await _db.Users.SingleAsync(Ct), Password));
    }

    [TestMethod]
    public async Task ConfirmDeletionAsync_BecameFinalSuperAdminAfterRequest_RechecksProtection()
    {
        var login = await LoginAsync();
        await _sut.RequestDeletionAsync(Principal(login), new() { CurrentPassword = Password }, Ct);
        var email = Assert.ContainsSingle(_emailSender.Messages);
        var user = await _db.Users.SingleAsync(Ct);
        Assert.IsTrue((await _users.AddToRoleAsync(user, RoleNames.SuperAdmin)).Succeeded);
        _db.ChangeTracker.Clear();
        var error = await Assert.ThrowsExactlyAsync<IdentityConflictException>(() => _sut.ConfirmDeletionAsync(
            Principal(login), new() { ChallengeId = email.Id, Code = email.Code }, Ct));
        Assert.AreEqual("LAST_SUPER_ADMIN_REQUIRED", error.Code);
        _db.ChangeTracker.Clear();
        Assert.AreEqual(AccountStatus.Active, (await _db.Users.SingleAsync(Ct)).Status);
        Assert.IsNull((await _db.OtpChallenges.SingleAsync(Ct)).ConsumedAtUtc);
    }

    [TestMethod]
    public async Task ForgotPasswordAsync_UnknownExistingAndCooldown_ReturnSameResponseAndOnlyOneEmail()
    {
        var unknown = await _sut.ForgotPasswordAsync(new() { Email = "unknown@example.test" }, Ct);
        var existing = await _sut.ForgotPasswordAsync(new() { Email = Email }, Ct);
        var cooldown = await _sut.ForgotPasswordAsync(new() { Email = Email }, Ct);

        Assert.AreEqual(unknown, existing);
        Assert.AreEqual(unknown, cooldown);
        Assert.HasCount(1, _emailSender.Messages);
        var otp = await _db.OtpChallenges.SingleAsync(Ct);
        Assert.AreNotEqual(_emailSender.Messages[0].Code, otp.CodeHash);
        Assert.AreEqual(OtpPurpose.PasswordReset, otp.Purpose);
        Assert.AreEqual(_userId, otp.UserId);
    }

    [TestMethod]
    public async Task ForgotPasswordAsync_DeliveryFailure_DoesNotDiscloseAccountExistence()
    {
        _emailSender.FailDelivery = true;
        var unknown = await _sut.ForgotPasswordAsync(new() { Email = "unknown@example.test" }, Ct);
        var existing = await _sut.ForgotPasswordAsync(new() { Email = Email }, Ct);
        Assert.AreEqual(unknown, existing);
    }

    [TestMethod]
    [DataRow(AccountStatus.Pending)]
    [DataRow(AccountStatus.Suspended)]
    [DataRow(AccountStatus.Deleted)]
    public async Task ForgotPasswordAsync_IneligibleAccount_DoesNotSendChallenge(AccountStatus status)
    {
        var user = await _db.Users.SingleAsync(Ct);
        user.Status = status;
        await _db.SaveChangesAsync(Ct);
        await _sut.ForgotPasswordAsync(new() { Email = Email }, Ct);
        Assert.IsEmpty(_emailSender.Messages);
        Assert.AreEqual(0, await _db.OtpChallenges.CountAsync(Ct));
    }

    [TestMethod]
    public async Task ResetPasswordAsync_ValidOtp_RevokesCredentialsAndConsumesChallenge()
    {
        var first = await LoginAsync();
        var second = await LoginAsync();
        var request = await RecoveryAsync();
        await _sut.ResetPasswordAsync(request, Ct);
        _db.ChangeTracker.Clear();

        var user = await _db.Users.SingleAsync(Ct);
        Assert.IsTrue(await _users.CheckPasswordAsync(user, NewPassword));
        Assert.IsFalse(await _users.CheckPasswordAsync(user, Password));
        Assert.IsNull(await _access.ValidateAsync(Principal(first), Ct));
        Assert.IsNull(await _access.ValidateAsync(Principal(second), Ct));
        Assert.AreEqual(0, await _db.RefreshTokens.CountAsync(token => token.RevokedAtUtc == null, Ct));
        var oldHash = await _db.PasswordHistory.SingleAsync(Ct);
        Assert.AreNotEqual(PasswordVerificationResult.Failed, _users.PasswordHasher.VerifyHashedPassword(user, oldHash.PasswordHash, Password));
        Assert.AreEqual(1, await _db.OutboxMessages.CountAsync(message => message.Type == "PasswordReset", Ct));
        await Assert.ThrowsExactlyAsync<IdentityValidationException>(() => _sut.ResetPasswordAsync(request, Ct));
        _db.ChangeTracker.Clear();
        await Assert.ThrowsExactlyAsync<IdentityUnauthorizedException>(() => _auth.RefreshAsync(new() { RefreshToken = first.RefreshToken }, null, Ct));
        _db.ChangeTracker.Clear();
        var fresh = await LoginAsync(NewPassword);
        Assert.IsNotNull(await _access.ValidateAsync(Principal(fresh), Ct));
    }

    [TestMethod]
    public async Task ResetPasswordAsync_WrongCodes_ExhaustAttemptsPersistently()
    {
        var request = await RecoveryAsync();
        var wrongCode = request.Code == "000000" ? "111111" : "000000";
        for (var i = 0; i < _otpOptions.OtpMaxAttempts; i++)
        {
            await Assert.ThrowsExactlyAsync<IdentityValidationException>(() => _sut.ResetPasswordAsync(request with { Code = wrongCode }, Ct));
            _db.ChangeTracker.Clear();
        }

        await Assert.ThrowsExactlyAsync<IdentityValidationException>(() => _sut.ResetPasswordAsync(request, Ct));
        var otp = await _db.OtpChallenges.SingleAsync(Ct);
        Assert.AreEqual(_otpOptions.OtpMaxAttempts, otp.FailedAttempts);
        Assert.IsNotNull(otp.InvalidatedAtUtc);
        Assert.AreEqual(0, await _db.PasswordHistory.CountAsync(Ct));
    }

    [TestMethod]
    public async Task ResetPasswordAsync_ExpiredOtp_LeavesPasswordUnchanged()
    {
        var request = await RecoveryAsync();
        _clock.Advance(TimeSpan.FromMinutes(_otpOptions.OtpLifetimeMinutes));
        await Assert.ThrowsExactlyAsync<IdentityValidationException>(() => _sut.ResetPasswordAsync(request, Ct));
        Assert.IsTrue(await _users.CheckPasswordAsync(await _db.Users.SingleAsync(Ct), Password));
    }

    [TestMethod]
    public async Task ResetPasswordAsync_ResentOtp_InvalidatesEarlierCode()
    {
        var first = await RecoveryAsync();
        _clock.Advance(TimeSpan.FromSeconds(_otpOptions.OtpResendCooldownSeconds));
        var second = await RecoveryAsync();
        await Assert.ThrowsExactlyAsync<IdentityValidationException>(() => _sut.ResetPasswordAsync(first, Ct));
        _db.ChangeTracker.Clear();
        await _sut.ResetPasswordAsync(second, Ct);
        Assert.IsTrue(await _users.CheckPasswordAsync(await _db.Users.SingleAsync(Ct), NewPassword));
    }

    [TestMethod]
    public async Task ResetPasswordAsync_InvalidPassword_RollsBackOtpConsumption()
    {
        var request = await RecoveryAsync();
        await Assert.ThrowsExactlyAsync<IdentityValidationException>(() => _sut.ResetPasswordAsync(request with { NewPassword = "weak" }, Ct));
        _db.ChangeTracker.Clear();
        Assert.IsNull((await _db.OtpChallenges.SingleAsync(Ct)).ConsumedAtUtc);
        await _sut.ResetPasswordAsync(request, Ct);
        Assert.IsTrue(await _users.CheckPasswordAsync(await _db.Users.SingleAsync(Ct), NewPassword));
    }

    [TestMethod]
    public async Task ChangePasswordAsync_PreviousOrCurrentPassword_IsRejectedWithoutRevokingSession()
    {
        var login = await LoginAsync();
        await _sut.ChangePasswordAsync(Principal(login), new() { CurrentPassword = Password, NewPassword = NewPassword }, Ct);
        var fresh = await LoginAsync(NewPassword);
        foreach (var rejected in new[] { Password, NewPassword })
        {
            var error = await Assert.ThrowsExactlyAsync<IdentityValidationException>(() => _sut.ChangePasswordAsync(
                Principal(fresh), new() { CurrentPassword = NewPassword, NewPassword = rejected }, Ct));
            Assert.AreEqual("PASSWORD_REUSED", error.Code);
            _db.ChangeTracker.Clear();
            Assert.IsNotNull(await _access.ValidateAsync(Principal(fresh), Ct));
        }
    }

    [TestMethod]
    public async Task ChangePasswordAsync_WrongPassword_PersistsLockoutAttempts()
    {
        var login = await LoginAsync();
        var request = new ChangePasswordRequest { CurrentPassword = "Wrong!Password123", NewPassword = NewPassword };
        for (var i = 0; i < 5; i++)
        {
            await Assert.ThrowsExactlyAsync<IdentityUnauthorizedException>(() => _sut.ChangePasswordAsync(Principal(login), request, Ct));
            _db.ChangeTracker.Clear();
        }

        await Assert.ThrowsExactlyAsync<IdentityLockedException>(() => _sut.ChangePasswordAsync(Principal(login), request, Ct));
        Assert.AreEqual(0, await _db.PasswordHistory.CountAsync(Ct));
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task LogoutAsync_RequestedScope_RevokesOnlySelectedSessions(bool allSessions)
    {
        var first = await LoginAsync();
        var second = await LoginAsync();
        await _sut.LogoutAsync(Principal(first), allSessions, Ct);
        _db.ChangeTracker.Clear();
        Assert.IsNull(await _access.ValidateAsync(Principal(first), Ct));
        var remaining = await _access.ValidateAsync(Principal(second), Ct);
        if (allSessions)
        {
            Assert.IsNull(remaining);
        }
        else
        {
            Assert.IsNotNull(remaining);
        }

        Assert.AreEqual(allSessions ? 0 : 1, await _db.RefreshTokens.CountAsync(token => token.RevokedAtUtc == null, Ct));
    }

    [TestMethod]
    public async Task ConfirmDeletionAsync_ValidConfirmation_RemovesCredentialsAndRevokesSessions()
    {
        var first = await LoginAsync();
        var second = await LoginAsync();
        var device = await _db.UserDevices.FirstAsync(Ct);
        device.FirebaseInstallationId = "firebase-installation";
        device.FirebaseInstallationUpdatedAtUtc = _clock.GetUtcNow();
        await _db.SaveChangesAsync(Ct);
        await _sut.RequestDeletionAsync(Principal(first), new() { CurrentPassword = Password }, Ct);
        var email = Assert.ContainsSingle(_emailSender.Messages);
        _db.ChangeTracker.Clear();
        await _sut.ConfirmDeletionAsync(Principal(first), new() { ChallengeId = email.Id, Code = email.Code }, Ct);
        _db.ChangeTracker.Clear();
        var user = await _db.Users.SingleAsync(Ct);
        Assert.AreEqual(AccountStatus.Deleted, user.Status);
        Assert.IsNotNull(user.DeletedAtUtc);
        Assert.IsNull(user.PasswordHash);
        Assert.AreNotEqual(Email, user.Email);
        Assert.IsFalse(user.EmailConfirmed);
        Assert.IsNull(await _users.FindByEmailAsync(Email));
        Assert.IsNull(await _access.ValidateAsync(Principal(first), Ct));
        Assert.IsNull(await _access.ValidateAsync(Principal(second), Ct));
        Assert.AreEqual(0, await _db.UserRoles.CountAsync(Ct));
        Assert.AreEqual(0, await _db.OtpChallenges.CountAsync(Ct));
        Assert.AreEqual(0, await _db.RefreshTokens.CountAsync(token => token.RevokedAtUtc == null, Ct));
        Assert.AreEqual(0, await _db.UserDevices.CountAsync(device => device.RevokedAtUtc == null, Ct));
        Assert.AreEqual(0, await _db.UserDevices.CountAsync(device => device.FirebaseInstallationId != null, Ct));
        var deletion = await _db.OutboxMessages.SingleAsync(message => message.Type == "UserDeleted", Ct);
        Assert.Contains(_userId.ToString(), deletion.Payload);
        Assert.DoesNotContain(Email, deletion.Payload);
        Assert.AreEqual(Email, _emailSender.DeletionConfirmationRecipient);
    }

    [TestMethod]
    public async Task RequestDeletionAsync_FinalSuperAdministrator_RejectsDeletion()
    {
        var user = await _db.Users.SingleAsync(Ct);
        Assert.IsTrue((await _users.AddToRoleAsync(user, RoleNames.SuperAdmin)).Succeeded);
        var login = await LoginAsync();
        var error = await Assert.ThrowsExactlyAsync<IdentityConflictException>(() => _sut.RequestDeletionAsync(
            Principal(login), new() { CurrentPassword = Password }, Ct));
        Assert.AreEqual("LAST_SUPER_ADMIN_REQUIRED", error.Code);
        Assert.IsEmpty(_emailSender.Messages);
    }

    [TestMethod]
    public async Task ConfirmDeletionAsync_RecoveryCodeCannotAuthorizeDeletion()
    {
        var login = await LoginAsync();
        var recovery = await RecoveryAsync();
        await Assert.ThrowsExactlyAsync<IdentityValidationException>(() => _sut.ConfirmDeletionAsync(
            Principal(login), new() { ChallengeId = recovery.ChallengeId, Code = recovery.Code }, Ct));
        Assert.AreEqual(AccountStatus.Active, (await _db.Users.SingleAsync(Ct)).Status);
    }

    [TestMethod]
    public async Task ConfirmDeletionAsync_ChallengeForAnotherUser_IsRejected()
    {
        var login = await LoginAsync();
        await _sut.RequestDeletionAsync(Principal(login), new() { CurrentPassword = Password }, Ct);
        var email = Assert.ContainsSingle(_emailSender.Messages);
        var other = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "another.user",
            Email = "another@example.test",
            FirstName = "Another",
            LastName = "User",
            Status = AccountStatus.Active,
            EmailConfirmed = true,
            CreatedAtUtc = _clock.GetUtcNow()
        };
        Assert.IsTrue((await _users.CreateAsync(other, Password)).Succeeded);
        var otherLogin = await LoginAsync(Password, other.Email);
        await Assert.ThrowsExactlyAsync<IdentityValidationException>(() => _sut.ConfirmDeletionAsync(
            Principal(otherLogin), new() { ChallengeId = email.Id, Code = email.Code }, Ct));
        _db.ChangeTracker.Clear();
        Assert.AreEqual(2, await _db.Users.CountAsync(user => user.Status == AccountStatus.Active, Ct));
        Assert.IsNull((await _db.OtpChallenges.SingleAsync(Ct)).ConsumedAtUtc);
    }

    private async Task<ResetPasswordRequest> RecoveryAsync()
    {
        _db.ChangeTracker.Clear();
        await _sut.ForgotPasswordAsync(new() { Email = Email }, Ct);
        var email = _emailSender.Messages.Last();
        _db.ChangeTracker.Clear();
        return new ResetPasswordRequest { ChallengeId = email.Id, Code = email.Code, NewPassword = NewPassword };
    }

    private async Task<AuthenticationTokenResponse> LoginAsync(string password = Password, string email = Email)
    {
        _db.ChangeTracker.Clear();
        var response = await _auth.LoginAsync(new()
        {
            Login = email,
            Password = password,
            DeviceInstallationId = Guid.NewGuid().ToString("N")
        }, "127.0.0.1", Ct);
        _db.ChangeTracker.Clear();
        return response;
    }

    private static ClaimsPrincipal Principal(AuthenticationTokenResponse login) =>
        new(new ClaimsIdentity(new JwtSecurityTokenHandler().ReadJwtToken(login.AccessToken).Claims, "Bearer"));

    private sealed class CapturingEmailSender : IIdentityEmailSender
    {
        public List<(Guid Id, string Code, OtpPurpose Purpose)> Messages { get; } = [];
        public bool FailDelivery { get; set; }
        public string? DeletionConfirmationRecipient { get; private set; }
        public Task SendAccountOtpAsync(string email, IdentityEmailContext context, OtpPurpose purpose, Guid challengeId, string code, DateTimeOffset expiresAtUtc, CancellationToken cancellationToken)
        {
            if (FailDelivery)
            {
                throw new IdentityDeliveryException("DELIVERY_FAILED", "Test delivery failure.");
            }

            Messages.Add((challengeId, code, purpose));
            return Task.CompletedTask;
        }

        public Task SendRegistrationOtpAsync(string email, IdentityEmailContext context, Guid challengeId, string code, DateTimeOffset expiresAtUtc, CancellationToken cancellationToken) =>
            throw new InvalidOperationException("Account lifecycle must not send registration codes.");

        public Task SendWelcomeAsync(string email, IdentityEmailContext context, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task SendAccountDeletionConfirmationAsync(string email, IdentityEmailContext context, CancellationToken cancellationToken)
        {
            DeletionConfirmationRecipient = email;
            return Task.CompletedTask;
        }
    }

    private sealed class CredentialRaceInterceptor : DbTransactionInterceptor
    {
        public bool Armed { get; set; }

        public override async ValueTask<DbTransaction> TransactionStartedAsync(
            DbConnection connection, TransactionEndEventData eventData, DbTransaction result,
            CancellationToken cancellationToken = default)
        {
            if (Armed)
            {
                Armed = false;
                // Simulate a credential update after the password read but before the new
                // transaction re-reads the user; the original entity is still tracked.
                await using var command = connection.CreateCommand();
                command.Transaction = result;
                command.CommandText = "UPDATE Users SET SecurityStamp = 'changed-by-concurrent-reset'";
                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            return result;
        }
    }
}
