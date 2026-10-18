using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Foodiya.Domain.Configuration;
using Foodiya.Application.DTOs.Auth.Response;
using Foodiya.Application.Interfaces.Auth;
using Foodiya.Domain.Interfaces.Core;
using Foodiya.Domain.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Foodiya.Infrastructure.Security;

public sealed class JwtAccessTokenService : IAccessTokenService
{
    private readonly JwtOptions _options;
    private readonly IDateTimeProvider _dateTimeProvider;

    public JwtAccessTokenService(IOptions<JwtOptions> options, IDateTimeProvider dateTimeProvider)
    {
        _options = options.Value;
        _dateTimeProvider = dateTimeProvider;
    }

    public AuthTokenResponse CreateToken(AppUser user)
    {
        var expiresAtUtc = _dateTimeProvider.UtcNow.AddMinutes(_options.AccessTokenLifetimeMinutes);
        var claims = BuildClaims(user);

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey)),
            SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: _dateTimeProvider.UtcNow,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new AuthTokenResponse
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(jwt),
            ExpiresAtUtc = expiresAtUtc,
            User = AuthUserResponse.FromAppUser(user)
        };
    }

    private static IReadOnlyCollection<Claim> BuildClaims(AppUser user)
    {
        return
        [
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("preferred_username", user.UserName),
            new Claim("given_name", user.FirstName),
            new Claim("family_name", user.LastName),
            new Claim("is_active", user.IsActive.ToString().ToLowerInvariant()),
            new Claim("is_chef", (user.ChefProfileUser is not null).ToString().ToLowerInvariant())
        ];
    }
}
