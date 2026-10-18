-- ============================================================================
-- Migration: UUID v7 + Auto-Generated Code for All Tables
-- Date:      2026-04-08
-- Purpose:   1. Add [Uuid] UNIQUEIDENTIFIER column to every entity table.
--               Uses NEWSEQUENTIALID() default for time-ordered, index-friendly
--               GUIDs (equivalent to UUID v7 in .NET).
--            2. Add [Code] VARCHAR column to tables that don't have one yet.
--               Format: {PREFIX}_{8-hex} — generated from NEWID() hex.
--               Tables that already have a Code column are left untouched.
--            3. Junction tables (composite PK, no own identity) are EXCLUDED
--               from both Uuid and Code (RecipeCategory, RecipeLike).
--               RecipeIngredient gets Uuid + Code because it has payload.
--            4. RecipeNutrition (1:1 dependent, PK = RecipeId) gets Uuid only.
--            Idempotent — safe to re-run.
-- ============================================================================


-- ═══════════════════════════════════════════════════════════════════════════
-- HELPER: Reusable pattern for each table
--   Step A — Add [Uuid] with NEWSEQUENTIALID() default
--   Step B — Backfill existing rows with NEWID()
--   Step C — Add unique index on [Uuid]
--   Step D — Add [Code] with temporary NULL, backfill, make NOT NULL + UNIQUE
-- ═══════════════════════════════════════════════════════════════════════════


-- ─────────────────────────────────────────────────────────────────────────────
-- 1. AppUser  (Code prefix: USR)
-- ─────────────────────────────────────────────────────────────────────────────

IF COL_LENGTH(N'[dbo].[AppUser]', N'Uuid') IS NULL
BEGIN
    ALTER TABLE [dbo].[AppUser]
    ADD [Uuid] UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT [DF_AppUser_Uuid] DEFAULT (NEWSEQUENTIALID());
END;
GO

-- Backfill existing rows that got 0x00 from the column add
UPDATE [dbo].[AppUser]
SET [Uuid] = NEWID()
WHERE [Uuid] = '00000000-0000-0000-0000-000000000000';
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = 'UQ_AppUser_Uuid' AND object_id = OBJECT_ID(N'[dbo].[AppUser]'))
BEGIN
    ALTER TABLE [dbo].[AppUser] ADD CONSTRAINT [UQ_AppUser_Uuid] UNIQUE ([Uuid]);
END;
GO

IF COL_LENGTH(N'[dbo].[AppUser]', N'Code') IS NULL
BEGIN
    ALTER TABLE [dbo].[AppUser] ADD [Code] VARCHAR(20) NULL;
END;
GO

UPDATE [dbo].[AppUser]
SET [Code] = 'USR_' + UPPER(LEFT(CONVERT(VARCHAR(36), NEWID()), 8))
WHERE [Code] IS NULL;
GO

ALTER TABLE [dbo].[AppUser] ALTER COLUMN [Code] VARCHAR(20) NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = 'UQ_AppUser_Code' AND object_id = OBJECT_ID(N'[dbo].[AppUser]'))
BEGIN
    ALTER TABLE [dbo].[AppUser] ADD CONSTRAINT [UQ_AppUser_Code] UNIQUE ([Code]);
END;
GO


-- ─────────────────────────────────────────────────────────────────────────────
-- 2. ChefProfile  (Code prefix: CHF)
-- ─────────────────────────────────────────────────────────────────────────────

IF COL_LENGTH(N'[dbo].[ChefProfile]', N'Uuid') IS NULL
BEGIN
    ALTER TABLE [dbo].[ChefProfile]
    ADD [Uuid] UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT [DF_ChefProfile_Uuid] DEFAULT (NEWSEQUENTIALID());
END;
GO

UPDATE [dbo].[ChefProfile]
SET [Uuid] = NEWID()
WHERE [Uuid] = '00000000-0000-0000-0000-000000000000';
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = 'UQ_ChefProfile_Uuid' AND object_id = OBJECT_ID(N'[dbo].[ChefProfile]'))
BEGIN
    ALTER TABLE [dbo].[ChefProfile] ADD CONSTRAINT [UQ_ChefProfile_Uuid] UNIQUE ([Uuid]);
END;
GO

IF COL_LENGTH(N'[dbo].[ChefProfile]', N'Code') IS NULL
BEGIN
    ALTER TABLE [dbo].[ChefProfile] ADD [Code] VARCHAR(20) NULL;
END;
GO

UPDATE [dbo].[ChefProfile]
SET [Code] = 'CHF_' + UPPER(LEFT(CONVERT(VARCHAR(36), NEWID()), 8))
WHERE [Code] IS NULL;
GO

ALTER TABLE [dbo].[ChefProfile] ALTER COLUMN [Code] VARCHAR(20) NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = 'UQ_ChefProfile_Code' AND object_id = OBJECT_ID(N'[dbo].[ChefProfile]'))
BEGIN
    ALTER TABLE [dbo].[ChefProfile] ADD CONSTRAINT [UQ_ChefProfile_Code] UNIQUE ([Code]);
END;
GO


-- ─────────────────────────────────────────────────────────────────────────────
-- 3. PresetAvatarImage  (Code already exists — Uuid only)
-- ─────────────────────────────────────────────────────────────────────────────

IF COL_LENGTH(N'[dbo].[PresetAvatarImage]', N'Uuid') IS NULL
BEGIN
    ALTER TABLE [dbo].[PresetAvatarImage]
    ADD [Uuid] UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT [DF_PresetAvatarImage_Uuid] DEFAULT (NEWSEQUENTIALID());
END;
GO

UPDATE [dbo].[PresetAvatarImage]
SET [Uuid] = NEWID()
WHERE [Uuid] = '00000000-0000-0000-0000-000000000000';
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = 'UQ_PresetAvatarImage_Uuid' AND object_id = OBJECT_ID(N'[dbo].[PresetAvatarImage]'))
BEGIN
    ALTER TABLE [dbo].[PresetAvatarImage] ADD CONSTRAINT [UQ_PresetAvatarImage_Uuid] UNIQUE ([Uuid]);
