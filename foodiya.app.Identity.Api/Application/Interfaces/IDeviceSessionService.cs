using System.Security.Claims;
using foodiya.app.Identity.Contracts.Accounts;
using foodiya.app.Identity.Contracts.Devices;

namespace foodiya.app.Identity.Application.Interfaces;

public interface IDeviceSessionService
{
    Task<DeviceListResponse> GetDevicesAsync(ClaimsPrincipal principal, int skip, int take, CancellationToken cancellationToken);
    Task<SessionListResponse> GetSessionsAsync(ClaimsPrincipal principal, int skip, int take, CancellationToken cancellationToken);
    Task RevokeDeviceAsync(ClaimsPrincipal principal, Guid deviceId, CancellationToken cancellationToken);
    Task RevokeSessionAsync(ClaimsPrincipal principal, Guid sessionId, CancellationToken cancellationToken);
    Task SetFirebaseInstallationAsync(ClaimsPrincipal principal, string? firebaseInstallationId, CancellationToken cancellationToken);
    Task<AccountChallengeResponse> RequestVerificationAsync(ClaimsPrincipal principal, CancellationToken cancellationToken);
    Task ConfirmVerificationAsync(ClaimsPrincipal principal, ConfirmDeviceVerificationRequest request, CancellationToken cancellationToken);
}
