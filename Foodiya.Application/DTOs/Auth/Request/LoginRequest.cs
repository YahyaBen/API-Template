using System.ComponentModel.DataAnnotations;

namespace Foodiya.Application.DTOs.Auth.Request;

public sealed class LoginRequest
{
    [Required, StringLength(150)]
    public string Identity { get; set; } = string.Empty;

    [Required, StringLength(128, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;
}