END;
GO


-- ─────────────────────────────────────────────────────────────────────────────
-- 4. Difficulty  (Code already exists — Uuid only)
-- ─────────────────────────────────────────────────────────────────────────────

IF COL_LENGTH(N'[dbo].[Difficulty]', N'Uuid') IS NULL
BEGIN
    ALTER TABLE [dbo].[Difficulty]
    ADD [Uuid] UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT [DF_Difficulty_Uuid] DEFAULT (NEWSEQUENTIALID());
END;
GO

UPDATE [dbo].[Difficulty]
SET [Uuid] = NEWID()
WHERE [Uuid] = '00000000-0000-0000-0000-000000000000';
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = 'UQ_Difficulty_Uuid' AND object_id = OBJECT_ID(N'[dbo].[Difficulty]'))
BEGIN
    ALTER TABLE [dbo].[Difficulty] ADD CONSTRAINT [UQ_Difficulty_Uuid] UNIQUE ([Uuid]);
END;
GO


-- ─────────────────────────────────────────────────────────────────────────────
-- 5. FoodCategory  (Code already exists — Uuid only)
-- ─────────────────────────────────────────────────────────────────────────────

IF COL_LENGTH(N'[dbo].[FoodCategory]', N'Uuid') IS NULL
BEGIN
    ALTER TABLE [dbo].[FoodCategory]
    ADD [Uuid] UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT [DF_FoodCategory_Uuid] DEFAULT (NEWSEQUENTIALID());
END;
GO

UPDATE [dbo].[FoodCategory]
SET [Uuid] = NEWID()
WHERE [Uuid] = '00000000-0000-0000-0000-000000000000';
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = 'UQ_FoodCategory_Uuid' AND object_id = OBJECT_ID(N'[dbo].[FoodCategory]'))
BEGIN
    ALTER TABLE [dbo].[FoodCategory] ADD CONSTRAINT [UQ_FoodCategory_Uuid] UNIQUE ([Uuid]);
END;
GO


-- ─────────────────────────────────────────────────────────────────────────────
-- 6. Cuisine  (Code already exists — Uuid only)
-- ─────────────────────────────────────────────────────────────────────────────

IF COL_LENGTH(N'[dbo].[Cuisine]', N'Uuid') IS NULL
BEGIN
    ALTER TABLE [dbo].[Cuisine]
    ADD [Uuid] UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT [DF_Cuisine_Uuid] DEFAULT (NEWSEQUENTIALID());
END;
GO

UPDATE [dbo].[Cuisine]
SET [Uuid] = NEWID()
WHERE [Uuid] = '00000000-0000-0000-0000-000000000000';
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = 'UQ_Cuisine_Uuid' AND object_id = OBJECT_ID(N'[dbo].[Cuisine]'))
BEGIN
    ALTER TABLE [dbo].[Cuisine] ADD CONSTRAINT [UQ_Cuisine_Uuid] UNIQUE ([Uuid]);
END;
GO


-- ─────────────────────────────────────────────────────────────────────────────
-- 7. MoroccanRegion  (Code already exists — Uuid only)
-- ─────────────────────────────────────────────────────────────────────────────

IF COL_LENGTH(N'[dbo].[MoroccanRegion]', N'Uuid') IS NULL
BEGIN
    ALTER TABLE [dbo].[MoroccanRegion]
    ADD [Uuid] UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT [DF_MoroccanRegion_Uuid] DEFAULT (NEWSEQUENTIALID());
END;
GO

UPDATE [dbo].[MoroccanRegion]
SET [Uuid] = NEWID()
WHERE [Uuid] = '00000000-0000-0000-0000-000000000000';
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = 'UQ_MoroccanRegion_Uuid' AND object_id = OBJECT_ID(N'[dbo].[MoroccanRegion]'))
BEGIN
    ALTER TABLE [dbo].[MoroccanRegion] ADD CONSTRAINT [UQ_MoroccanRegion_Uuid] UNIQUE ([Uuid]);
END;
GO


-- ─────────────────────────────────────────────────────────────────────────────
-- 8. MoroccanCity  (Code prefix: CTY)
-- ─────────────────────────────────────────────────────────────────────────────

IF COL_LENGTH(N'[dbo].[MoroccanCity]', N'Uuid') IS NULL
BEGIN
    ALTER TABLE [dbo].[MoroccanCity]
    ADD [Uuid] UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT [DF_MoroccanCity_Uuid] DEFAULT (NEWSEQUENTIALID());
END;
GO

UPDATE [dbo].[MoroccanCity]
SET [Uuid] = NEWID()
WHERE [Uuid] = '00000000-0000-0000-0000-000000000000';
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = 'UQ_MoroccanCity_Uuid' AND object_id = OBJECT_ID(N'[dbo].[MoroccanCity]'))
BEGIN
    ALTER TABLE [dbo].[MoroccanCity] ADD CONSTRAINT [UQ_MoroccanCity_Uuid] UNIQUE ([Uuid]);
END;
GO

-- MoroccanCity already has Slug as a unique natural key.
-- We generate Code from the Slug (uppercased, dashes to underscores) for consistency.
IF COL_LENGTH(N'[dbo].[MoroccanCity]', N'Code') IS NULL
BEGIN
    ALTER TABLE [dbo].[MoroccanCity] ADD [Code] VARCHAR(30) NULL;
END;
GO

UPDATE [dbo].[MoroccanCity]
SET [Code] = UPPER(REPLACE([Slug], '-', '_'))
WHERE [Code] IS NULL;
GO

ALTER TABLE [dbo].[MoroccanCity] ALTER COLUMN [Code] VARCHAR(30) NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = 'UQ_MoroccanCity_Code' AND object_id = OBJECT_ID(N'[dbo].[MoroccanCity]'))
BEGIN
    ALTER TABLE [dbo].[MoroccanCity] ADD CONSTRAINT [UQ_MoroccanCity_Code] UNIQUE ([Code]);
END;
GO


-- ─────────────────────────────────────────────────────────────────────────────
-- 9. Unit  (Code already exists — Uuid only)
-- ─────────────────────────────────────────────────────────────────────────────

