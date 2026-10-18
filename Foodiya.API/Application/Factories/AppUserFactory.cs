using Foodiya.Application.Interfaces.Factories;
using Foodiya.Domain.Constants;
using Foodiya.Domain.Extensions;
using Foodiya.Domain.Interfaces.Core;
using Foodiya.Domain.Models;
using static Foodiya.Application.Factories.Helpers.EntityNormalizationHelper;

namespace Foodiya.Application.Factories;

public sealed class AppUserFactory : IAppUserFactory
{
    public AppUser CreateFromRegistration(
        string normalizedUserName,
        string normalizedEmail,
        byte[] passwordHash,
        byte[] passwordSalt) => new()
    {
        UserName = normalizedUserName,
        FirstName = Required(normalizedUserName, nameof(normalizedUserName)),
        LastName = string.Empty,
        Email = normalizedEmail,
        PasswordHash = passwordHash,
        PasswordSalt = passwordSalt,
        Role = AppRoleConstants.User,
        IsActive = true,
        DeleteReason = string.Empty,
        Code = EntityCodeGenerator.For("USR")
    };

    public AppUser CreateFromGoogle(
        string email,
        string? givenName,
        string? familyName,
        string? pictureUrl) => new()
    {
        Email = email.Trim().ToLowerInvariant(),
        FirstName = string.IsNullOrWhiteSpace(givenName) ? "Google" : givenName.Trim(),
        LastName = string.IsNullOrWhiteSpace(familyName) ? "User" : familyName.Trim(),
        PasswordHash = [],
        PasswordSalt = [],
        ProfileImageUrl = Optional(pictureUrl),
        Role = AppRoleConstants.User,
        IsActive = true,
        DeleteReason = string.Empty,
        Code = EntityCodeGenerator.For("USR")
    };

    public AppUserExternalLogin CreateExternalLogin(
        int appUserId,
        string provider,
        string providerSubject,
        DateTime utcNow) => new()
    {
        AppUserId = appUserId,
        Provider = provider,
        ProviderSubject = providerSubject,
        DateInsert = utcNow,
        LastSignInAt = utcNow
    };
}
