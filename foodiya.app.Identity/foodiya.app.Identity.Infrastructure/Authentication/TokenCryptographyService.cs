using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using foodiya.app.Identity.Infrastructure.Options;

namespace foodiya.app.Identity.Infrastructure.Authentication;

internal sealed class TokenCryptographyService(IOptions<AuthenticationTokenOptions> options)
    : ITokenCryptographyService
{
    private readonly byte[] _pepper = Encoding.UTF8.GetBytes(options.Value.RefreshTokenPepper);

    public string GenerateRefreshToken() =>
        WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(64));

    public string HashRefreshToken(string token) => Hash($"refresh:{token}");

    public string HashDeviceInstallationId(string installationId) =>
        Hash($"device:{installationId.Trim()}");

    private string Hash(string value)
    {
        using var hmac = new HMACSHA256(_pepper);
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(value)));
    }
}
