using System.ComponentModel.DataAnnotations;

namespace Foodiya.Application.DTOs.FoodCategory.Request;

public sealed class UpdateFoodCategoryRequest
{
    [StringLength(100)]
    public string? Name { get; set; }

    [StringLength(250)]
    public string? Description { get; set; }

    [StringLength(30)]
    public string? Code { get; set; }

    [Range(0, int.MaxValue)]
    public int? SortOrder { get; set; }

    [StringLength(500)]
    public string? IconUrl { get; set; }

    [StringLength(20)]
    public string? Color { get; set; }

    public bool? IsActive { get; set; }
}
