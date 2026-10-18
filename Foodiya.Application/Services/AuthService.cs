using System.Text.RegularExpressions;
using Foodiya.Domain.Constants;
using Foodiya.Application.DTOs.Auth.Request;
using Foodiya.Application.DTOs.Auth.Response;
using Foodiya.Domain.Exceptions;
using Foodiya.Application.Interfaces.Auth;
using Foodiya.Application.Interfaces.Factories;
using Foodiya.Application.Interfaces.Services;
using Foodiya.Domain.Interfaces.Core;
using Foodiya.Domain.Models;
using Foodiya.Domain.Specifications.AppUsers;
using Microsoft.Extensions.Logging;

namespace Foodiya.Application.Services;

public sealed class AuthService : IAuthService
{
    private static readonly Regex UsernameSanitizer = new("[^a-z0-9._]", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // ─── High-performance logger messages (zero allocation) ─────────────────
    private static readonly Action<ILogger, string, Exception?> _logRegistered =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(1001, "UserRegistered"),
            "[AUTH] User registered — UserId={UserId}");

    private static readonly Action<ILogger, string, string, Exception?> _logRegistrationConflict =
        LoggerMessage.Define<string, string>(LogLevel.Warning, new EventId(1002, "RegistrationConflict"),
            "[AUTH] Registration conflict — Field={Field} Value={Value}");

    private static readonly Action<ILogger, string, string, Exception?> _logLoginSuccess =
        LoggerMessage.Define<string, string>(LogLevel.Information, new EventId(1003, "LoginSuccess"),
            "[AUTH] Login success — UserId={UserId} Identity={Identity}");

