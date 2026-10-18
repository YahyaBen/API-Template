using Foodiya.Application.Factories;
using Foodiya.Application.Services;
using Foodiya.Application.Interfaces.Factories;
using Foodiya.Application.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Foodiya.Application.Extensions
{
    public static partial class ServiceCollectionExtensions
    {
        public static void AddServices(this IServiceCollection services)
        {
            services.AddAutoMapper(_ => { }, typeof(ServiceCollectionExtensions).Assembly);

            services.AddScoped<IReferenceDataService, ReferenceDataService>();
            services.AddScoped<IRecipeService, RecipeService>();
            services.AddScoped<IChefProfileService, ChefProfileService>();
            services.AddScoped<ICuisineService, CuisineService>();
            services.AddScoped<IDifficultyService, DifficultyService>();
            services.AddScoped<IFoodCategoryService, FoodCategoryService>();
            services.AddScoped<IIngredientService, IngredientService>();
            services.AddScoped<IIngredientImageService, IngredientImageService>();
            services.AddScoped<IIngredientTypeService, IngredientTypeService>();
            services.AddScoped<IMoroccanCityService, MoroccanCityService>();
            services.AddScoped<IMoroccanRegionService, MoroccanRegionService>();
            services.AddScoped<IPresetAvatarImageService, PresetAvatarImageService>();
            services.AddScoped<IRecipeImageService, RecipeImageService>();
            services.AddScoped<IRecipeIngredientService, RecipeIngredientService>();
            services.AddScoped<IRecipeLikeService, RecipeLikeService>();
            services.AddScoped<IRecipeNutritionService, RecipeNutritionService>();
            services.AddScoped<IRecipeShareService, RecipeShareService>();
            services.AddScoped<IRecipeStepService, RecipeStepService>();
            services.AddScoped<IUnitService, UnitService>();
            services.AddScoped<IDailyRecipeStatService, DailyRecipeStatService>();

            // Factories
            services.AddSingleton<IRecipeFactory, RecipeFactory>();
            services.AddSingleton<IChefProfileFactory, ChefProfileFactory>();
            services.AddSingleton<ICuisineFactory, CuisineFactory>();
            services.AddSingleton<IDifficultyFactory, DifficultyFactory>();
            services.AddSingleton<IFoodCategoryFactory, FoodCategoryFactory>();
            services.AddSingleton<IIngredientFactory, IngredientFactory>();
            services.AddSingleton<IIngredientImageFactory, IngredientImageFactory>();
            services.AddSingleton<IIngredientTypeFactory, IngredientTypeFactory>();
            services.AddSingleton<IMoroccanCityFactory, MoroccanCityFactory>();
            services.AddSingleton<IMoroccanRegionFactory, MoroccanRegionFactory>();
            services.AddSingleton<IPresetAvatarImageFactory, PresetAvatarImageFactory>();
            services.AddSingleton<IRecipeImageFactory, RecipeImageFactory>();
            services.AddSingleton<IRecipeIngredientFactory, RecipeIngredientFactory>();
            services.AddSingleton<IRecipeLikeFactory, RecipeLikeFactory>();
            services.AddSingleton<IRecipeNutritionFactory, RecipeNutritionFactory>();
            services.AddSingleton<IRecipeShareFactory, RecipeShareFactory>();
            services.AddSingleton<IRecipeStepFactory, RecipeStepFactory>();
            services.AddSingleton<IUnitFactory, UnitFactory>();
        }
    }
}
