-- Script de suppression du schema FoodRecipeApp.
-- Compatible avec Azure SQL Database.
-- Supprime uniquement les tables de cette application.
-- A executer avant FoodRecipeApp_SQL.sql si vous voulez repartir de zero.

IF OBJECT_ID(N'[dbo].[RecipeShare]', N'U') IS NOT NULL
    DROP TABLE [dbo].[RecipeShare];
GO

IF OBJECT_ID(N'[dbo].[RecipeLike]', N'U') IS NOT NULL
    DROP TABLE [dbo].[RecipeLike];
GO

IF OBJECT_ID(N'[dbo].[RecipeNutrition]', N'U') IS NOT NULL
    DROP TABLE [dbo].[RecipeNutrition];
GO

IF OBJECT_ID(N'[dbo].[RecipeStep]', N'U') IS NOT NULL
    DROP TABLE [dbo].[RecipeStep];
GO

IF OBJECT_ID(N'[dbo].[RecipeIngredient]', N'U') IS NOT NULL
    DROP TABLE [dbo].[RecipeIngredient];
GO

IF OBJECT_ID(N'[dbo].[RecipeCategory]', N'U') IS NOT NULL
    DROP TABLE [dbo].[RecipeCategory];
GO

IF OBJECT_ID(N'[dbo].[RecipeImage]', N'U') IS NOT NULL
    DROP TABLE [dbo].[RecipeImage];
GO

IF OBJECT_ID(N'[dbo].[Recipe]', N'U') IS NOT NULL
    DROP TABLE [dbo].[Recipe];
GO

IF OBJECT_ID(N'[dbo].[IngredientImage]', N'U') IS NOT NULL
    DROP TABLE [dbo].[IngredientImage];
GO

IF OBJECT_ID(N'[dbo].[Ingredient]', N'U') IS NOT NULL
    DROP TABLE [dbo].[Ingredient];
GO

IF OBJECT_ID(N'[dbo].[IngredientType]', N'U') IS NOT NULL
    DROP TABLE [dbo].[IngredientType];
GO

IF OBJECT_ID(N'[dbo].[Unit]', N'U') IS NOT NULL
    DROP TABLE [dbo].[Unit];
GO

IF OBJECT_ID(N'[dbo].[MoroccanCity]', N'U') IS NOT NULL
    DROP TABLE [dbo].[MoroccanCity];
GO

IF OBJECT_ID(N'[dbo].[MoroccanRegion]', N'U') IS NOT NULL
    DROP TABLE [dbo].[MoroccanRegion];
GO

IF OBJECT_ID(N'[dbo].[Cuisine]', N'U') IS NOT NULL
    DROP TABLE [dbo].[Cuisine];
GO

IF OBJECT_ID(N'[dbo].[FoodCategory]', N'U') IS NOT NULL
    DROP TABLE [dbo].[FoodCategory];
GO

IF OBJECT_ID(N'[dbo].[Difficulty]', N'U') IS NOT NULL
    DROP TABLE [dbo].[Difficulty];
GO

IF OBJECT_ID(N'[dbo].[ChefProfile]', N'U') IS NOT NULL
    DROP TABLE [dbo].[ChefProfile];
GO

IF OBJECT_ID(N'[dbo].[AppUser]', N'U') IS NOT NULL
    DROP TABLE [dbo].[AppUser];
GO

IF OBJECT_ID(N'[dbo].[PresetAvatarImage]', N'U') IS NOT NULL
    DROP TABLE [dbo].[PresetAvatarImage];
GO