IF COL_LENGTH(N'[dbo].[Unit]', N'Uuid') IS NULL
BEGIN
    ALTER TABLE [dbo].[Unit]
    ADD [Uuid] UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT [DF_Unit_Uuid] DEFAULT (NEWSEQUENTIALID());
END;
GO

UPDATE [dbo].[Unit]
SET [Uuid] = NEWID()
WHERE [Uuid] = '00000000-0000-0000-0000-000000000000';
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = 'UQ_Unit_Uuid' AND object_id = OBJECT_ID(N'[dbo].[Unit]'))
BEGIN
    ALTER TABLE [dbo].[Unit] ADD CONSTRAINT [UQ_Unit_Uuid] UNIQUE ([Uuid]);
END;
GO


-- ─────────────────────────────────────────────────────────────────────────────
-- 10. IngredientType  (Code already exists — Uuid only)
-- ─────────────────────────────────────────────────────────────────────────────

IF COL_LENGTH(N'[dbo].[IngredientType]', N'Uuid') IS NULL
BEGIN
    ALTER TABLE [dbo].[IngredientType]
    ADD [Uuid] UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT [DF_IngredientType_Uuid] DEFAULT (NEWSEQUENTIALID());
END;
GO

UPDATE [dbo].[IngredientType]
SET [Uuid] = NEWID()
WHERE [Uuid] = '00000000-0000-0000-0000-000000000000';
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = 'UQ_IngredientType_Uuid' AND object_id = OBJECT_ID(N'[dbo].[IngredientType]'))
BEGIN
    ALTER TABLE [dbo].[IngredientType] ADD CONSTRAINT [UQ_IngredientType_Uuid] UNIQUE ([Uuid]);
END;
GO


-- ─────────────────────────────────────────────────────────────────────────────
-- 11. Ingredient  (Code prefix: ING)
-- ─────────────────────────────────────────────────────────────────────────────

IF COL_LENGTH(N'[dbo].[Ingredient]', N'Uuid') IS NULL
BEGIN
    ALTER TABLE [dbo].[Ingredient]
    ADD [Uuid] UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT [DF_Ingredient_Uuid] DEFAULT (NEWSEQUENTIALID());
END;
GO

UPDATE [dbo].[Ingredient]
SET [Uuid] = NEWID()
WHERE [Uuid] = '00000000-0000-0000-0000-000000000000';
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = 'UQ_Ingredient_Uuid' AND object_id = OBJECT_ID(N'[dbo].[Ingredient]'))
BEGIN
    ALTER TABLE [dbo].[Ingredient] ADD CONSTRAINT [UQ_Ingredient_Uuid] UNIQUE ([Uuid]);
END;
GO

IF COL_LENGTH(N'[dbo].[Ingredient]', N'Code') IS NULL
BEGIN
    ALTER TABLE [dbo].[Ingredient] ADD [Code] VARCHAR(20) NULL;
END;
GO

UPDATE [dbo].[Ingredient]
SET [Code] = 'ING_' + UPPER(LEFT(CONVERT(VARCHAR(36), NEWID()), 8))
WHERE [Code] IS NULL;
GO

ALTER TABLE [dbo].[Ingredient] ALTER COLUMN [Code] VARCHAR(20) NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = 'UQ_Ingredient_Code' AND object_id = OBJECT_ID(N'[dbo].[Ingredient]'))
BEGIN
    ALTER TABLE [dbo].[Ingredient] ADD CONSTRAINT [UQ_Ingredient_Code] UNIQUE ([Code]);
END;
GO


-- ─────────────────────────────────────────────────────────────────────────────
-- 12. IngredientImage  (Code prefix: IGI)
-- ─────────────────────────────────────────────────────────────────────────────

IF COL_LENGTH(N'[dbo].[IngredientImage]', N'Uuid') IS NULL
BEGIN
    ALTER TABLE [dbo].[IngredientImage]
    ADD [Uuid] UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT [DF_IngredientImage_Uuid] DEFAULT (NEWSEQUENTIALID());
END;
GO

UPDATE [dbo].[IngredientImage]
SET [Uuid] = NEWID()
WHERE [Uuid] = '00000000-0000-0000-0000-000000000000';
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = 'UQ_IngredientImage_Uuid' AND object_id = OBJECT_ID(N'[dbo].[IngredientImage]'))
BEGIN
    ALTER TABLE [dbo].[IngredientImage] ADD CONSTRAINT [UQ_IngredientImage_Uuid] UNIQUE ([Uuid]);
END;
GO

IF COL_LENGTH(N'[dbo].[IngredientImage]', N'Code') IS NULL
BEGIN
    ALTER TABLE [dbo].[IngredientImage] ADD [Code] VARCHAR(20) NULL;
END;
GO

UPDATE [dbo].[IngredientImage]
SET [Code] = 'IGI_' + UPPER(LEFT(CONVERT(VARCHAR(36), NEWID()), 8))
WHERE [Code] IS NULL;
GO

ALTER TABLE [dbo].[IngredientImage] ALTER COLUMN [Code] VARCHAR(20) NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = 'UQ_IngredientImage_Code' AND object_id = OBJECT_ID(N'[dbo].[IngredientImage]'))
BEGIN
    ALTER TABLE [dbo].[IngredientImage] ADD CONSTRAINT [UQ_IngredientImage_Code] UNIQUE ([Code]);
END;
GO


-- ─────────────────────────────────────────────────────────────────────────────
-- 13. Recipe  (Code prefix: RCP)
-- ─────────────────────────────────────────────────────────────────────────────

IF COL_LENGTH(N'[dbo].[Recipe]', N'Uuid') IS NULL
BEGIN
    ALTER TABLE [dbo].[Recipe]
    ADD [Uuid] UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT [DF_Recipe_Uuid] DEFAULT (NEWSEQUENTIALID());
END;
GO

UPDATE [dbo].[Recipe]
SET [Uuid] = NEWID()
WHERE [Uuid] = '00000000-0000-0000-0000-000000000000';
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = 'UQ_Recipe_Uuid' AND object_id = OBJECT_ID(N'[dbo].[Recipe]'))
BEGIN
    ALTER TABLE [dbo].[Recipe] ADD CONSTRAINT [UQ_Recipe_Uuid] UNIQUE ([Uuid]);
