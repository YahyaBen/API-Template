using foodiya.app.Identity.Contracts.Authentication;

namespace foodiya.app.Identity.Application.Interfaces;

public interface IIdentityAuthenticationService
{
    Task<AuthenticationTokenResponse> LoginAsync(
        LoginRequest request,
        string? ipAddress,
        CancellationToken cancellationToken);

    Task<AuthenticationTokenResponse> SignInWithGoogleAsync(
        GoogleSignInRequest request,
        string? ipAddress,
        CancellationToken cancellationToken);

    Task<AuthenticationTokenResponse> RefreshAsync(
        RefreshAccessTokenRequest request,
        string? ipAddress,
        CancellationToken cancellationToken);
}
