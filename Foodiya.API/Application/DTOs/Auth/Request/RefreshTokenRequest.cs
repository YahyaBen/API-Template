using System.ComponentModel.DataAnnotations;

namespace Foodiya.Application.DTOs.Auth.Request;

public sealed class RefreshTokenRequest
{
    [Required]
    public string AccessToken { get; set; } = string.Empty;

    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