END;
GO

IF COL_LENGTH(N'[dbo].[Recipe]', N'Code') IS NULL
BEGIN
    ALTER TABLE [dbo].[Recipe] ADD [Code] VARCHAR(20) NULL;
END;
GO

UPDATE [dbo].[Recipe]
SET [Code] = 'RCP_' + UPPER(LEFT(CONVERT(VARCHAR(36), NEWID()), 8))
WHERE [Code] IS NULL;
GO

ALTER TABLE [dbo].[Recipe] ALTER COLUMN [Code] VARCHAR(20) NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = 'UQ_Recipe_Code' AND object_id = OBJECT_ID(N'[dbo].[Recipe]'))
BEGIN
    ALTER TABLE [dbo].[Recipe] ADD CONSTRAINT [UQ_Recipe_Code] UNIQUE ([Code]);
END;
GO


-- ─────────────────────────────────────────────────────────────────────────────
-- 14. RecipeImage  (Code prefix: RIM)
-- ─────────────────────────────────────────────────────────────────────────────

IF COL_LENGTH(N'[dbo].[RecipeImage]', N'Uuid') IS NULL
BEGIN
    ALTER TABLE [dbo].[RecipeImage]
    ADD [Uuid] UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT [DF_RecipeImage_Uuid] DEFAULT (NEWSEQUENTIALID());
END;
GO

UPDATE [dbo].[RecipeImage]
SET [Uuid] = NEWID()
WHERE [Uuid] = '00000000-0000-0000-0000-000000000000';
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = 'UQ_RecipeImage_Uuid' AND object_id = OBJECT_ID(N'[dbo].[RecipeImage]'))
BEGIN
    ALTER TABLE [dbo].[RecipeImage] ADD CONSTRAINT [UQ_RecipeImage_Uuid] UNIQUE ([Uuid]);
END;
GO

IF COL_LENGTH(N'[dbo].[RecipeImage]', N'Code') IS NULL
BEGIN
    ALTER TABLE [dbo].[RecipeImage] ADD [Code] VARCHAR(20) NULL;
END;
GO

UPDATE [dbo].[RecipeImage]
SET [Code] = 'RIM_' + UPPER(LEFT(CONVERT(VARCHAR(36), NEWID()), 8))
WHERE [Code] IS NULL;
GO

ALTER TABLE [dbo].[RecipeImage] ALTER COLUMN [Code] VARCHAR(20) NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = 'UQ_RecipeImage_Code' AND object_id = OBJECT_ID(N'[dbo].[RecipeImage]'))
BEGIN
    ALTER TABLE [dbo].[RecipeImage] ADD CONSTRAINT [UQ_RecipeImage_Code] UNIQUE ([Code]);
END;
GO


-- ─────────────────────────────────────────────────────────────────────────────
-- 15. RecipeStep  (Code prefix: RST)
-- ─────────────────────────────────────────────────────────────────────────────

IF COL_LENGTH(N'[dbo].[RecipeStep]', N'Uuid') IS NULL
BEGIN
    ALTER TABLE [dbo].[RecipeStep]
    ADD [Uuid] UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT [DF_RecipeStep_Uuid] DEFAULT (NEWSEQUENTIALID());
END;
GO

UPDATE [dbo].[RecipeStep]
SET [Uuid] = NEWID()
WHERE [Uuid] = '00000000-0000-0000-0000-000000000000';
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = 'UQ_RecipeStep_Uuid' AND object_id = OBJECT_ID(N'[dbo].[RecipeStep]'))
BEGIN
    ALTER TABLE [dbo].[RecipeStep] ADD CONSTRAINT [UQ_RecipeStep_Uuid] UNIQUE ([Uuid]);
END;
GO

IF COL_LENGTH(N'[dbo].[RecipeStep]', N'Code') IS NULL
BEGIN
    ALTER TABLE [dbo].[RecipeStep] ADD [Code] VARCHAR(20) NULL;
END;
GO

UPDATE [dbo].[RecipeStep]
SET [Code] = 'RST_' + UPPER(LEFT(CONVERT(VARCHAR(36), NEWID()), 8))
WHERE [Code] IS NULL;
GO

ALTER TABLE [dbo].[RecipeStep] ALTER COLUMN [Code] VARCHAR(20) NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = 'UQ_RecipeStep_Code' AND object_id = OBJECT_ID(N'[dbo].[RecipeStep]'))
BEGIN
    ALTER TABLE [dbo].[RecipeStep] ADD CONSTRAINT [UQ_RecipeStep_Code] UNIQUE ([Code]);
END;
GO


-- ─────────────────────────────────────────────────────────────────────────────
-- 16. RecipeNutrition  (Uuid only — 1:1 dependent, PK = RecipeId)
-- ─────────────────────────────────────────────────────────────────────────────

IF COL_LENGTH(N'[dbo].[RecipeNutrition]', N'Uuid') IS NULL
BEGIN
    ALTER TABLE [dbo].[RecipeNutrition]
    ADD [Uuid] UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT [DF_RecipeNutrition_Uuid] DEFAULT (NEWSEQUENTIALID());
END;
GO

UPDATE [dbo].[RecipeNutrition]
SET [Uuid] = NEWID()
WHERE [Uuid] = '00000000-0000-0000-0000-000000000000';
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = 'UQ_RecipeNutrition_Uuid' AND object_id = OBJECT_ID(N'[dbo].[RecipeNutrition]'))
BEGIN
    ALTER TABLE [dbo].[RecipeNutrition] ADD CONSTRAINT [UQ_RecipeNutrition_Uuid] UNIQUE ([Uuid]);
END;
GO


-- ─────────────────────────────────────────────────────────────────────────────
-- 17. RecipeShare  (Code prefix: RSH)
-- ─────────────────────────────────────────────────────────────────────────────

