-- ============================================================================
-- Migration: Add ImageUrl to All Tables
-- Date:      2026-04-09
-- Purpose:   Add an ImageUrl column to every table that does not already have
--            one.  Existing NULL values in tables that already carry an image
--            column are back-filled with a sensible Unsplash default.
-- Notes:     • Fully idempotent — safe to re-run.
--            • No transactions — each statement separated by GO.
--            • Default URLs point to Unsplash Source (royalty-free).
-- ============================================================================

-- ─────────────────────────────────────────────────────────────────────────────
-- STEP 1 — Add ImageUrl to tables that DO NOT have any image column yet
-- ─────────────────────────────────────────────────────────────────────────────

-- ChefProfile
IF COL_LENGTH(N'[dbo].[ChefProfile]', N'ImageUrl') IS NULL
BEGIN
    ALTER TABLE [dbo].[ChefProfile]
    ADD [ImageUrl] [varchar](500) NOT NULL
        CONSTRAINT [DF_ChefProfile_ImageUrl]
        DEFAULT ('https://images.unsplash.com/photo-1577219491135-ce391730fb2c?w=400&fit=crop');
END;
GO

-- MoroccanRegion
IF COL_LENGTH(N'[dbo].[MoroccanRegion]', N'ImageUrl') IS NULL
BEGIN
    ALTER TABLE [dbo].[MoroccanRegion]
    ADD [ImageUrl] [varchar](500) NOT NULL
        CONSTRAINT [DF_MoroccanRegion_ImageUrl]
        DEFAULT ('https://images.unsplash.com/photo-1548017544-2e7a30ae42ca?w=400&fit=crop');
END;
GO

-- MoroccanCity
IF COL_LENGTH(N'[dbo].[MoroccanCity]', N'ImageUrl') IS NULL
BEGIN
    ALTER TABLE [dbo].[MoroccanCity]
    ADD [ImageUrl] [varchar](500) NOT NULL
        CONSTRAINT [DF_MoroccanCity_ImageUrl]
        DEFAULT ('https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop');
END;
GO

-- Unit
IF COL_LENGTH(N'[dbo].[Unit]', N'ImageUrl') IS NULL
BEGIN
    ALTER TABLE [dbo].[Unit]
    ADD [ImageUrl] [varchar](500) NOT NULL
        CONSTRAINT [DF_Unit_ImageUrl]
        DEFAULT ('https://images.unsplash.com/photo-1466637574441-749b8f19452f?w=400&fit=crop');
END;
GO

-- Ingredient
IF COL_LENGTH(N'[dbo].[Ingredient]', N'ImageUrl') IS NULL
BEGIN
    ALTER TABLE [dbo].[Ingredient]
    ADD [ImageUrl] [varchar](500) NOT NULL
        CONSTRAINT [DF_Ingredient_ImageUrl]
        DEFAULT ('https://images.unsplash.com/photo-1506976785307-8732e854ad03?w=400&fit=crop');
END;
GO

-- RecipeStep
IF COL_LENGTH(N'[dbo].[RecipeStep]', N'ImageUrl') IS NULL
BEGIN
    ALTER TABLE [dbo].[RecipeStep]
    ADD [ImageUrl] [varchar](500) NULL
        CONSTRAINT [DF_RecipeStep_ImageUrl]
        DEFAULT ('https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?w=400&fit=crop');
END;
GO

-- RecipeNutrition
IF COL_LENGTH(N'[dbo].[RecipeNutrition]', N'ImageUrl') IS NULL
BEGIN
    ALTER TABLE [dbo].[RecipeNutrition]
    ADD [ImageUrl] [varchar](500) NOT NULL
        CONSTRAINT [DF_RecipeNutrition_ImageUrl]
        DEFAULT ('https://images.unsplash.com/photo-1490645935967-10de6ba17061?w=400&fit=crop');
END;
GO

-- RecipeIngredient
IF COL_LENGTH(N'[dbo].[RecipeIngredient]', N'ImageUrl') IS NULL
BEGIN
    ALTER TABLE [dbo].[RecipeIngredient]
    ADD [ImageUrl] [varchar](500) NULL
        CONSTRAINT [DF_RecipeIngredient_ImageUrl]
        DEFAULT ('https://images.unsplash.com/photo-1506976785307-8732e854ad03?w=400&fit=crop');
END;
GO

-- RecipeShare
IF COL_LENGTH(N'[dbo].[RecipeShare]', N'ImageUrl') IS NULL
BEGIN
    ALTER TABLE [dbo].[RecipeShare]
    ADD [ImageUrl] [varchar](500) NULL
        CONSTRAINT [DF_RecipeShare_ImageUrl]
        DEFAULT ('https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=400&fit=crop');
END;
GO

-- DailyRecipeStats
IF COL_LENGTH(N'[dbo].[DailyRecipeStats]', N'ImageUrl') IS NULL
BEGIN
    ALTER TABLE [dbo].[DailyRecipeStats]
    ADD [ImageUrl] [varchar](500) NOT NULL
        CONSTRAINT [DF_DailyRecipeStats_ImageUrl]
        DEFAULT ('https://images.unsplash.com/photo-1551288049-bebda4e38f71?w=400&fit=crop');
