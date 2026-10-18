using Foodiya.Application.DTOs.Auth.Request;
using Foodiya.Application.DTOs.Auth.Response;

namespace Foodiya.Application.Interfaces.Services;

public interface IAuthService
{
    Task<AuthTokenResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<AuthTokenResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<AuthTokenResponse> SignInWithGoogleAsync(GoogleSignInRequest request, CancellationToken ct = default);
    Task<AuthTokenResponse> RefreshAsync(RefreshTokenRequest request, CancellationToken ct = default);
    Task<AuthUserResponse?> GetCurrentUserAsync(int userId, CancellationToken ct = default);
}
