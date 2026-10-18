-- ============================================================================
-- Migration: SaaS-Ready Referential Standardization
-- Date:      2026-04-04
-- Purpose:   Add IsActive, Code, SortOrder, IconUrl, Color to all referentials.
--            Replace varchar Status/Visibility with standardized enum values.
--            Add audit columns (DateInsert, DateModif) where missing.
-- ============================================================================

-- ─────────────────────────────────────────────────────────────────────────────
-- STEP 1 — Add IsActive to ALL reference tables + AppUser + ChefProfile
-- ─────────────────────────────────────────────────────────────────────────────

-- Difficulty
IF COL_LENGTH(N'[dbo].[Difficulty]', N'IsActive') IS NULL
BEGIN
    ALTER TABLE [dbo].[Difficulty]
    ADD [IsActive] [bit] NOT NULL CONSTRAINT [DF_Difficulty_IsActive] DEFAULT ((1));
END;
GO

-- FoodCategory
IF COL_LENGTH(N'[dbo].[FoodCategory]', N'IsActive') IS NULL
BEGIN
    ALTER TABLE [dbo].[FoodCategory]
    ADD [IsActive] [bit] NOT NULL CONSTRAINT [DF_FoodCategory_IsActive] DEFAULT ((1));
END;
GO

-- Cuisine
IF COL_LENGTH(N'[dbo].[Cuisine]', N'IsActive') IS NULL
BEGIN
    ALTER TABLE [dbo].[Cuisine]
    ADD [IsActive] [bit] NOT NULL CONSTRAINT [DF_Cuisine_IsActive] DEFAULT ((1));
END;
GO

-- Unit
IF COL_LENGTH(N'[dbo].[Unit]', N'IsActive') IS NULL
BEGIN
    ALTER TABLE [dbo].[Unit]
    ADD [IsActive] [bit] NOT NULL CONSTRAINT [DF_Unit_IsActive] DEFAULT ((1));
END;
GO

-- IngredientType
IF COL_LENGTH(N'[dbo].[IngredientType]', N'IsActive') IS NULL
BEGIN
    ALTER TABLE [dbo].[IngredientType]
    ADD [IsActive] [bit] NOT NULL CONSTRAINT [DF_IngredientType_IsActive] DEFAULT ((1));
END;
GO

-- MoroccanRegion
IF COL_LENGTH(N'[dbo].[MoroccanRegion]', N'IsActive') IS NULL
BEGIN
    ALTER TABLE [dbo].[MoroccanRegion]
    ADD [IsActive] [bit] NOT NULL CONSTRAINT [DF_MoroccanRegion_IsActive] DEFAULT ((1));
END;
GO

-- MoroccanCity
IF COL_LENGTH(N'[dbo].[MoroccanCity]', N'IsActive') IS NULL
BEGIN
    ALTER TABLE [dbo].[MoroccanCity]
    ADD [IsActive] [bit] NOT NULL CONSTRAINT [DF_MoroccanCity_IsActive] DEFAULT ((1));
END;
GO

-- AppUser
IF COL_LENGTH(N'[dbo].[AppUser]', N'IsActive') IS NULL
BEGIN
    ALTER TABLE [dbo].[AppUser]
    ADD [IsActive] [bit] NOT NULL CONSTRAINT [DF_AppUser_IsActive] DEFAULT ((1));
END;
GO

-- ChefProfile
IF COL_LENGTH(N'[dbo].[ChefProfile]', N'IsActive') IS NULL
BEGIN
    ALTER TABLE [dbo].[ChefProfile]
    ADD [IsActive] [bit] NOT NULL CONSTRAINT [DF_ChefProfile_IsActive] DEFAULT ((1));
END;
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- STEP 2 — Add Code (machine key) to tables that only have Name
-- ─────────────────────────────────────────────────────────────────────────────

-- Difficulty
IF COL_LENGTH(N'[dbo].[Difficulty]', N'Code') IS NULL
BEGIN
    ALTER TABLE [dbo].[Difficulty] ADD [Code] [varchar](30) NULL;
END;
GO

UPDATE [dbo].[Difficulty] SET [Code] = UPPER(REPLACE([Name], ' ', '_')) WHERE [Code] IS NULL;
GO

ALTER TABLE [dbo].[Difficulty] ALTER COLUMN [Code] [varchar](30) NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM [sys].[indexes] WHERE [name] = 'UQ_Difficulty_Code' AND [object_id] = OBJECT_ID(N'[dbo].[Difficulty]'))
BEGIN
    ALTER TABLE [dbo].[Difficulty] ADD CONSTRAINT [UQ_Difficulty_Code] UNIQUE ([Code]);
END;
GO

