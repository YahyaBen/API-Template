using System.Security.Claims;
using foodiya.app.Identity.Contracts.Users;

namespace foodiya.app.Identity.Application.Interfaces;

public interface ICurrentIdentityService
{
    Task<CurrentIdentityResponse> GetAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken);
}
