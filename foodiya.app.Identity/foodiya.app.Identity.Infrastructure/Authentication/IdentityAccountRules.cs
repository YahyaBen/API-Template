using foodiya.app.Identity.Application.Exceptions;
using foodiya.app.Identity.Domain.Enums;
using foodiya.app.Identity.Infrastructure.Identity;

namespace foodiya.app.Identity.Infrastructure.Authentication;

internal static class IdentityAccountRules
{
    public static void EnsureCanAuthenticate(ApplicationUser user)
    {
        if (!user.EmailConfirmed || user.Status == AccountStatus.Pending)
        {
            throw new IdentityForbiddenException(
                "EMAIL_NOT_CONFIRMED",
                "Confirm the email address before signing in.");
        }

        if (user.Status != AccountStatus.Active)
        {
            throw new IdentityForbiddenException(
                "ACCOUNT_NOT_ACTIVE",
                "This account is not active.");
        }
    }

    public static bool CanAuthenticate(ApplicationUser user) =>
        user.EmailConfirmed && user.Status == AccountStatus.Active;
}
