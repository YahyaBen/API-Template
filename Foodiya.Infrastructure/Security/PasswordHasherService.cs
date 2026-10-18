using System.Security.Cryptography;
using Foodiya.Application.Interfaces.Auth;

namespace Foodiya.Infrastructure.Security;

public sealed class PasswordHasherService : IPasswordHasherService
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 210_000;

    public (byte[] hash, byte[] salt) HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA512,
            HashSize);

        return (hash, salt);
    }

    public bool VerifyPassword(string password, byte[] passwordHash, byte[] passwordSalt)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        if (passwordHash.Length == 0 || passwordSalt.Length == 0)
            return false;

        var computedHash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            passwordSalt,
            Iterations,
            HashAlgorithmName.SHA512,
            HashSize);

        return CryptographicOperations.FixedTimeEquals(computedHash, passwordHash);
    }
}
