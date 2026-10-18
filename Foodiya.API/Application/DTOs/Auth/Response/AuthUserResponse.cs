using Foodiya.Domain.Models;

namespace Foodiya.Application.DTOs.Auth.Response;

public sealed class AuthUserResponse
{
    public Guid Uuid { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public bool IsActive { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsChef { get; set; }
    public Guid? ChefProfileUuid { get; set; }

    public static AuthUserResponse FromAppUser(AppUser user)
    {
        return new AuthUserResponse
        {
            Uuid = user.Uuid,
            UserName = user.UserName,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            ProfileImageUrl = user.ProfileImageUrl,
            IsActive = user.IsActive,
            Role = user.Role,
            IsChef = user.ChefProfileUser is not null,
            ChefProfileUuid = user.ChefProfileUser?.Uuid
        };
    }
}