END;
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- STEP 2 — Back-fill existing tables that ALREADY have an image column
--          but may contain NULL values
-- ─────────────────────────────────────────────────────────────────────────────

-- Cuisine.IconUrl
UPDATE [dbo].[Cuisine]
SET    [IconUrl] = 'https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=400&fit=crop'
WHERE  [IconUrl] IS NULL;
GO

-- Difficulty.IconUrl
UPDATE [dbo].[Difficulty]
SET    [IconUrl] = 'https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?w=400&fit=crop'
WHERE  [IconUrl] IS NULL;
GO

-- FoodCategory.IconUrl
UPDATE [dbo].[FoodCategory]
SET    [IconUrl] = 'https://images.unsplash.com/photo-1476224203421-9ac39bcb3327?w=400&fit=crop'
WHERE  [IconUrl] IS NULL;
GO

-- IngredientType.IconUrl
UPDATE [dbo].[IngredientType]
SET    [IconUrl] = 'https://images.unsplash.com/photo-1506976785307-8732e854ad03?w=400&fit=crop'
WHERE  [IconUrl] IS NULL;
GO

-- Recipe.CoverImageUrl
UPDATE [dbo].[Recipe]
SET    [CoverImageUrl] = 'https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=400&fit=crop'
WHERE  [CoverImageUrl] IS NULL;
GO

-- AppUser.ProfileImageUrl
UPDATE [dbo].[AppUser]
SET    [ProfileImageUrl] = 'https://images.unsplash.com/photo-1511367461989-f85a21fda167?w=400&fit=crop'
WHERE  [ProfileImageUrl] IS NULL;
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- STEP 3 — Add DEFAULT constraints to existing image columns that lack one
-- ─────────────────────────────────────────────────────────────────────────────

-- Cuisine.IconUrl default
IF NOT EXISTS (SELECT 1 FROM [sys].[default_constraints]
               WHERE [name] = 'DF_Cuisine_IconUrl'
                 AND [parent_object_id] = OBJECT_ID(N'[dbo].[Cuisine]'))
BEGIN
    ALTER TABLE [dbo].[Cuisine]
    ADD CONSTRAINT [DF_Cuisine_IconUrl]
        DEFAULT ('https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=400&fit=crop')
        FOR [IconUrl];
END;
GO

-- Difficulty.IconUrl default
IF NOT EXISTS (SELECT 1 FROM [sys].[default_constraints]
               WHERE [name] = 'DF_Difficulty_IconUrl'
                 AND [parent_object_id] = OBJECT_ID(N'[dbo].[Difficulty]'))
BEGIN
    ALTER TABLE [dbo].[Difficulty]
    ADD CONSTRAINT [DF_Difficulty_IconUrl]
        DEFAULT ('https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?w=400&fit=crop')
        FOR [IconUrl];
END;
GO

-- FoodCategory.IconUrl default
IF NOT EXISTS (SELECT 1 FROM [sys].[default_constraints]
               WHERE [name] = 'DF_FoodCategory_IconUrl'
                 AND [parent_object_id] = OBJECT_ID(N'[dbo].[FoodCategory]'))
BEGIN
    ALTER TABLE [dbo].[FoodCategory]
    ADD CONSTRAINT [DF_FoodCategory_IconUrl]
        DEFAULT ('https://images.unsplash.com/photo-1476224203421-9ac39bcb3327?w=400&fit=crop')
        FOR [IconUrl];
END;
GO

-- IngredientType.IconUrl default
IF NOT EXISTS (SELECT 1 FROM [sys].[default_constraints]
               WHERE [name] = 'DF_IngredientType_IconUrl'
                 AND [parent_object_id] = OBJECT_ID(N'[dbo].[IngredientType]'))
BEGIN
    ALTER TABLE [dbo].[IngredientType]
    ADD CONSTRAINT [DF_IngredientType_IconUrl]
        DEFAULT ('https://images.unsplash.com/photo-1506976785307-8732e854ad03?w=400&fit=crop')
        FOR [IconUrl];
END;
GO

-- Recipe.CoverImageUrl default
IF NOT EXISTS (SELECT 1 FROM [sys].[default_constraints]
               WHERE [name] = 'DF_Recipe_CoverImageUrl'
                 AND [parent_object_id] = OBJECT_ID(N'[dbo].[Recipe]'))
BEGIN
    ALTER TABLE [dbo].[Recipe]
    ADD CONSTRAINT [DF_Recipe_CoverImageUrl]
        DEFAULT ('https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=400&fit=crop')
        FOR [CoverImageUrl];
END;
GO

-- AppUser.ProfileImageUrl default
IF NOT EXISTS (SELECT 1 FROM [sys].[default_constraints]
               WHERE [name] = 'DF_AppUser_ProfileImageUrl'
                 AND [parent_object_id] = OBJECT_ID(N'[dbo].[AppUser]'))
BEGIN
    ALTER TABLE [dbo].[AppUser]
    ADD CONSTRAINT [DF_AppUser_ProfileImageUrl]
        DEFAULT ('https://images.unsplash.com/photo-1511367461989-f85a21fda167?w=400&fit=crop')
        FOR [ProfileImageUrl];
END;
GO

PRINT 'Migration [ImageUrl_AllTables] completed successfully.';
GO
