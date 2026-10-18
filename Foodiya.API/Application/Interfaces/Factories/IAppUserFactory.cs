using Foodiya.Application.DTOs.Auth.Request;
using Foodiya.Domain.Models;

namespace Foodiya.Application.Interfaces.Factories;

public interface IAppUserFactory
{
    AppUser CreateFromRegistration(string normalizedUserName, string normalizedEmail, byte[] passwordHash, byte[] passwordSalt);
    AppUser CreateFromGoogle(string email, string? givenName, string? familyName, string? pictureUrl);
    AppUserExternalLogin CreateExternalLogin(int appUserId, string provider, string providerSubject, DateTime utcNow);
}
