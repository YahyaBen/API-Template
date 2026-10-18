using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Foodiya.Domain.Constants;

namespace Foodiya.Application.DTOs.RecipeShare.Request;

public sealed class CreateRecipeShareRequest
{
    [Required]
    public Guid RecipeUuid { get; set; }

    [JsonIgnore]
    public int RecipeId { get; set; }

    [Required]
    public Guid SharedByUserUuid { get; set; }

    public Guid? SharedWithUserUuid { get; set; }

    [JsonIgnore]
    public int SharedByUserId { get; set; }

    [JsonIgnore]
    public int? SharedWithUserId { get; set; }

    [Required, StringLength(30)]
    [RegularExpression(ShareChannelConstants.ValidationPattern)]
    public string ShareChannel { get; set; } = string.Empty;

    [StringLength(500)]
    public string? ShareMessage { get; set; }
}
