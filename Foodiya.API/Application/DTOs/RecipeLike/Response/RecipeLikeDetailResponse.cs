namespace Foodiya.Application.DTOs.RecipeLike.Response;

public sealed class RecipeLikeDetailResponse
{
    public Guid RecipeUuid { get; set; }
    public string RecipeTitle { get; set; } = string.Empty;
    public Guid UserUuid { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserFullName { get; set; } = string.Empty;
    public DateTime LikedAt { get; set; }
}
