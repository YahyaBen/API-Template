namespace Foodiya.Application.Interfaces.Auth;

public interface IPasswordHasherService
{
    (byte[] hash, byte[] salt) HashPassword(string password);
    bool VerifyPassword(string password, byte[] passwordHash, byte[] passwordSalt);
}
