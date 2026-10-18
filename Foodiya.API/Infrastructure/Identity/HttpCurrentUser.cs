using System.IdentityModel.Tokens.Jwt;
using Foodiya.Application.Interfaces.Services;
using Foodiya.Domain.Constants;
using Foodiya.Domain.Exceptions;

namespace Foodiya.Infrastructure.Identity;

internal sealed class HttpCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public Guid UserUuid => accessor.HttpContext?.User is { Identity.IsAuthenticated: true } user
        && Guid.TryParse(user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var subject)
        && subject != Guid.Empty
            ? subject
            : throw new FoodiyaUnauthorizedException("A valid Identity subject is required.");

    public bool IsAdministrator => accessor.HttpContext?.User is { } user
        && (user.IsInRole(AppRoleConstants.Admin) || user.IsInRole(AppRoleConstants.SuperAdmin));
}
