using System.Security.Claims;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using foodiya.app.Identity.Application.Exceptions;
using foodiya.app.Identity.Application.Interfaces;
using foodiya.app.Identity.Domain.Entities;
using foodiya.app.Identity.Domain.Enums;
using foodiya.app.Identity.Infrastructure.Authorization;
using foodiya.app.Identity.Infrastructure.Devices;
using foodiya.app.Identity.Infrastructure.Identity;
using foodiya.app.Identity.Infrastructure.Options;
using foodiya.app.Identity.Infrastructure.Persistence;
using foodiya.app.Identity.Infrastructure.Registration;
using foodiya.app.Identity.Infrastructure.Tests.Authentication;
using MicrosoftOptions = Microsoft.Extensions.Options.Options;

namespace foodiya.app.Identity.Infrastructure.Tests.Devices;

[TestClass]
public sealed class DeviceSessionServiceTests
{
    private readonly TestContext _context;
    private readonly SqliteConnection _connection;
    private readonly IdentityDataContext _db;
    private readonly TestTimeProvider _clock = new(DateTimeOffset.UtcNow);
    private readonly EmailCapture _email = new();
    private readonly DeviceSessionService _sut;
    private readonly AccessSessionValidator _validator;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _otherId = Guid.NewGuid();
    private readonly Guid _deviceId = Guid.NewGuid();
    private readonly Guid _secondDeviceId = Guid.NewGuid();
    private readonly Guid _foreignDeviceId = Guid.NewGuid();
    private readonly Guid _sessionId = Guid.NewGuid();
    private readonly Guid _secondSessionId = Guid.NewGuid();
    private readonly Guid _foreignSessionId = Guid.NewGuid();
    private CancellationToken Ct => _context.CancellationToken;

    public DeviceSessionServiceTests(TestContext context)
    {
        _context = context;
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        _db = new IdentityDataContext(new DbContextOptionsBuilder<IdentityDataContext>().UseSqlite(_connection)
            .ReplaceService<IModelCustomizer, SqliteTestModelCustomizer>().Options);
        _validator = new AccessSessionValidator(_db, _clock);
        var options = MicrosoftOptions.Create(new RegistrationOptions { OtpPepper = "device-test-pepper-at-least-thirty-two-characters" });
        _sut = new DeviceSessionService(_db, _validator, new HmacOtpCodeService(options), _email, options, _clock);
    }

    [TestInitialize]
    public async Task InitializeAsync()
    {
        await _db.Database.EnsureCreatedAsync(Ct);
        foreach (var id in new[] { _userId, _otherId })
        {
            _db.Users.Add(new ApplicationUser
            {
                Id = id,
                FirstName = "Test",
                LastName = "User",
                UserName = id.ToString(),
                NormalizedUserName = id.ToString().ToUpperInvariant(),
                Email = $"{id:N}@example.test",
                NormalizedEmail = $"{id:N}@example.test".ToUpperInvariant(),
                EmailConfirmed = true,
                Status = AccountStatus.Active,
                SecurityStamp = "device-test-stamp",
                CreatedAtUtc = _clock.GetUtcNow()
            });
        }

        AddDeviceAndSession(_userId, _deviceId, _sessionId);
        AddDeviceAndSession(_userId, _secondDeviceId, _secondSessionId);
        AddDeviceAndSession(_otherId, _foreignDeviceId, _foreignSessionId);
        await _db.SaveChangesAsync(Ct);
        _db.ChangeTracker.Clear();
    }

