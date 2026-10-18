namespace Foodiya.Application.DTOs.ReferenceData.Response;

public sealed class ReferenceDataResponse
{
    public IReadOnlyCollection<ReferenceDataItemResponse> Cuisines { get; set; } = [];

    public IReadOnlyCollection<ReferenceDataItemResponse> Difficulties { get; set; } = [];

    public IReadOnlyCollection<ReferenceDataItemResponse> FoodCategories { get; set; } = [];

    public IReadOnlyCollection<ReferenceDataItemResponse> IngredientTypes { get; set; } = [];

    public IReadOnlyCollection<ReferenceDataItemResponse> Units { get; set; } = [];

    public IReadOnlyCollection<ReferenceDataItemResponse> MoroccanRegions { get; set; } = [];

    public IReadOnlyCollection<ReferenceDataItemResponse> MoroccanCities { get; set; } = [];

    public IReadOnlyCollection<ReferenceDataItemResponse> PresetAvatarImages { get; set; } = [];
}
