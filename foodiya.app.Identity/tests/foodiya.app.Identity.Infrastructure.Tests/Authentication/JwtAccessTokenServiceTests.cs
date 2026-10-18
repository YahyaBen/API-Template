using System.IdentityModel.Tokens.Jwt;
using foodiya.app.Identity.Domain.Enums;
using foodiya.app.Identity.Infrastructure.Authentication;
using foodiya.app.Identity.Infrastructure.Identity;
using foodiya.app.Identity.Infrastructure.Options;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using MicrosoftOptions = Microsoft.Extensions.Options.Options;

namespace foodiya.app.Identity.Infrastructure.Tests.Authentication;

[TestClass]
public sealed class JwtAccessTokenServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 4, 10, 30, 0, TimeSpan.Zero);
    private static readonly Guid UserId = Guid.Parse("11111111-aaaa-bbbb-cccc-222222222222");
    private static readonly Guid SessionId = Guid.Parse("22222222-aaaa-bbbb-cccc-333333333333");
    private static readonly Guid DeviceId = Guid.Parse("33333333-aaaa-bbbb-cccc-444444444444");

    private readonly RsaSigningKeyProvider _keyProvider;
    private readonly JwtAccessTokenService _sut;

    public JwtAccessTokenServiceTests()
    {
        var options = MicrosoftOptions.Create(CreateOptions());
        _keyProvider = new RsaSigningKeyProvider(
            options,
            NullLogger<RsaSigningKeyProvider>.Instance,
            new TestHostEnvironment());
        _sut = new JwtAccessTokenService(options, _keyProvider, new TestTimeProvider(Now));
    }

    [TestMethod]
    public void Create_ActiveUser_EmitsExpectedIdentityAndSessionClaims()
    {
        // Arrange
        var user = CreateUser();

        // Act
        var result = _sut.Create(
            user,
            ["USER", "CHEF"],
            SessionId,
            DeviceId,
            Now.AddDays(30));
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.Token);

        // Assert
        Assert.AreEqual("foodiya-app-identity-tests", jwt.Issuer);
        Assert.Contains("sandbox-clients", jwt.Audiences);
        Assert.AreEqual(UserId.ToString(), jwt.Subject);
        Assert.AreEqual("cook@example.test", jwt.Claims.Single(claim => claim.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.AreEqual("Ada Lovelace", jwt.Claims.Single(claim => claim.Type == "name").Value);
        Assert.AreEqual("ada", jwt.Claims.Single(claim => claim.Type == "preferred_username").Value);
        Assert.AreEqual(user.SecurityStamp, jwt.Claims.Single(claim => claim.Type == "sst").Value);
        Assert.AreEqual(SessionId.ToString(), jwt.Claims.Single(claim => claim.Type == "sid").Value);
        Assert.AreEqual(DeviceId.ToString(), jwt.Claims.Single(claim => claim.Type == "did").Value);
        Assert.AreSequenceEqual(
            new[] { "USER", "CHEF" },
            jwt.Claims.Where(claim => claim.Type == "role").Select(claim => claim.Value).ToArray());
        Assert.IsNotEmpty(jwt.Claims.Single(claim => claim.Type == JwtRegisteredClaimNames.Jti).Value);
        Assert.AreEqual(Now.AddMinutes(15), result.ExpiresAtUtc);
        Assert.AreEqual(result.ExpiresAtUtc.UtcDateTime, jwt.ValidTo);
        Assert.AreEqual(Now.UtcDateTime, jwt.ValidFrom);
    }

    [TestMethod]
    public void Create_SessionExpiresFirst_CapsAccessTokenAtSessionExpiry()
    {
        // Arrange
        var sessionExpiry = Now.AddMinutes(4);

        // Act
        var result = _sut.Create(
            CreateUser(),
            [],
            SessionId,
            DeviceId,
            sessionExpiry);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.Token);

        // Assert
        Assert.AreEqual(sessionExpiry, result.ExpiresAtUtc);
        Assert.AreEqual(sessionExpiry.UtcDateTime, jwt.ValidTo);
    }

    [TestMethod]
    public void Create_SameInputsTwice_UsesUniqueTokenIdentifiers()
    {
        // Arrange
        var user = CreateUser();

        // Act
        var first = new JwtSecurityTokenHandler().ReadJwtToken(
            _sut.Create(user, ["USER"], SessionId, DeviceId, Now.AddDays(1)).Token);
        var second = new JwtSecurityTokenHandler().ReadJwtToken(
            _sut.Create(user, ["USER"], SessionId, DeviceId, Now.AddDays(1)).Token);

        // Assert
        Assert.AreNotEqual(first.Id, second.Id);
    }

    [TestMethod]
    public void Create_TokenHeader_UsesRs256AndKeyId()
    {
        // Arrange
        var user = CreateUser();

        // Act
        var result = _sut.Create(user, ["USER"], SessionId, DeviceId, Now.AddDays(1));
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.Token);

        // Assert
        Assert.AreEqual(SecurityAlgorithms.RsaSha256, jwt.Header.Alg);
        Assert.AreEqual("foodiya-app-identity-v1", jwt.Header.Kid);
    }

    [TestMethod]
    public void Create_TokenSignature_ValidatesWithJwksPublicKey()
    {
        // Arrange
        var user = CreateUser();
        var token = _sut.Create(user, ["USER"], SessionId, DeviceId, Now.AddDays(1)).Token;
        var jwks = _keyProvider.GetPublicJwks();
        var jwk = jwks.Keys.Single();

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "foodiya-app-identity-tests",
            ValidateAudience = true,
            ValidAudience = "sandbox-clients",
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = jwk
        };

        // Act
        var principal = new JwtSecurityTokenHandler().ValidateToken(
            token,
            validationParameters,
            out var validatedToken);

        // Assert
        Assert.IsNotNull(principal);
        Assert.IsNotNull(validatedToken);
    }

    [TestMethod]
    public void GetPublicJwks_NeverExposesPrivateParameters()
    {
        // Act
        var jwks = _keyProvider.GetPublicJwks();
        var jwk = jwks.Keys.Single();

        // Assert
        Assert.AreEqual("RSA", jwk.Kty);
        Assert.AreEqual("sig", jwk.Use);
        Assert.AreEqual(SecurityAlgorithms.RsaSha256, jwk.Alg);
        Assert.IsFalse(string.IsNullOrWhiteSpace(jwk.N), "Modulus (n) must be present.");
        Assert.IsFalse(string.IsNullOrWhiteSpace(jwk.E), "Exponent (e) must be present.");
        Assert.IsTrue(string.IsNullOrWhiteSpace(jwk.D), "Private exponent (d) must NOT be exposed in JWKS.");
        Assert.IsTrue(string.IsNullOrWhiteSpace(jwk.DP), "DP must NOT be exposed in JWKS.");
        Assert.IsTrue(string.IsNullOrWhiteSpace(jwk.DQ), "DQ must NOT be exposed in JWKS.");
        Assert.IsTrue(string.IsNullOrWhiteSpace(jwk.P), "P must NOT be exposed in JWKS.");
        Assert.IsTrue(string.IsNullOrWhiteSpace(jwk.Q), "Q must NOT be exposed in JWKS.");
        Assert.IsTrue(string.IsNullOrWhiteSpace(jwk.QI), "QI must NOT be exposed in JWKS.");
    }

    [TestMethod]
    public void SigningKeyProvider_ProductionWithoutPrivateKey_FailsFast()
    {
        var options = MicrosoftOptions.Create(CreateOptions());

        var exception = Assert.ThrowsExactly<InvalidOperationException>(() =>
            new RsaSigningKeyProvider(
                options,
                NullLogger<RsaSigningKeyProvider>.Instance,
                new TestHostEnvironment(Environments.Production)));

        Assert.Contains("RsaPrivateKey", exception.Message);
    }

    private static AuthenticationTokenOptions CreateOptions() => new()
    {
        Issuer = "foodiya-app-identity-tests",
        Audience = "sandbox-clients",
        RefreshTokenPepper = "unit-test-refresh-token-pepper-32-characters",
        AccessTokenLifetimeMinutes = 15,
        RefreshTokenLifetimeDays = 30
    };

    private static ApplicationUser CreateUser() => new()
    {
        Id = UserId,
        UserName = "ada",
        Email = "cook@example.test",
        FirstName = "Ada",
        LastName = "Lovelace",
        EmailConfirmed = true,
        SecurityStamp = "security-stamp-001",
        Status = AccountStatus.Active,
        CreatedAtUtc = Now.AddDays(-1)
    };
}
