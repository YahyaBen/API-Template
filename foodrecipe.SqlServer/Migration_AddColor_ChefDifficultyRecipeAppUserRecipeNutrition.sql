-- ─────────────────────────────────────────────────────────────────────────────
-- Migration: Add [Color] column
-- Tables:    ChefProfile, Recipe, AppUser, RecipeNutrition
-- Note:      Difficulty already has [Color] — skipped.
-- Date:      2026-04-07
-- Pattern:   [varchar](20) NULL  (matches existing Color columns)
-- ─────────────────────────────────────────────────────────────────────────────

-- ChefProfile
IF COL_LENGTH(N'[dbo].[ChefProfile]', N'Color') IS NULL
    ALTER TABLE [dbo].[ChefProfile] ADD [Color] [varchar](20) NULL;
GO

-- Recipe
IF COL_LENGTH(N'[dbo].[Recipe]', N'Color') IS NULL
    ALTER TABLE [dbo].[Recipe] ADD [Color] [varchar](20) NULL;
GO

-- AppUser
IF COL_LENGTH(N'[dbo].[AppUser]', N'Color') IS NULL
    ALTER TABLE [dbo].[AppUser] ADD [Color] [varchar](20) NULL;
GO

-- RecipeNutrition
IF COL_LENGTH(N'[dbo].[RecipeNutrition]', N'Color') IS NULL
    ALTER TABLE [dbo].[RecipeNutrition] ADD [Color] [varchar](20) NULL;
GO
