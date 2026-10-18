namespace Foodiya.Domain.Models;

public sealed class ReferenceData
{
    public IReadOnlyCollection<ReferenceDataItem> Cuisines { get; init; } = [];

    public IReadOnlyCollection<ReferenceDataItem> Difficulties { get; init; } = [];

    public IReadOnlyCollection<ReferenceDataItem> FoodCategories { get; init; } = [];

    public IReadOnlyCollection<ReferenceDataItem> IngredientTypes { get; init; } = [];

    public IReadOnlyCollection<ReferenceDataItem> Units { get; init; } = [];

    public IReadOnlyCollection<ReferenceDataItem> MoroccanRegions { get; init; } = [];

    public IReadOnlyCollection<ReferenceDataItem> MoroccanCities { get; init; } = [];

    public IReadOnlyCollection<ReferenceDataItem> PresetAvatarImages { get; init; } = [];
}
