using System.ComponentModel.DataAnnotations;

namespace Foodiya.Application.DTOs.ChefProfile.Request;

public sealed class CreateChefProfileRequest
{
    [Required, StringLength(120)]
    public string DisplayName { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Bio { get; set; }

    [StringLength(150)]
    public string? Specialty { get; set; }

    [Range(0, 100)]
    public int? YearsOfExperience { get; set; }
}