-- FoodCategory
IF COL_LENGTH(N'[dbo].[FoodCategory]', N'Code') IS NULL
BEGIN
    ALTER TABLE [dbo].[FoodCategory] ADD [Code] [varchar](30) NULL;
END;
GO

UPDATE [dbo].[FoodCategory] SET [Code] = UPPER(REPLACE(REPLACE([Name], '-', '_'), ' ', '_')) WHERE [Code] IS NULL;
GO

ALTER TABLE [dbo].[FoodCategory] ALTER COLUMN [Code] [varchar](30) NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM [sys].[indexes] WHERE [name] = 'UQ_FoodCategory_Code' AND [object_id] = OBJECT_ID(N'[dbo].[FoodCategory]'))
BEGIN
    ALTER TABLE [dbo].[FoodCategory] ADD CONSTRAINT [UQ_FoodCategory_Code] UNIQUE ([Code]);
END;
GO

-- Cuisine
IF COL_LENGTH(N'[dbo].[Cuisine]', N'Code') IS NULL
BEGIN
    ALTER TABLE [dbo].[Cuisine] ADD [Code] [varchar](30) NULL;
END;
GO

UPDATE [dbo].[Cuisine] SET [Code] = UPPER(REPLACE([Name], ' ', '_')) WHERE [Code] IS NULL;
GO

ALTER TABLE [dbo].[Cuisine] ALTER COLUMN [Code] [varchar](30) NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM [sys].[indexes] WHERE [name] = 'UQ_Cuisine_Code' AND [object_id] = OBJECT_ID(N'[dbo].[Cuisine]'))
BEGIN
    ALTER TABLE [dbo].[Cuisine] ADD CONSTRAINT [UQ_Cuisine_Code] UNIQUE ([Code]);
END;
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- STEP 3 — Add SortOrder where missing
-- ─────────────────────────────────────────────────────────────────────────────

IF COL_LENGTH(N'[dbo].[FoodCategory]', N'SortOrder') IS NULL
BEGIN
    ALTER TABLE [dbo].[FoodCategory] ADD [SortOrder] [int] NOT NULL CONSTRAINT [DF_FoodCategory_SortOrder] DEFAULT ((0));
END;
GO

IF COL_LENGTH(N'[dbo].[Cuisine]', N'SortOrder') IS NULL
BEGIN
    ALTER TABLE [dbo].[Cuisine] ADD [SortOrder] [int] NOT NULL CONSTRAINT [DF_Cuisine_SortOrder] DEFAULT ((0));
END;
GO

IF COL_LENGTH(N'[dbo].[Unit]', N'SortOrder') IS NULL
BEGIN
    ALTER TABLE [dbo].[Unit] ADD [SortOrder] [int] NOT NULL CONSTRAINT [DF_Unit_SortOrder] DEFAULT ((0));
END;
GO

IF COL_LENGTH(N'[dbo].[IngredientType]', N'SortOrder') IS NULL
BEGIN
    ALTER TABLE [dbo].[IngredientType] ADD [SortOrder] [int] NOT NULL CONSTRAINT [DF_IngredientType_SortOrder] DEFAULT ((0));
END;
GO

IF COL_LENGTH(N'[dbo].[MoroccanRegion]', N'SortOrder') IS NULL
BEGIN
    ALTER TABLE [dbo].[MoroccanRegion] ADD [SortOrder] [int] NOT NULL CONSTRAINT [DF_MoroccanRegion_SortOrder] DEFAULT ((0));
END;
GO

IF COL_LENGTH(N'[dbo].[MoroccanCity]', N'SortOrder') IS NULL
BEGIN
    ALTER TABLE [dbo].[MoroccanCity] ADD [SortOrder] [int] NOT NULL CONSTRAINT [DF_MoroccanCity_SortOrder] DEFAULT ((0));
END;
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- STEP 4 — Add IconUrl + Color for rich referentials
-- ─────────────────────────────────────────────────────────────────────────────

-- Difficulty
IF COL_LENGTH(N'[dbo].[Difficulty]', N'IconUrl') IS NULL
    ALTER TABLE [dbo].[Difficulty] ADD [IconUrl] [varchar](500) NULL;
GO
IF COL_LENGTH(N'[dbo].[Difficulty]', N'Color') IS NULL
    ALTER TABLE [dbo].[Difficulty] ADD [Color] [varchar](20) NULL;
GO

-- FoodCategory
IF COL_LENGTH(N'[dbo].[FoodCategory]', N'IconUrl') IS NULL
    ALTER TABLE [dbo].[FoodCategory] ADD [IconUrl] [varchar](500) NULL;