IF COL_LENGTH(N'[dbo].[RecipeShare]', N'Uuid') IS NULL
BEGIN
    ALTER TABLE [dbo].[RecipeShare]
    ADD [Uuid] UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT [DF_RecipeShare_Uuid] DEFAULT (NEWSEQUENTIALID());
END;
GO

UPDATE [dbo].[RecipeShare]
SET [Uuid] = NEWID()
WHERE [Uuid] = '00000000-0000-0000-0000-000000000000';
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = 'UQ_RecipeShare_Uuid' AND object_id = OBJECT_ID(N'[dbo].[RecipeShare]'))
BEGIN
    ALTER TABLE [dbo].[RecipeShare] ADD CONSTRAINT [UQ_RecipeShare_Uuid] UNIQUE ([Uuid]);
END;
GO

IF COL_LENGTH(N'[dbo].[RecipeShare]', N'Code') IS NULL
BEGIN
    ALTER TABLE [dbo].[RecipeShare] ADD [Code] VARCHAR(20) NULL;
END;
GO

UPDATE [dbo].[RecipeShare]
SET [Code] = 'RSH_' + UPPER(LEFT(CONVERT(VARCHAR(36), NEWID()), 8))
WHERE [Code] IS NULL;
GO

ALTER TABLE [dbo].[RecipeShare] ALTER COLUMN [Code] VARCHAR(20) NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = 'UQ_RecipeShare_Code' AND object_id = OBJECT_ID(N'[dbo].[RecipeShare]'))
BEGIN
    ALTER TABLE [dbo].[RecipeShare] ADD CONSTRAINT [UQ_RecipeShare_Code] UNIQUE ([Code]);
END;
GO


-- ─────────────────────────────────────────────────────────────────────────────
-- 18. RecipeIngredient  (Code prefix: RIG — has payload, not a pure junction)
-- ─────────────────────────────────────────────────────────────────────────────

IF COL_LENGTH(N'[dbo].[RecipeIngredient]', N'Uuid') IS NULL
BEGIN
    ALTER TABLE [dbo].[RecipeIngredient]
    ADD [Uuid] UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT [DF_RecipeIngredient_Uuid] DEFAULT (NEWSEQUENTIALID());
END;
GO

UPDATE [dbo].[RecipeIngredient]
SET [Uuid] = NEWID()
WHERE [Uuid] = '00000000-0000-0000-0000-000000000000';
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = 'UQ_RecipeIngredient_Uuid' AND object_id = OBJECT_ID(N'[dbo].[RecipeIngredient]'))
BEGIN
    ALTER TABLE [dbo].[RecipeIngredient] ADD CONSTRAINT [UQ_RecipeIngredient_Uuid] UNIQUE ([Uuid]);
END;
GO

IF COL_LENGTH(N'[dbo].[RecipeIngredient]', N'Code') IS NULL
BEGIN
    ALTER TABLE [dbo].[RecipeIngredient] ADD [Code] VARCHAR(20) NULL;
END;
GO

UPDATE [dbo].[RecipeIngredient]
SET [Code] = 'RIG_' + UPPER(LEFT(CONVERT(VARCHAR(36), NEWID()), 8))
WHERE [Code] IS NULL;
GO

ALTER TABLE [dbo].[RecipeIngredient] ALTER COLUMN [Code] VARCHAR(20) NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = 'UQ_RecipeIngredient_Code' AND object_id = OBJECT_ID(N'[dbo].[RecipeIngredient]'))
BEGIN
    ALTER TABLE [dbo].[RecipeIngredient] ADD CONSTRAINT [UQ_RecipeIngredient_Code] UNIQUE ([Code]);
END;
GO


-- ─────────────────────────────────────────────────────────────────────────────
-- 19. AppUserExternalLogin  (Code prefix: EXL)
--     Guarded: table may not exist if Google-auth migration was not run.
-- ─────────────────────────────────────────────────────────────────────────────

IF OBJECT_ID(N'[dbo].[AppUserExternalLogin]', N'U') IS NOT NULL
  AND COL_LENGTH(N'[dbo].[AppUserExternalLogin]', N'Uuid') IS NULL
BEGIN
    ALTER TABLE [dbo].[AppUserExternalLogin]
    ADD [Uuid] UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT [DF_AppUserExternalLogin_Uuid] DEFAULT (NEWSEQUENTIALID());
END;
GO

IF OBJECT_ID(N'[dbo].[AppUserExternalLogin]', N'U') IS NOT NULL
    UPDATE [dbo].[AppUserExternalLogin]
    SET [Uuid] = NEWID()
    WHERE [Uuid] = '00000000-0000-0000-0000-000000000000';
GO

IF OBJECT_ID(N'[dbo].[AppUserExternalLogin]', N'U') IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = 'UQ_AppUserExternalLogin_Uuid' AND object_id = OBJECT_ID(N'[dbo].[AppUserExternalLogin]'))
BEGIN
    ALTER TABLE [dbo].[AppUserExternalLogin] ADD CONSTRAINT [UQ_AppUserExternalLogin_Uuid] UNIQUE ([Uuid]);
END;
GO

IF OBJECT_ID(N'[dbo].[AppUserExternalLogin]', N'U') IS NOT NULL
  AND COL_LENGTH(N'[dbo].[AppUserExternalLogin]', N'Code') IS NULL
BEGIN
    ALTER TABLE [dbo].[AppUserExternalLogin] ADD [Code] VARCHAR(20) NULL;
END;
GO

IF OBJECT_ID(N'[dbo].[AppUserExternalLogin]', N'U') IS NOT NULL
    UPDATE [dbo].[AppUserExternalLogin]
    SET [Code] = 'EXL_' + UPPER(LEFT(CONVERT(VARCHAR(36), NEWID()), 8))
    WHERE [Code] IS NULL;
GO

IF OBJECT_ID(N'[dbo].[AppUserExternalLogin]', N'U') IS NOT NULL
    ALTER TABLE [dbo].[AppUserExternalLogin] ALTER COLUMN [Code] VARCHAR(20) NOT NULL;
GO

