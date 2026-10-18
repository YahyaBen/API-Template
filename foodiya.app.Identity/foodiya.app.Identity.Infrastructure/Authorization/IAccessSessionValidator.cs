using System.Security.Claims;

namespace foodiya.app.Identity.Infrastructure.Authorization;

internal interface IAccessSessionValidator
{
    Task<ValidatedAccessSession?> ValidateAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken);
}