GO
IF COL_LENGTH(N'[dbo].[FoodCategory]', N'Color') IS NULL
    ALTER TABLE [dbo].[FoodCategory] ADD [Color] [varchar](20) NULL;
GO

-- Cuisine
IF COL_LENGTH(N'[dbo].[Cuisine]', N'IconUrl') IS NULL
    ALTER TABLE [dbo].[Cuisine] ADD [IconUrl] [varchar](500) NULL;
GO
IF COL_LENGTH(N'[dbo].[Cuisine]', N'Color') IS NULL
    ALTER TABLE [dbo].[Cuisine] ADD [Color] [varchar](20) NULL;
GO

-- IngredientType
IF COL_LENGTH(N'[dbo].[IngredientType]', N'IconUrl') IS NULL
    ALTER TABLE [dbo].[IngredientType] ADD [IconUrl] [varchar](500) NULL;
GO
IF COL_LENGTH(N'[dbo].[IngredientType]', N'Color') IS NULL
    ALTER TABLE [dbo].[IngredientType] ADD [Color] [varchar](20) NULL;
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- STEP 5 — Standardize Status: PUBLISH → PUBLISHED, normalize Visibility
--          Order: DROP constraints → UPDATE data → RE-ADD constraints
-- ─────────────────────────────────────────────────────────────────────────────

-- 5a. Drop old Status CHECK first (blocks UPDATE otherwise)
IF EXISTS (SELECT 1 FROM [sys].[check_constraints] WHERE [name] = 'CK_Recipe_Status' AND [parent_object_id] = OBJECT_ID(N'[dbo].[Recipe]'))
BEGIN
    ALTER TABLE [dbo].[Recipe] DROP CONSTRAINT [CK_Recipe_Status];
END;
GO

-- 5b. Drop old Status DEFAULT
IF EXISTS (SELECT 1 FROM [sys].[default_constraints] WHERE [name] = 'DF_Recipe_Status' AND [parent_object_id] = OBJECT_ID(N'[dbo].[Recipe]'))
BEGIN
    ALTER TABLE [dbo].[Recipe] DROP CONSTRAINT [DF_Recipe_Status];
END;
GO

-- 5c. Now safe to migrate data
UPDATE [dbo].[Recipe]
SET [Status] = CASE UPPER(LTRIM(RTRIM([Status])))
    WHEN 'PUBLISH'   THEN 'PUBLISHED'
    WHEN 'PUBLISHED' THEN 'PUBLISHED'
    WHEN 'DRAFT'     THEN 'DRAFT'
    WHEN 'ARCHIVE'   THEN 'ARCHIVED'
    WHEN 'ARCHIVED'  THEN 'ARCHIVED'
    ELSE 'DRAFT'
END
WHERE [Status] IS NOT NULL;
GO

-- 5d. Re-add constraints with new values
ALTER TABLE [dbo].[Recipe] ADD CONSTRAINT [DF_Recipe_Status] DEFAULT ('DRAFT') FOR [Status];
GO

ALTER TABLE [dbo].[Recipe] WITH CHECK ADD CONSTRAINT [CK_Recipe_Status] CHECK ([Status] IN ('PUBLISHED', 'DRAFT', 'ARCHIVED'));
GO

-- 5e. Drop old Visibility DEFAULT + CHECK first
IF EXISTS (SELECT 1 FROM [sys].[check_constraints] WHERE [name] = 'CK_Recipe_Visibility' AND [parent_object_id] = OBJECT_ID(N'[dbo].[Recipe]'))
BEGIN
    ALTER TABLE [dbo].[Recipe] DROP CONSTRAINT [CK_Recipe_Visibility];
END;
GO

IF EXISTS (SELECT 1 FROM [sys].[default_constraints] WHERE [name] = 'DF_Recipe_Visibility' AND [parent_object_id] = OBJECT_ID(N'[dbo].[Recipe]'))
BEGIN
    ALTER TABLE [dbo].[Recipe] DROP CONSTRAINT [DF_Recipe_Visibility];
END;
GO

-- 5f. Now safe to migrate Visibility data
UPDATE [dbo].[Recipe]
SET [Visibility] = CASE LTRIM(RTRIM([Visibility]))
    WHEN 'Publique' THEN 'PUBLIC'
    WHEN 'PUBLIC'   THEN 'PUBLIC'
    WHEN 'Prive'    THEN 'PRIVATE'
    WHEN 'PRIVATE'  THEN 'PRIVATE'
    WHEN 'UNLISTED' THEN 'UNLISTED'
    ELSE 'PUBLIC'
END
WHERE [Visibility] IS NOT NULL;
GO

-- 5g. Re-add Visibility constraints with new values
ALTER TABLE [dbo].[Recipe] ADD CONSTRAINT [DF_Recipe_Visibility] DEFAULT ('PUBLIC') FOR [Visibility];
GO

