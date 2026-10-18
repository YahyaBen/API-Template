using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Foodiya.Domain.Constants;

namespace Foodiya.Application.DTOs.RecipeShare.Request;

public sealed class UpdateRecipeShareRequest
{
    public Guid? SharedWithUserUuid { get; set; }

    [JsonIgnore]
    public int? SharedWithUserId { get; set; }

    public bool ClearSharedWithUser { get; set; }

    [StringLength(30)]
    [RegularExpression(ShareChannelConstants.ValidationPattern)]
    public string? ShareChannel { get; set; }

    [StringLength(500)]
    public string? ShareMessage { get; set; }

    public bool ClearShareMessage { get; set; }
}
