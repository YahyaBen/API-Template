using AutoMapper;
using Foodiya.Application.DTOs.Recipe.Response;
using Foodiya.Domain.Models;

namespace Foodiya.Application.Profiles;

public sealed class RecipeProfile : Profile
{
    public RecipeProfile()
    {
        // Recipe → RecipeDetailResponse
        CreateMap<Recipe, RecipeDetailResponse>()
            .ForMember(d => d.TotalTimeMinutes, o => o.MapFrom(s => s.PrepTimeMinutes + s.CookTimeMinutes))
            .ForMember(d => d.DifficultyName, o => o.MapFrom(s => s.Difficulty.Name))
            .ForMember(d => d.CuisineName, o => o.MapFrom(s => s.Cuisine != null ? s.Cuisine.Name : null))
            .ForMember(d => d.CityName, o => o.MapFrom(s => s.City != null ? s.City.Name : null))
            .ForMember(d => d.ChefDisplayName, o => o.MapFrom(s => s.Chef.DisplayName))
            .ForMember(d => d.ChefUuid, o => o.MapFrom(s => s.Chef.Uuid))
            .ForMember(d => d.LikesCount, o => o.MapFrom(s => s.RecipeLikes.Count))
            .ForMember(d => d.Categories, o => o.MapFrom(s => s.FoodCategories.Select(c => c.Name)))
            .ForMember(d => d.Steps, o => o.MapFrom(s => s.RecipeSteps.OrderBy(st => st.StepNumber)))
            .ForMember(d => d.Ingredients, o => o.MapFrom(s => s.RecipeIngredients.OrderBy(ri => ri.SortOrder)))
            .ForMember(d => d.Images, o => o.MapFrom(s => s.RecipeImages.OrderBy(ri => ri.SortOrder)))
            .ForMember(d => d.Nutrition, o => o.MapFrom(s => s.RecipeNutrition));

        // Child mappings
        CreateMap<RecipeStep, RecipeStepResponse>();
        CreateMap<RecipeIngredient, RecipeIngredientResponse>()
            .ForMember(d => d.IngredientName, o => o.MapFrom(s => s.Ingredient.Name))
            .ForMember(d => d.UnitLabel, o => o.MapFrom(s => s.Unit != null ? s.Unit.Label : null));
        CreateMap<RecipeImage, RecipeImageResponse>();
        CreateMap<RecipeNutrition, RecipeNutritionResponse>();
    }
}
