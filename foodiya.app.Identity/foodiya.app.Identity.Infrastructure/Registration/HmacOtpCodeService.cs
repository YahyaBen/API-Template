using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using foodiya.app.Identity.Application.Interfaces;
using foodiya.app.Identity.Infrastructure.Options;

namespace foodiya.app.Identity.Infrastructure.Registration;

internal sealed class HmacOtpCodeService(IOptions<RegistrationOptions> options) : IOtpCodeService
{
    private readonly byte[] _pepper = Encoding.UTF8.GetBytes(options.Value.OtpPepper);

    public string GenerateCode() =>
        RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6", CultureInfo.InvariantCulture);

    public string HashCode(Guid challengeId, string code)
    {
        using var hmac = new HMACSHA256(_pepper);
        var value = Encoding.UTF8.GetBytes($"{challengeId:N}:{code}");
        return Convert.ToHexString(hmac.ComputeHash(value));
    }

    public bool VerifyCode(Guid challengeId, string code, string expectedHash)
    {
        try
        {
            var actual = Convert.FromHexString(HashCode(challengeId, code));
            var expected = Convert.FromHexString(expectedHash);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