    private static readonly Action<ILogger, string, Exception?> _logLoginFailedNotFound =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(1004, "LoginFailedNotFound"),
            "[AUTH] Login failed — identity not found Identity={Identity}");

    private static readonly Action<ILogger, string, Exception?> _logLoginFailedBadPassword =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(1005, "LoginFailedBadPassword"),
            "[AUTH] Login failed — wrong password UserId={UserId}");

    private static readonly Action<ILogger, string, Exception?> _logLoginBlockedInactive =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(1006, "LoginBlockedInactive"),
            "[AUTH] Sign-in blocked — account inactive UserId={UserId}");

    private static readonly Action<ILogger, string, Exception?> _logLoginBlockedDeleted =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(1007, "LoginBlockedDeleted"),
            "[AUTH] Sign-in blocked — account deleted UserId={UserId}");

    private static readonly Action<ILogger, string, Exception?> _logGoogleLoginExisting =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(1008, "GoogleLoginExisting"),
            "[AUTH] Google sign-in — existing linked account UserId={UserId}");

    private static readonly Action<ILogger, string, Exception?> _logGoogleLoginLinked =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(1009, "GoogleLoginLinked"),
            "[AUTH] Google sign-in — linked to existing email account UserId={UserId}");

    private static readonly Action<ILogger, string, Exception?> _logGoogleRegistered =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(1010, "GoogleRegistered"),
            "[AUTH] Google sign-in — new user registered UserId={UserId}");

    private static readonly Action<ILogger, string, Exception?> _logGoogleIdentityConflict =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(1011, "GoogleIdentityConflict"),
            "[AUTH] Google sign-in blocked — different Google identity already linked UserId={UserId}");

    private static readonly Action<ILogger, string, Exception?> _logGoogleEmailConflict =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(1012, "GoogleEmailConflict"),
            "[AUTH] Google sign-in blocked — email exists but Google not authoritative Email={Email}");

    private static readonly Action<ILogger, string, Exception?> _logTokenRefreshed =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(1013, "TokenRefreshed"),
            "[AUTH] Token refreshed — UserId={UserId}");

    private static readonly Action<ILogger, string, Exception?> _logTokenRefreshFailed =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(1014, "TokenRefreshFailed"),
            "[AUTH] Token refresh failed — malformed access token Value={TokenPreview}");

    private static readonly Action<ILogger, string, Exception?> _logRefreshTokenRevoked =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(1015, "RefreshTokenRevokeDetected"),
            "[AUTH] Refresh token reuse detected — all sessions revoked UserId={UserId}");
    // ────────────────────────────────────────────────────────────────────────

    private readonly IGenericRepository<AppUser> _appUserRepo;
    private readonly IAppUserExternalLoginRepository _externalLoginRepo;
    private readonly IGoogleIdTokenValidator _googleIdTokenValidator;
    private readonly IAccessTokenService _accessTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IPasswordHasherService _passwordHasherService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAppUserFactory _factory;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IGenericRepository<AppUser> appUserRepo,
        IAppUserExternalLoginRepository externalLoginRepo,
        IGoogleIdTokenValidator googleIdTokenValidator,
        IAccessTokenService accessTokenService,
        IRefreshTokenService refreshTokenService,
        IPasswordHasherService passwordHasherService,
        IDateTimeProvider dateTimeProvider,
        IAppUserFactory factory,
        ILogger<AuthService> logger)
    {
        _appUserRepo = appUserRepo;
        _externalLoginRepo = externalLoginRepo;
        _googleIdTokenValidator = googleIdTokenValidator;
        _accessTokenService = accessTokenService;
        _refreshTokenService = refreshTokenService;
        _passwordHasherService = passwordHasherService;
        _dateTimeProvider = dateTimeProvider;
        _factory = factory;
        _logger = logger;
    }

    public async Task<AuthTokenResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var normalizedEmail = NormalizeEmail(request.Email);
        var normalizedUserName = NormalizeUserName(request.UserName);

        EnsurePasswordStrength(request.Password);

        var conflict = _appUserRepo.GetAll()
            .Where(user => user.Email.ToLower() == normalizedEmail
                        || user.UserName.ToLower() == normalizedUserName.ToLower())
            .Select(user => new { user.Email, user.UserName })
            .FirstOrDefault();

        if (conflict is not null)
        {
            if (conflict.Email.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase))
            {
                _logRegistrationConflict(_logger, "Email", normalizedEmail, null);
                throw new FoodiyaValueAlreadyExistsException("An account with this email already exists.");
            }

            _logRegistrationConflict(_logger, "UserName", normalizedUserName, null);
            throw new FoodiyaValueAlreadyExistsException("This username is already taken.");
        }

        var (passwordHash, passwordSalt) = _passwordHasherService.HashPassword(request.Password);

        var user = _factory.CreateFromRegistration(normalizedUserName, normalizedEmail, passwordHash, passwordSalt);
        user.FirstName = NormalizeRequired(request.FirstName, nameof(request.FirstName));
        user.LastName = NormalizeRequired(request.LastName, nameof(request.LastName));
        user.DateInsert = _dateTimeProvider.UtcNow;

        await _appUserRepo.InsertAsync(user, ct);
        await _appUserRepo.SaveAsync(ct);

        var createdUser = await _appUserRepo.GetSingleAsync(new AppUserForAuthByIdSpecification(user.Id), ct)
            ?? user;

        _logRegistered(_logger, createdUser.Id.ToString(), null);
        return await CreateTokenPairAsync(createdUser, ct);
    }

    public async Task<AuthTokenResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var identity = NormalizeRequired(request.Identity, nameof(request.Identity));
        var user = await _appUserRepo.GetSingleAsync(new AppUserByIdentitySpecification(identity), ct);

        if (user is null)
        {
            _logLoginFailedNotFound(_logger, identity, null);
            throw new FoodiyaUnauthorizedException("Invalid credentials.");
        }

        EnsureUserCanSignIn(user);

        if (!_passwordHasherService.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            _logLoginFailedBadPassword(_logger, user.Id.ToString(), null);
            throw new FoodiyaUnauthorizedException("Invalid credentials.");
        }

        _logLoginSuccess(_logger, user.Id.ToString(), identity, null);
        return await CreateTokenPairAsync(user, ct);
    }

    public async Task<AuthTokenResponse> SignInWithGoogleAsync(GoogleSignInRequest request, CancellationToken ct = default)
    {
        var payload = await _googleIdTokenValidator.ValidateAsync(request.IdToken, ct);

        var existingLogin = await _externalLoginRepo.GetSingleAsync(
            new AppUserExternalLoginByProviderSubjectSpecification(AuthProviderConstants.Google, payload.Subject),
            ct);

        if (existingLogin is not null)
        {
            EnsureUserCanSignIn(existingLogin.AppUser);

            existingLogin.LastSignInAt = _dateTimeProvider.UtcNow;
            _externalLoginRepo.Update(existingLogin);
            await _externalLoginRepo.SaveAsync(ct);

            _logGoogleLoginExisting(_logger, existingLogin.AppUserId.ToString(), null);
            return await CreateTokenPairAsync(existingLogin.AppUser, ct);
        }

        var existingUser = await _appUserRepo.GetSingleAsync(new AppUserByEmailSpecification(payload.Email), ct);
        if (existingUser is not null)
        {
            EnsureUserCanSignIn(existingUser);

            var existingProviderLink = await _externalLoginRepo.GetSingleAsync(
                new AppUserExternalLoginByUserAndProviderSpecification(existingUser.Id, AuthProviderConstants.Google),
                ct);

            if (existingProviderLink is not null && existingProviderLink.ProviderSubject != payload.Subject)
            {
                _logGoogleIdentityConflict(_logger, existingUser.Id.ToString(), null);
                throw new FoodiyaForbiddenException("This account is already linked to a different Google identity.");
            }

            if (!IsGoogleAuthoritativeForEmail(payload))
            {
                _logGoogleEmailConflict(_logger, payload.Email, null);
                throw new FoodiyaForbiddenException("An account with this email already exists. Link Google from an authenticated session instead of auto-linking by email.");
            }

            var linkedLogin = _factory.CreateExternalLogin(
                existingUser.Id,
                AuthProviderConstants.Google,
                payload.Subject,
                _dateTimeProvider.UtcNow);

            await _externalLoginRepo.InsertAsync(linkedLogin, ct);
            await _externalLoginRepo.SaveAsync(ct);

            var linkedUser = await _appUserRepo.GetSingleAsync(new AppUserForAuthByIdSpecification(existingUser.Id), ct)
                ?? existingUser;

            _logGoogleLoginLinked(_logger, linkedUser.Id.ToString(), null);
            return await CreateTokenPairAsync(linkedUser, ct);
        }

        var newUser = _factory.CreateFromGoogle(payload.Email, payload.GivenName, payload.FamilyName, payload.PictureUrl);
        newUser.UserName = GenerateUniqueUserName(payload.Email);
        newUser.DateInsert = _dateTimeProvider.UtcNow;

        await using var transaction = await _appUserRepo.BeginTransactionAsync(ct);

        await _appUserRepo.InsertAsync(newUser, ct);
        await _appUserRepo.SaveAsync(ct);

        var newLogin = _factory.CreateExternalLogin(
            newUser.Id,
            AuthProviderConstants.Google,
            payload.Subject,
            _dateTimeProvider.UtcNow);

        await _externalLoginRepo.InsertAsync(newLogin, ct);
        await _externalLoginRepo.SaveAsync(ct);
        await transaction.CommitAsync(ct);

        var createdUser = await _appUserRepo.GetSingleAsync(new AppUserForAuthByIdSpecification(newUser.Id), ct)
            ?? newUser;

        _logGoogleRegistered(_logger, createdUser.Id.ToString(), null);
        return await CreateTokenPairAsync(createdUser, ct);
    }

    public async Task<AuthTokenResponse> RefreshAsync(RefreshTokenRequest request, CancellationToken ct = default)
    {
        var userId = ExtractUserIdFromExpiredAccessToken(request.AccessToken);

        var result = await _refreshTokenService.RotateRefreshTokenAsync(request.RefreshToken, userId, ct);

        _logTokenRefreshed(_logger, userId.ToString(), null);
        var tokenResponse = _accessTokenService.CreateToken(result.user);
        tokenResponse.RefreshToken = result.newRefreshToken;
        return tokenResponse;
    }

    public async Task<AuthUserResponse?> GetCurrentUserAsync(int userId, CancellationToken ct = default)
    {
        var user = await _appUserRepo.GetSingleAsync(new AppUserForAuthByIdSpecification(userId), ct);
        if (user is null || user.DeletedAt is not null)
            return null;

        return AuthUserResponse.FromAppUser(user);
    }

    private async Task<AuthTokenResponse> CreateTokenPairAsync(AppUser user, CancellationToken ct)
    {
        var tokenResponse = _accessTokenService.CreateToken(user);
        tokenResponse.RefreshToken = await _refreshTokenService.CreateRefreshTokenAsync(user.Id, ct);
        return tokenResponse;
    }

    private int ExtractUserIdFromExpiredAccessToken(string accessToken)
    {
        // JWT = header.payload.signature — decode the payload (base64url) without a library dependency
        var parts = accessToken.Split('.');
        if (parts.Length != 3)
        {
            var preview = accessToken.Length > 12 ? accessToken[..12] + "..." : accessToken;
            _logTokenRefreshFailed(_logger, preview, null);
            throw new FoodiyaUnauthorizedException("Invalid access token format.");
        }

        var payload = parts[1];
        // Base64url → Base64
        payload = payload.Replace('-', '+').Replace('_', '/');
        switch (payload.Length % 4)
        {
            case 2: payload += "=="; break;
            case 3: payload += "="; break;
        }

        var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(payload));
        var doc = System.Text.Json.JsonDocument.Parse(json);

        if (doc.RootElement.TryGetProperty("sub", out var sub) && int.TryParse(sub.GetString(), out var userId))
            return userId;

        throw new FoodiyaUnauthorizedException("Invalid access token — missing subject.");
    }

    private string GenerateUniqueUserName(string email)
    {
        var emailLocalPart = email.Split('@', 2)[0];
        var baseUserName = UsernameSanitizer.Replace(emailLocalPart.Trim().ToLowerInvariant(), string.Empty);
        if (string.IsNullOrWhiteSpace(baseUserName))
            baseUserName = "googleuser";

        baseUserName = baseUserName[..Math.Min(baseUserName.Length, 40)];

        var takenNames = _appUserRepo.GetAll()
            .Where(user => user.UserName.ToLower().StartsWith(baseUserName.ToLower()))
            .Select(user => user.UserName.ToLower())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (!takenNames.Contains(baseUserName))
            return baseUserName;

        var suffix = 1;
        string candidate;
        do
        {
            candidate = $"{baseUserName}{suffix}";
            candidate = candidate[..Math.Min(candidate.Length, 50)];
            suffix++;
        } while (takenNames.Contains(candidate));

        return candidate;
    }

    private static bool IsGoogleAuthoritativeForEmail(GoogleIdentityTokenPayload payload)
    {
        var email = payload.Email.Trim().ToLowerInvariant();
        return email.EndsWith("@gmail.com", StringComparison.Ordinal)
               || (payload.EmailVerified && !string.IsNullOrWhiteSpace(payload.HostedDomain));
    }

    private static void EnsureUserCanSignIn(AppUser user)
    {
        if (user.DeletedAt is not null)
            throw new FoodiyaForbiddenException("This account was deleted and cannot sign in.");

        if (!user.IsActive)
            throw new FoodiyaForbiddenException("This account is inactive and cannot sign in.");
    }

    private static void EnsurePasswordStrength(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            throw new FoodiyaBadRequestException("Password must be at least 8 characters long.");
    }

    private static string NormalizeRequired(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new FoodiyaBadRequestException($"{fieldName} cannot be empty.");

        return value.Trim();
    }

    private static string NormalizeEmail(string email)
    {
        return NormalizeRequired(email, nameof(email)).ToLowerInvariant();
    }

    private static string NormalizeUserName(string userName)
    {
        return NormalizeRequired(userName, nameof(userName));
    }
}
