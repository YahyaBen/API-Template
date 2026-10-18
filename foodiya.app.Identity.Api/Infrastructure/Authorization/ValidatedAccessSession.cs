using System.Security.Claims;
using foodiya.app.Identity.Domain.Entities;
using foodiya.app.Identity.Infrastructure.Identity;

namespace foodiya.app.Identity.Infrastructure.Authorization;

internal sealed record ValidatedAccessSession(
    ApplicationUser User,
    UserSession Session,
    UserDevice Device);

// Stored only on the HTTP request that authenticated this principal.
internal sealed record AuthenticatedAccessSession(
    ClaimsPrincipal Principal,
    ValidatedAccessSession Access);