IF OBJECT_ID(N'[dbo].[AppUserExternalLogin]', N'U') IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = 'UQ_AppUserExternalLogin_Code' AND object_id = OBJECT_ID(N'[dbo].[AppUserExternalLogin]'))
BEGIN
    ALTER TABLE [dbo].[AppUserExternalLogin] ADD CONSTRAINT [UQ_AppUserExternalLogin_Code] UNIQUE ([Code]);
END;
GO


-- ─────────────────────────────────────────────────────────────────────────────
-- 20. AppUserRefreshToken  (Code prefix: RTK)
--     Guarded: table may not exist if refresh-token migration was not run.
-- ─────────────────────────────────────────────────────────────────────────────

IF OBJECT_ID(N'[dbo].[AppUserRefreshToken]', N'U') IS NOT NULL
  AND COL_LENGTH(N'[dbo].[AppUserRefreshToken]', N'Uuid') IS NULL
BEGIN
    ALTER TABLE [dbo].[AppUserRefreshToken]
    ADD [Uuid] UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT [DF_AppUserRefreshToken_Uuid] DEFAULT (NEWSEQUENTIALID());
END;
GO

IF OBJECT_ID(N'[dbo].[AppUserRefreshToken]', N'U') IS NOT NULL
    UPDATE [dbo].[AppUserRefreshToken]
    SET [Uuid] = NEWID()
    WHERE [Uuid] = '00000000-0000-0000-0000-000000000000';
GO

IF OBJECT_ID(N'[dbo].[AppUserRefreshToken]', N'U') IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = 'UQ_AppUserRefreshToken_Uuid' AND object_id = OBJECT_ID(N'[dbo].[AppUserRefreshToken]'))
BEGIN
    ALTER TABLE [dbo].[AppUserRefreshToken] ADD CONSTRAINT [UQ_AppUserRefreshToken_Uuid] UNIQUE ([Uuid]);
END;
GO

IF OBJECT_ID(N'[dbo].[AppUserRefreshToken]', N'U') IS NOT NULL
  AND COL_LENGTH(N'[dbo].[AppUserRefreshToken]', N'Code') IS NULL
BEGIN
    ALTER TABLE [dbo].[AppUserRefreshToken] ADD [Code] VARCHAR(20) NULL;
END;
GO

IF OBJECT_ID(N'[dbo].[AppUserRefreshToken]', N'U') IS NOT NULL
    UPDATE [dbo].[AppUserRefreshToken]
    SET [Code] = 'RTK_' + UPPER(LEFT(CONVERT(VARCHAR(36), NEWID()), 8))
    WHERE [Code] IS NULL;
GO

IF OBJECT_ID(N'[dbo].[AppUserRefreshToken]', N'U') IS NOT NULL
    ALTER TABLE [dbo].[AppUserRefreshToken] ALTER COLUMN [Code] VARCHAR(20) NOT NULL;
GO

IF OBJECT_ID(N'[dbo].[AppUserRefreshToken]', N'U') IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = 'UQ_AppUserRefreshToken_Code' AND object_id = OBJECT_ID(N'[dbo].[AppUserRefreshToken]'))
BEGIN
    ALTER TABLE [dbo].[AppUserRefreshToken] ADD CONSTRAINT [UQ_AppUserRefreshToken_Code] UNIQUE ([Code]);
END;
GO


-- ─────────────────────────────────────────────────────────────────────────────
-- 21. DailyRecipeStats  (Code prefix: DRS — date-based)
--     Guarded: table may not exist if DailyRecipeStats migration was not run.
-- ─────────────────────────────────────────────────────────────────────────────

IF OBJECT_ID(N'[dbo].[DailyRecipeStats]', N'U') IS NOT NULL
  AND COL_LENGTH(N'[dbo].[DailyRecipeStats]', N'Uuid') IS NULL
BEGIN
    ALTER TABLE [dbo].[DailyRecipeStats]
    ADD [Uuid] UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT [DF_DailyRecipeStats_Uuid] DEFAULT (NEWSEQUENTIALID());
END;
GO

IF OBJECT_ID(N'[dbo].[DailyRecipeStats]', N'U') IS NOT NULL
    UPDATE [dbo].[DailyRecipeStats]
    SET [Uuid] = NEWID()
    WHERE [Uuid] = '00000000-0000-0000-0000-000000000000';
GO

IF OBJECT_ID(N'[dbo].[DailyRecipeStats]', N'U') IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = 'UQ_DailyRecipeStats_Uuid' AND object_id = OBJECT_ID(N'[dbo].[DailyRecipeStats]'))
BEGIN
    ALTER TABLE [dbo].[DailyRecipeStats] ADD CONSTRAINT [UQ_DailyRecipeStats_Uuid] UNIQUE ([Uuid]);
END;
GO

-- For DailyRecipeStats, Code = DRS_YYYYMMDD (derived from StatsDate)
IF OBJECT_ID(N'[dbo].[DailyRecipeStats]', N'U') IS NOT NULL
  AND COL_LENGTH(N'[dbo].[DailyRecipeStats]', N'Code') IS NULL
BEGIN
    ALTER TABLE [dbo].[DailyRecipeStats] ADD [Code] VARCHAR(20) NULL;
END;
GO

IF OBJECT_ID(N'[dbo].[DailyRecipeStats]', N'U') IS NOT NULL
    UPDATE [dbo].[DailyRecipeStats]
    SET [Code] = 'DRS_' + CONVERT(VARCHAR(8), [StatsDate], 112)
    WHERE [Code] IS NULL;
GO

IF OBJECT_ID(N'[dbo].[DailyRecipeStats]', N'U') IS NOT NULL
    ALTER TABLE [dbo].[DailyRecipeStats] ALTER COLUMN [Code] VARCHAR(20) NOT NULL;
GO

IF OBJECT_ID(N'[dbo].[DailyRecipeStats]', N'U') IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = 'UQ_DailyRecipeStats_Code' AND object_id = OBJECT_ID(N'[dbo].[DailyRecipeStats]'))
BEGIN
    ALTER TABLE [dbo].[DailyRecipeStats] ADD CONSTRAINT [UQ_DailyRecipeStats_Code] UNIQUE ([Code]);
END;
GO


