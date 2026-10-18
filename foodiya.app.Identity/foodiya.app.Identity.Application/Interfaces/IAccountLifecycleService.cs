using System.Security.Claims;
using foodiya.app.Identity.Contracts.Accounts;

namespace foodiya.app.Identity.Application.Interfaces;

public interface IAccountLifecycleService
{
    Task<PasswordRecoveryResponse> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken);
    Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken);
    Task ChangePasswordAsync(ClaimsPrincipal principal, ChangePasswordRequest request, CancellationToken cancellationToken);
    Task LogoutAsync(ClaimsPrincipal principal, bool allSessions, CancellationToken cancellationToken);
    Task<AccountChallengeResponse> RequestDeletionAsync(ClaimsPrincipal principal, RequestAccountDeletionRequest request, CancellationToken cancellationToken);
    Task ConfirmDeletionAsync(ClaimsPrincipal principal, ConfirmAccountDeletionRequest request, CancellationToken cancellationToken);
}
