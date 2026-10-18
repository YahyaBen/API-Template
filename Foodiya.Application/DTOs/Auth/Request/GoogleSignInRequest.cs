using System.ComponentModel.DataAnnotations;

namespace Foodiya.Application.DTOs.Auth.Request;

public sealed class GoogleSignInRequest
{
    [Required]
    public string IdToken { get; set; } = string.Empty;
}
