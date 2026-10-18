using System;
using System.Collections.Generic;

namespace Foodiya.Domain.Models;

public partial class Recipe
{
    public int Id { get; set; }

    public int ChefId { get; set; }

    public int DifficultyId { get; set; }

    public int? CuisineId { get; set; }

    public int? CityId { get; set; }

    public string Title { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? Summary { get; set; }

    public int PrepTimeMinutes { get; set; }

    public int CookTimeMinutes { get; set; }

    public int Servings { get; set; }

    public string? CoverImageUrl { get; set; }

    public string Status { get; set; } = null!;

    public string Visibility { get; set; } = null!;

    public DateTime DateInsert { get; set; }

    public DateTime? DateModif { get; set; }

    public DateTime? PublishedAt { get; set; }

    public bool IsActive { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedByUserId { get; set; }

    public string DeleteReason { get; set; } = null!;

    public string? Color { get; set; }

    public Guid Uuid { get; set; }

    public string Code { get; set; } = null!;

    public virtual ChefProfile Chef { get; set; } = null!;

    public virtual MoroccanCity? City { get; set; }

    public virtual Cuisine? Cuisine { get; set; }

    public virtual AppUser? DeletedByUser { get; set; }

    public virtual Difficulty Difficulty { get; set; } = null!;

    public virtual ICollection<RecipeImage> RecipeImages { get; set; } = new List<RecipeImage>();

    public virtual ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();

    public virtual ICollection<RecipeLike> RecipeLikes { get; set; } = new List<RecipeLike>();

    public virtual RecipeNutrition? RecipeNutrition { get; set; }

    public virtual ICollection<RecipeShare> RecipeShares { get; set; } = new List<RecipeShare>();

    public virtual ICollection<RecipeStep> RecipeSteps { get; set; } = new List<RecipeStep>();

    public virtual ICollection<FoodCategory> FoodCategories { get; set; } = new List<FoodCategory>();
}