-- ─────────────────────────────────────────────────────────────────────────────
-- SKIPPED (pure junction tables — no own identity):
--   • RecipeCategory   (PK = RecipeId + FoodCategoryId)
--   • RecipeLike       (PK = RecipeId + UserId)
-- These are lookups identified by their FK pair — no Uuid/Code needed.
-- ─────────────────────────────────────────────────────────────────────────────


-- ═══════════════════════════════════════════════════════════════════════════
-- AUTO-GENERATION TRIGGER: Generate Code on INSERT for new rows
-- One trigger per table that needs a generated Code (not referentials).
-- Referential tables (Difficulty, Cuisine, etc.) keep manual Code values.
-- ═══════════════════════════════════════════════════════════════════════════

-- ── AppUser ─────────────────────────────────────────────────────────────────
IF OBJECT_ID(N'[dbo].[TR_AppUser_GenerateCode]', N'TR') IS NOT NULL
    DROP TRIGGER [dbo].[TR_AppUser_GenerateCode];
GO
CREATE TRIGGER [dbo].[TR_AppUser_GenerateCode]
ON [dbo].[AppUser] AFTER INSERT AS
BEGIN
    SET NOCOUNT ON;
    UPDATE t SET t.[Code] = 'USR_' + UPPER(LEFT(CONVERT(VARCHAR(36), NEWID()), 8))
    FROM [dbo].[AppUser] t
    INNER JOIN inserted i ON t.Id = i.Id
    WHERE t.[Code] IS NULL OR t.[Code] = '';
END;
GO

-- ── ChefProfile ─────────────────────────────────────────────────────────────
IF OBJECT_ID(N'[dbo].[TR_ChefProfile_GenerateCode]', N'TR') IS NOT NULL
    DROP TRIGGER [dbo].[TR_ChefProfile_GenerateCode];
GO
CREATE TRIGGER [dbo].[TR_ChefProfile_GenerateCode]
ON [dbo].[ChefProfile] AFTER INSERT AS
BEGIN
    SET NOCOUNT ON;
    UPDATE t SET t.[Code] = 'CHF_' + UPPER(LEFT(CONVERT(VARCHAR(36), NEWID()), 8))
    FROM [dbo].[ChefProfile] t
    INNER JOIN inserted i ON t.Id = i.Id
    WHERE t.[Code] IS NULL OR t.[Code] = '';
END;
GO

-- ── MoroccanCity ────────────────────────────────────────────────────────────
IF OBJECT_ID(N'[dbo].[TR_MoroccanCity_GenerateCode]', N'TR') IS NOT NULL
    DROP TRIGGER [dbo].[TR_MoroccanCity_GenerateCode];
GO
CREATE TRIGGER [dbo].[TR_MoroccanCity_GenerateCode]
ON [dbo].[MoroccanCity] AFTER INSERT AS
BEGIN
    SET NOCOUNT ON;
    UPDATE t SET t.[Code] = UPPER(REPLACE(t.[Slug], '-', '_'))
    FROM [dbo].[MoroccanCity] t
    INNER JOIN inserted i ON t.Id = i.Id
    WHERE t.[Code] IS NULL OR t.[Code] = '';
END;
GO

-- ── Ingredient ──────────────────────────────────────────────────────────────
IF OBJECT_ID(N'[dbo].[TR_Ingredient_GenerateCode]', N'TR') IS NOT NULL
    DROP TRIGGER [dbo].[TR_Ingredient_GenerateCode];
GO
CREATE TRIGGER [dbo].[TR_Ingredient_GenerateCode]
ON [dbo].[Ingredient] AFTER INSERT AS
BEGIN
    SET NOCOUNT ON;
    UPDATE t SET t.[Code] = 'ING_' + UPPER(LEFT(CONVERT(VARCHAR(36), NEWID()), 8))
    FROM [dbo].[Ingredient] t
    INNER JOIN inserted i ON t.Id = i.Id
    WHERE t.[Code] IS NULL OR t.[Code] = '';
END;
GO

-- ── IngredientImage ─────────────────────────────────────────────────────────
IF OBJECT_ID(N'[dbo].[TR_IngredientImage_GenerateCode]', N'TR') IS NOT NULL
    DROP TRIGGER [dbo].[TR_IngredientImage_GenerateCode];
GO
CREATE TRIGGER [dbo].[TR_IngredientImage_GenerateCode]
ON [dbo].[IngredientImage] AFTER INSERT AS
BEGIN
    SET NOCOUNT ON;
    UPDATE t SET t.[Code] = 'IGI_' + UPPER(LEFT(CONVERT(VARCHAR(36), NEWID()), 8))
    FROM [dbo].[IngredientImage] t
    INNER JOIN inserted i ON t.Id = i.Id
    WHERE t.[Code] IS NULL OR t.[Code] = '';
END;
GO

-- ── Recipe ──────────────────────────────────────────────────────────────────
IF OBJECT_ID(N'[dbo].[TR_Recipe_GenerateCode]', N'TR') IS NOT NULL
    DROP TRIGGER [dbo].[TR_Recipe_GenerateCode];
GO
CREATE TRIGGER [dbo].[TR_Recipe_GenerateCode]
ON [dbo].[Recipe] AFTER INSERT AS
BEGIN
    SET NOCOUNT ON;
    UPDATE t SET t.[Code] = 'RCP_' + UPPER(LEFT(CONVERT(VARCHAR(36), NEWID()), 8))
    FROM [dbo].[Recipe] t
    INNER JOIN inserted i ON t.Id = i.Id
    WHERE t.[Code] IS NULL OR t.[Code] = '';
END;
GO

-- ── RecipeImage ─────────────────────────────────────────────────────────────
IF OBJECT_ID(N'[dbo].[TR_RecipeImage_GenerateCode]', N'TR') IS NOT NULL
    DROP TRIGGER [dbo].[TR_RecipeImage_GenerateCode];
GO
CREATE TRIGGER [dbo].[TR_RecipeImage_GenerateCode]
ON [dbo].[RecipeImage] AFTER INSERT AS
BEGIN
    SET NOCOUNT ON;
    UPDATE t SET t.[Code] = 'RIM_' + UPPER(LEFT(CONVERT(VARCHAR(36), NEWID()), 8))
    FROM [dbo].[RecipeImage] t
    INNER JOIN inserted i ON t.Id = i.Id
    WHERE t.[Code] IS NULL OR t.[Code] = '';
