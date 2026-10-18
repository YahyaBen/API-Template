using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Foodiya.Application.DTOs.MoroccanCity.Request;

public sealed class CreateMoroccanCityRequest
{
    [Required]
    public Guid RegionUuid { get; set; }

    [JsonIgnore]
    public int RegionId { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
