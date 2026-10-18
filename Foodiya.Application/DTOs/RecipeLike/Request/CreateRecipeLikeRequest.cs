using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Foodiya.Application.DTOs.RecipeLike.Request;

public sealed class CreateRecipeLikeRequest
{
    [Required]
    public Guid RecipeUuid { get; set; }

    [JsonIgnore]
    public int RecipeId { get; set; }

    [Required]
    public Guid UserUuid { get; set; }

    [JsonIgnore]
    public int UserId { get; set; }
}