    [TestCleanup]
    public async Task CleanupAsync()
    {
        await _db.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [TestMethod]
    public async Task ListsAsync_OnlyOwnRecordsAndBoundedPages()
    {
        var devices = await _sut.GetDevicesAsync(Principal(), 0, 25, Ct);
        var sessions = await _sut.GetSessionsAsync(Principal(), 0, 25, Ct);
        Assert.HasCount(2, devices.Items);
        Assert.HasCount(2, sessions.Items);
        Assert.DoesNotContain(_foreignDeviceId, devices.Items.Select(item => item.Id));
        Assert.DoesNotContain(_foreignSessionId, sessions.Items.Select(item => item.Id));
        Assert.HasCount(1, devices.Items.Where(item => item.IsCurrent));
        Assert.HasCount(1, sessions.Items.Where(item => item.IsCurrent));
        Assert.HasCount(1, (await _sut.GetDevicesAsync(Principal(), 1, 1, Ct)).Items);
        await Assert.ThrowsExactlyAsync<IdentityValidationException>(() => _sut.GetDevicesAsync(Principal(), 0, 101, Ct));
        await Assert.ThrowsExactlyAsync<IdentityValidationException>(() => _sut.GetSessionsAsync(Principal(), -1, 25, Ct));
    }

    [TestMethod]
    public async Task RevokeAsync_ForeignResources_ReturnNotFoundAndPreserveState()
    {
        await Assert.ThrowsExactlyAsync<IdentityNotFoundException>(() => _sut.RevokeDeviceAsync(Principal(), _foreignDeviceId, Ct));
        await Assert.ThrowsExactlyAsync<IdentityNotFoundException>(() => _sut.RevokeSessionAsync(Principal(), _foreignSessionId, Ct));
        Assert.AreEqual(3, await _db.RefreshTokens.CountAsync(token => token.RevokedAtUtc == null, Ct));
        Assert.AreEqual(0, await _db.OutboxMessages.CountAsync(Ct));
    }

    [TestMethod]
    public async Task RevokeDeviceAsync_ClosesEverySessionAndClearsFirebaseInstallationAndOtp()
    {
        var secondPrincipal = Principal(_secondDeviceId, _secondSessionId);
        await _sut.SetFirebaseInstallationAsync(secondPrincipal, "firebase-installation", Ct);
        await _sut.RequestVerificationAsync(secondPrincipal, Ct);
        var additionalSessionId = Guid.NewGuid();
        AddSession(_userId, _secondDeviceId, additionalSessionId);
        await _db.SaveChangesAsync(Ct);
        _db.ChangeTracker.Clear();
        await _sut.RevokeDeviceAsync(Principal(), _secondDeviceId, Ct);
        _db.ChangeTracker.Clear();
        await _sut.RevokeDeviceAsync(Principal(), _secondDeviceId, Ct);
        Assert.IsNull(await _validator.ValidateAsync(secondPrincipal, Ct));
        Assert.IsNotNull(await _validator.ValidateAsync(Principal(), Ct));
        var device = await _db.UserDevices.SingleAsync(item => item.Id == _secondDeviceId, Ct);
        Assert.IsNull(device.FirebaseInstallationId);
        Assert.AreEqual(_clock.GetUtcNow(), device.FirebaseInstallationUpdatedAtUtc);
        Assert.IsNotNull(device.RevokedAtUtc);
        Assert.IsNotNull((await _db.OtpChallenges.SingleAsync(Ct)).InvalidatedAtUtc);
        Assert.AreEqual(0, await _db.RefreshTokens.CountAsync(token =>
            (token.SessionId == _secondSessionId || token.SessionId == additionalSessionId) && token.RevokedAtUtc == null, Ct));
        Assert.AreEqual(1, await _db.OutboxMessages.CountAsync(message => message.Type == "UserDeviceRevoked", Ct));
    }

    [TestMethod]
    public async Task RevokeSessionAsync_PreservesDeviceAndOtherSessions()
    {
        await _sut.RevokeSessionAsync(Principal(), _secondSessionId, Ct);
        Assert.IsNull(await _validator.ValidateAsync(Principal(_secondDeviceId, _secondSessionId), Ct));
        Assert.IsNotNull(await _validator.ValidateAsync(Principal(), Ct));
        Assert.IsNull((await _db.UserDevices.SingleAsync(device => device.Id == _secondDeviceId, Ct)).RevokedAtUtc);
        Assert.AreEqual(2, await _db.RefreshTokens.CountAsync(token => token.RevokedAtUtc == null, Ct));
    }

    [TestMethod]
    public async Task VerifyAsync_ValidOtp_RecordsTrustAndRejectsReplay()
    {
        var challenge = await _sut.RequestVerificationAsync(Principal(), Ct);
        Assert.AreNotEqual(_email.Code, (await _db.OtpChallenges.SingleAsync(Ct)).CodeHash);
        await _sut.ConfirmVerificationAsync(Principal(), new() { ChallengeId = challenge.ChallengeId, Code = _email.Code }, Ct);
        _db.ChangeTracker.Clear();
        Assert.AreEqual(_clock.GetUtcNow(), (await _db.UserDevices.SingleAsync(device => device.Id == _deviceId, Ct)).VerifiedAtUtc);
        await Assert.ThrowsExactlyAsync<IdentityValidationException>(() => _sut.ConfirmVerificationAsync(
            Principal(), new() { ChallengeId = challenge.ChallengeId, Code = _email.Code }, Ct));
        await Assert.ThrowsExactlyAsync<IdentityConflictException>(() => _sut.RequestVerificationAsync(Principal(), Ct));
        Assert.AreEqual(1, await _db.OutboxMessages.CountAsync(message => message.Type == "UserDeviceVerified", Ct));
    }

    [TestMethod]
    public async Task VerifyAsync_OtherDeviceAndOtherAccount_CannotConsumeCode()
    {
        var challenge = await _sut.RequestVerificationAsync(Principal(), Ct);
        var request = new foodiya.app.Identity.Contracts.Devices.ConfirmDeviceVerificationRequest { ChallengeId = challenge.ChallengeId, Code = _email.Code };
        await Assert.ThrowsExactlyAsync<IdentityValidationException>(() => _sut.ConfirmVerificationAsync(Principal(_secondDeviceId, _secondSessionId), request, Ct));
        await Assert.ThrowsExactlyAsync<IdentityValidationException>(() => _sut.ConfirmVerificationAsync(Principal(_foreignDeviceId, _foreignSessionId, _otherId), request, Ct));
        Assert.IsNull((await _db.OtpChallenges.SingleAsync(Ct)).ConsumedAtUtc);
    }

    [TestMethod]
    public async Task VerifyAsync_WrongCodes_ExhaustAttempts()
    {
        var challenge = await _sut.RequestVerificationAsync(Principal(), Ct);
        var wrong = _email.Code == "000000" ? "111111" : "000000";
        for (var i = 0; i < 5; i++)
        {
            await Assert.ThrowsExactlyAsync<IdentityValidationException>(() => _sut.ConfirmVerificationAsync(
                Principal(), new() { ChallengeId = challenge.ChallengeId, Code = wrong }, Ct));
            _db.ChangeTracker.Clear();
        }

        await Assert.ThrowsExactlyAsync<IdentityValidationException>(() => _sut.ConfirmVerificationAsync(
            Principal(), new() { ChallengeId = challenge.ChallengeId, Code = _email.Code }, Ct));
        Assert.AreEqual(5, (await _db.OtpChallenges.SingleAsync(Ct)).FailedAttempts);
    }

    [TestMethod]
    public async Task VerifyAsync_CooldownResendAndExpiry_AreEnforced()
    {
        var first = await _sut.RequestVerificationAsync(Principal(), Ct);
        var oldCode = _email.Code;
        await Assert.ThrowsExactlyAsync<IdentityThrottleException>(() => _sut.RequestVerificationAsync(Principal(), Ct));
        _clock.Advance(TimeSpan.FromMinutes(1));
        var second = await _sut.RequestVerificationAsync(Principal(), Ct);
        _db.ChangeTracker.Clear();
        await Assert.ThrowsExactlyAsync<IdentityValidationException>(() => _sut.ConfirmVerificationAsync(
            Principal(), new() { ChallengeId = first.ChallengeId, Code = oldCode }, Ct));
        _clock.Advance(TimeSpan.FromMinutes(10));
        await Assert.ThrowsExactlyAsync<IdentityValidationException>(() => _sut.ConfirmVerificationAsync(
            Principal(), new() { ChallengeId = second.ChallengeId, Code = _email.Code }, Ct));
        Assert.IsNull((await _db.UserDevices.SingleAsync(device => device.Id == _deviceId, Ct)).VerifiedAtUtc);
    }

    [TestMethod]
    public async Task FirebaseInstallationAsync_StoresClearsAndMovesWithinAccount_WithoutLeakingValue()
    {
        const string fid = "firebase-installation";
        await _sut.SetFirebaseInstallationAsync(Principal(), fid, Ct);
        await _sut.SetFirebaseInstallationAsync(Principal(_secondDeviceId, _secondSessionId), fid, Ct);
        Assert.AreEqual(1, await _db.UserDevices.CountAsync(device => device.FirebaseInstallationId == fid, Ct));
        Assert.AreEqual(_clock.GetUtcNow(), (await _db.UserDevices.SingleAsync(
            device => device.Id == _secondDeviceId, Ct)).FirebaseInstallationUpdatedAtUtc);
        var listing = await _sut.GetDevicesAsync(Principal(), 0, 25, Ct);
        Assert.DoesNotContain(fid, System.Text.Json.JsonSerializer.Serialize(listing));
        foreach (var payload in await _db.OutboxMessages.Select(message => message.Payload).ToListAsync(Ct))
        {
            Assert.DoesNotContain(fid, payload);
        }

        await _sut.SetFirebaseInstallationAsync(Principal(_secondDeviceId, _secondSessionId), null, Ct);
        Assert.AreEqual(0, await _db.UserDevices.CountAsync(device => device.FirebaseInstallationId != null, Ct));
    }

    [TestMethod]
    public async Task FirebaseInstallationAsync_CannotStealAnotherAccountInstallation()
    {
        const string fid = "foreign-firebase-installation";
        await _sut.SetFirebaseInstallationAsync(Principal(_foreignDeviceId, _foreignSessionId, _otherId), fid, Ct);
        await Assert.ThrowsExactlyAsync<IdentityConflictException>(() =>
            _sut.SetFirebaseInstallationAsync(Principal(), fid, Ct));
        Assert.AreEqual(fid, (await _db.UserDevices.SingleAsync(
            device => device.Id == _foreignDeviceId, Ct)).FirebaseInstallationId);
        await Assert.ThrowsExactlyAsync<IdentityValidationException>(() =>
            _sut.SetFirebaseInstallationAsync(Principal(), "   ", Ct));
        await Assert.ThrowsExactlyAsync<IdentityValidationException>(() =>
            _sut.SetFirebaseInstallationAsync(Principal(), new string('f', 129), Ct));
    }

    [TestMethod]
    public async Task RevokeCurrentDeviceAsync_InvalidatesAccessAndBlocksPushChanges()
    {
        await _sut.RevokeDeviceAsync(Principal(), _deviceId, Ct);
        _db.ChangeTracker.Clear();
        await Assert.ThrowsExactlyAsync<IdentityUnauthorizedException>(() =>
            _sut.SetFirebaseInstallationAsync(Principal(), "firebase-installation", Ct));
        await Assert.ThrowsExactlyAsync<IdentityUnauthorizedException>(() => _sut.RequestVerificationAsync(Principal(), Ct));
    }

    private void AddDeviceAndSession(Guid userId, Guid deviceId, Guid sessionId)
    {
        _db.UserDevices.Add(new UserDevice
        {
            Id = deviceId,
            UserId = userId,
            InstallationIdHash = deviceId.ToString("N").PadRight(64, '0'),
            FirstSeenAtUtc = _clock.GetUtcNow(),
            LastSeenAtUtc = _clock.GetUtcNow()
        });
        AddSession(userId, deviceId, sessionId);
    }

    private void AddSession(Guid userId, Guid deviceId, Guid sessionId)
    {
        _db.UserSessions.Add(new UserSession
        {
            Id = sessionId,
            UserId = userId,
            DeviceId = deviceId,
            CreatedAtUtc = _clock.GetUtcNow(),
            ExpiresAtUtc = _clock.GetUtcNow().AddDays(1)
        });
        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = userId,
            SessionId = sessionId,
            TokenHash = sessionId.ToString("N").PadRight(64, '0'),
            CreatedAtUtc = _clock.GetUtcNow(),
            ExpiresAtUtc = _clock.GetUtcNow().AddDays(1)
        });
    }

    private ClaimsPrincipal Principal(Guid? deviceId = null, Guid? sessionId = null, Guid? userId = null) => new(new ClaimsIdentity(
    [
        new Claim("sub", (userId ?? _userId).ToString()),
        new Claim("sid", (sessionId ?? _sessionId).ToString()),
        new Claim("did", (deviceId ?? _deviceId).ToString()),
        new Claim("sst", "device-test-stamp")
    ], "Bearer"));

    private sealed class EmailCapture : IIdentityEmailSender
    {
        public string Code { get; private set; } = string.Empty;
        public Task SendAccountOtpAsync(string email, IdentityEmailContext context, OtpPurpose purpose, Guid challengeId, string code, DateTimeOffset expiresAtUtc, CancellationToken cancellationToken)
        {
            Code = code;
            return Task.CompletedTask;
        }

        public Task SendRegistrationOtpAsync(string email, IdentityEmailContext context, Guid challengeId, string code, DateTimeOffset expiresAtUtc, CancellationToken cancellationToken) =>
            throw new InvalidOperationException("Unexpected registration email.");

        public Task SendWelcomeAsync(string email, IdentityEmailContext context, CancellationToken cancellationToken) =>
            throw new InvalidOperationException("Unexpected welcome email.");

        public Task SendAccountDeletionConfirmationAsync(string email, IdentityEmailContext context, CancellationToken cancellationToken) =>
            throw new InvalidOperationException("Unexpected account-deletion confirmation email.");
    }
}