ALTER TABLE [dbo].[Recipe] WITH CHECK ADD CONSTRAINT [CK_Recipe_Visibility] CHECK ([Visibility] IN ('PUBLIC', 'PRIVATE', 'UNLISTED'));
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- STEP 6 — Add DateInsert / DateModif audit columns where missing
-- ─────────────────────────────────────────────────────────────────────────────

-- Difficulty
IF COL_LENGTH(N'[dbo].[Difficulty]', N'DateInsert') IS NULL
    ALTER TABLE [dbo].[Difficulty] ADD [DateInsert] [smalldatetime] NOT NULL CONSTRAINT [DF_Difficulty_DateInsert] DEFAULT (GETDATE());
GO
IF COL_LENGTH(N'[dbo].[Difficulty]', N'DateModif') IS NULL
    ALTER TABLE [dbo].[Difficulty] ADD [DateModif] [smalldatetime] NULL;
GO

-- FoodCategory
IF COL_LENGTH(N'[dbo].[FoodCategory]', N'DateInsert') IS NULL
    ALTER TABLE [dbo].[FoodCategory] ADD [DateInsert] [smalldatetime] NOT NULL CONSTRAINT [DF_FoodCategory_DateInsert] DEFAULT (GETDATE());
GO
IF COL_LENGTH(N'[dbo].[FoodCategory]', N'DateModif') IS NULL
    ALTER TABLE [dbo].[FoodCategory] ADD [DateModif] [smalldatetime] NULL;
GO

-- Cuisine
IF COL_LENGTH(N'[dbo].[Cuisine]', N'DateInsert') IS NULL
    ALTER TABLE [dbo].[Cuisine] ADD [DateInsert] [smalldatetime] NOT NULL CONSTRAINT [DF_Cuisine_DateInsert] DEFAULT (GETDATE());
GO
IF COL_LENGTH(N'[dbo].[Cuisine]', N'DateModif') IS NULL
    ALTER TABLE [dbo].[Cuisine] ADD [DateModif] [smalldatetime] NULL;
GO

-- Unit
IF COL_LENGTH(N'[dbo].[Unit]', N'DateInsert') IS NULL
    ALTER TABLE [dbo].[Unit] ADD [DateInsert] [smalldatetime] NOT NULL CONSTRAINT [DF_Unit_DateInsert] DEFAULT (GETDATE());
GO
IF COL_LENGTH(N'[dbo].[Unit]', N'DateModif') IS NULL
    ALTER TABLE [dbo].[Unit] ADD [DateModif] [smalldatetime] NULL;
GO

-- IngredientType
IF COL_LENGTH(N'[dbo].[IngredientType]', N'DateInsert') IS NULL
    ALTER TABLE [dbo].[IngredientType] ADD [DateInsert] [smalldatetime] NOT NULL CONSTRAINT [DF_IngredientType_DateInsert] DEFAULT (GETDATE());
GO
IF COL_LENGTH(N'[dbo].[IngredientType]', N'DateModif') IS NULL
    ALTER TABLE [dbo].[IngredientType] ADD [DateModif] [smalldatetime] NULL;
GO

-- MoroccanRegion
IF COL_LENGTH(N'[dbo].[MoroccanRegion]', N'DateInsert') IS NULL
    ALTER TABLE [dbo].[MoroccanRegion] ADD [DateInsert] [smalldatetime] NOT NULL CONSTRAINT [DF_MoroccanRegion_DateInsert] DEFAULT (GETDATE());
GO
IF COL_LENGTH(N'[dbo].[MoroccanRegion]', N'DateModif') IS NULL
    ALTER TABLE [dbo].[MoroccanRegion] ADD [DateModif] [smalldatetime] NULL;
GO

-- MoroccanCity
IF COL_LENGTH(N'[dbo].[MoroccanCity]', N'DateInsert') IS NULL
    ALTER TABLE [dbo].[MoroccanCity] ADD [DateInsert] [smalldatetime] NOT NULL CONSTRAINT [DF_MoroccanCity_DateInsert] DEFAULT (GETDATE());
GO
IF COL_LENGTH(N'[dbo].[MoroccanCity]', N'DateModif') IS NULL
    ALTER TABLE [dbo].[MoroccanCity] ADD [DateModif] [smalldatetime] NULL;
GO

-- ChefProfile (missing DateModif)
IF COL_LENGTH(N'[dbo].[ChefProfile]', N'DateModif') IS NULL
    ALTER TABLE [dbo].[ChefProfile] ADD [DateModif] [smalldatetime] NULL;
GO

PRINT 'Migration completed successfully.';
GO
