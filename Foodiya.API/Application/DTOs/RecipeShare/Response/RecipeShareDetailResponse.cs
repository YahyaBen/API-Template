namespace Foodiya.Application.DTOs.RecipeShare.Response;

public sealed class RecipeShareDetailResponse
{
    public Guid Uuid { get; set; }
    public string Code { get; set; } = string.Empty;
    public Guid RecipeUuid { get; set; }
    public string RecipeTitle { get; set; } = string.Empty;
    public Guid SharedByUserUuid { get; set; }
    public string SharedByUserName { get; set; } = string.Empty;
    public string SharedByUserFullName { get; set; } = string.Empty;
    public Guid? SharedWithUserUuid { get; set; }
    public string? SharedWithUserName { get; set; }
    public string? SharedWithUserFullName { get; set; }
    public string ShareChannel { get; set; } = string.Empty;
    public string? ShareMessage { get; set; }
    public DateTime SharedAt { get; set; }
}
