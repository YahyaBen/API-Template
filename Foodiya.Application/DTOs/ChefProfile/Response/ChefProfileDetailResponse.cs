namespace Foodiya.Application.DTOs.ChefProfile.Response;

public sealed class ChefProfileDetailResponse
{
    public Guid Uuid { get; set; }
    public Guid UserUuid { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? Specialty { get; set; }
    public int? YearsOfExperience { get; set; }
    public bool IsVerified { get; set; }
    public DateTime DateInsert { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public string Code { get; set; } = string.Empty;
    public string CodeAlpha { get; set; } = string.Empty;
}
