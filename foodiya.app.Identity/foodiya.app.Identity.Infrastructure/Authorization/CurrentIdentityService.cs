using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using foodiya.app.Identity.Application.Exceptions;
using foodiya.app.Identity.Application.Interfaces;
using foodiya.app.Identity.Contracts.Users;
using foodiya.app.Identity.Infrastructure.Identity;

namespace foodiya.app.Identity.Infrastructure.Authorization;

internal sealed class CurrentIdentityService(
    IAccessSessionValidator accessSessionValidator,
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor) : ICurrentIdentityService
{
    public async Task<CurrentIdentityResponse> GetAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken)
    {
        var authenticated = httpContextAccessor.HttpContext?.Features.Get<AuthenticatedAccessSession>();
        var access = authenticated is not null && ReferenceEquals(authenticated.Principal, principal)
            ? authenticated.Access
            : await accessSessionValidator.ValidateAsync(principal, cancellationToken);
        if (access is null)
        {
            throw new IdentityUnauthorizedException(
                "ACCESS_SESSION_INVALID",
                "The authenticated session is no longer valid.");
        }
        var roles = (await userManager.GetRolesAsync(access.User))
            .Order(StringComparer.Ordinal)
            .ToArray();

        return new CurrentIdentityResponse(
            access.User.Id,
            access.User.UserName ?? string.Empty,
            access.User.Email ?? string.Empty,
            access.User.EmailConfirmed,
            access.User.FirstName,
            access.User.LastName,
            access.User.PreferredLanguage,
            access.User.Status.ToString(),
            roles,
            access.Session.Id,
            access.Session.CreatedAtUtc,
            access.Session.ExpiresAtUtc,
            access.Device.Id,
            access.Device.Platform,
            access.Device.DeviceName,
            access.Device.OsVersion,
            access.Device.AppVersion);
    }
}
