namespace Foodiya.Application.DTOs.Auth.Response;

public sealed class AuthTokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public AuthUserResponse User { get; set; } = new();
}