END;
GO

-- ── RecipeStep ──────────────────────────────────────────────────────────────
IF OBJECT_ID(N'[dbo].[TR_RecipeStep_GenerateCode]', N'TR') IS NOT NULL
    DROP TRIGGER [dbo].[TR_RecipeStep_GenerateCode];
GO
CREATE TRIGGER [dbo].[TR_RecipeStep_GenerateCode]
ON [dbo].[RecipeStep] AFTER INSERT AS
BEGIN
    SET NOCOUNT ON;
    UPDATE t SET t.[Code] = 'RST_' + UPPER(LEFT(CONVERT(VARCHAR(36), NEWID()), 8))
    FROM [dbo].[RecipeStep] t
    INNER JOIN inserted i ON t.Id = i.Id
    WHERE t.[Code] IS NULL OR t.[Code] = '';
END;
GO

-- ── RecipeShare ─────────────────────────────────────────────────────────────
IF OBJECT_ID(N'[dbo].[TR_RecipeShare_GenerateCode]', N'TR') IS NOT NULL
    DROP TRIGGER [dbo].[TR_RecipeShare_GenerateCode];
GO
CREATE TRIGGER [dbo].[TR_RecipeShare_GenerateCode]
ON [dbo].[RecipeShare] AFTER INSERT AS
BEGIN
    SET NOCOUNT ON;
    UPDATE t SET t.[Code] = 'RSH_' + UPPER(LEFT(CONVERT(VARCHAR(36), NEWID()), 8))
    FROM [dbo].[RecipeShare] t
    INNER JOIN inserted i ON t.Id = i.Id
    WHERE t.[Code] IS NULL OR t.[Code] = '';
END;
GO

-- ── RecipeIngredient ────────────────────────────────────────────────────────
IF OBJECT_ID(N'[dbo].[TR_RecipeIngredient_GenerateCode]', N'TR') IS NOT NULL
    DROP TRIGGER [dbo].[TR_RecipeIngredient_GenerateCode];
GO
CREATE TRIGGER [dbo].[TR_RecipeIngredient_GenerateCode]
ON [dbo].[RecipeIngredient] AFTER INSERT AS
BEGIN
    SET NOCOUNT ON;
    UPDATE t SET t.[Code] = 'RIG_' + UPPER(LEFT(CONVERT(VARCHAR(36), NEWID()), 8))
    FROM [dbo].[RecipeIngredient] t
    INNER JOIN inserted i ON t.RecipeId = i.RecipeId AND t.IngredientId = i.IngredientId
    WHERE t.[Code] IS NULL OR t.[Code] = '';
END;
GO

-- ── AppUserExternalLogin (guarded — table may not exist) ────────────────────
IF OBJECT_ID(N'[dbo].[TR_AppUserExternalLogin_GenerateCode]', N'TR') IS NOT NULL
    DROP TRIGGER [dbo].[TR_AppUserExternalLogin_GenerateCode];
GO

IF OBJECT_ID(N'[dbo].[AppUserExternalLogin]', N'U') IS NOT NULL
BEGIN
    EXEC('
    CREATE TRIGGER [dbo].[TR_AppUserExternalLogin_GenerateCode]
    ON [dbo].[AppUserExternalLogin] AFTER INSERT AS
    BEGIN
        SET NOCOUNT ON;
        UPDATE t SET t.[Code] = ''EXL_'' + UPPER(LEFT(CONVERT(VARCHAR(36), NEWID()), 8))
        FROM [dbo].[AppUserExternalLogin] t
        INNER JOIN inserted i ON t.Id = i.Id
        WHERE t.[Code] IS NULL OR t.[Code] = '''';
    END;
    ');
END;
GO

-- ── AppUserRefreshToken (guarded — table may not exist) ─────────────────────
IF OBJECT_ID(N'[dbo].[TR_AppUserRefreshToken_GenerateCode]', N'TR') IS NOT NULL
    DROP TRIGGER [dbo].[TR_AppUserRefreshToken_GenerateCode];
GO

IF OBJECT_ID(N'[dbo].[AppUserRefreshToken]', N'U') IS NOT NULL
BEGIN
    EXEC('
    CREATE TRIGGER [dbo].[TR_AppUserRefreshToken_GenerateCode]
    ON [dbo].[AppUserRefreshToken] AFTER INSERT AS
    BEGIN
        SET NOCOUNT ON;
        UPDATE t SET t.[Code] = ''RTK_'' + UPPER(LEFT(CONVERT(VARCHAR(36), NEWID()), 8))
        FROM [dbo].[AppUserRefreshToken] t
        INNER JOIN inserted i ON t.Id = i.Id
        WHERE t.[Code] IS NULL OR t.[Code] = '''';
    END;
    ');
END;
GO

-- ── DailyRecipeStats (guarded — table may not exist) ────────────────────────
IF OBJECT_ID(N'[dbo].[TR_DailyRecipeStats_GenerateCode]', N'TR') IS NOT NULL
    DROP TRIGGER [dbo].[TR_DailyRecipeStats_GenerateCode];
GO

IF OBJECT_ID(N'[dbo].[DailyRecipeStats]', N'U') IS NOT NULL
BEGIN
    EXEC('
    CREATE TRIGGER [dbo].[TR_DailyRecipeStats_GenerateCode]
    ON [dbo].[DailyRecipeStats] AFTER INSERT AS
    BEGIN
        SET NOCOUNT ON;
        UPDATE t SET t.[Code] = ''DRS_'' + CONVERT(VARCHAR(8), t.[StatsDate], 112)
        FROM [dbo].[DailyRecipeStats] t
        INNER JOIN inserted i ON t.Id = i.Id
        WHERE t.[Code] IS NULL OR t.[Code] = '''';
    END;
    ');
END;
GO


PRINT '✅ Migration_Uuid_Code_AllTables applied successfully.';
GO
