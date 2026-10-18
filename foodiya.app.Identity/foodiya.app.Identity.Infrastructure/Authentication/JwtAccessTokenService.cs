using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using foodiya.app.Identity.Infrastructure.Identity;
using foodiya.app.Identity.Infrastructure.Options;

namespace foodiya.app.Identity.Infrastructure.Authentication;

internal sealed class JwtAccessTokenService(
    IOptions<AuthenticationTokenOptions> options,
    IRsaSigningKeyProvider rsaSigningKeyProvider,
    TimeProvider timeProvider) : IAccessTokenService
{
    private readonly AuthenticationTokenOptions _options = options.Value;

    public (string Token, DateTimeOffset ExpiresAtUtc) Create(
        ApplicationUser user,
        IReadOnlyCollection<string> roles,
        Guid sessionId,
        Guid deviceId,
        DateTimeOffset sessionExpiresAtUtc)
    {
        var now = timeProvider.GetUtcNow();
        var configuredExpiration = now.AddMinutes(_options.AccessTokenLifetimeMinutes);
        var expiresAt = configuredExpiration < sessionExpiresAtUtc
            ? configuredExpiration
            : sessionExpiresAtUtc;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new("name", $"{user.FirstName} {user.LastName}".Trim()),
            new(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new("preferred_username", user.UserName ?? string.Empty),
            new("sst", user.SecurityStamp ?? string.Empty),
            new("sid", sessionId.ToString()),
            new("did", deviceId.ToString())
        };
        claims.AddRange(roles.Select(role => new Claim("role", role)));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            Subject = new ClaimsIdentity(claims),
            NotBefore = now.UtcDateTime,
            IssuedAt = now.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            SigningCredentials = new SigningCredentials(
                rsaSigningKeyProvider.GetSigningKey(),
                SecurityAlgorithms.RsaSha256)
        };

        var handler = new JwtSecurityTokenHandler();
        return (handler.WriteToken(handler.CreateToken(descriptor)), expiresAt);
    }
}
