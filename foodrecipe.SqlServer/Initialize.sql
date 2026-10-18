-- Foodiya development baseline: current schema and repository example data.
-- Run once against an EMPTY database using SSMS, Azure SQL query editor, or sqlcmd -b.
-- Identity uses its own EF Core migrations in a separate database.
-- This script does not create/drop databases or upgrade existing schemas.
SET NOCOUNT ON;
SET XACT_ABORT ON;
IF EXISTS (SELECT 1 FROM sys.tables WHERE is_ms_shipped = 0)
BEGIN
    RAISERROR ('Foodiya baseline requires an empty database.', 16, 1);
    SET NOEXEC ON;
END;
GO
BEGIN TRANSACTION;
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[IngredientType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Code] [varchar](30) NOT NULL,
	[Label] [varchar](80) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[SortOrder] [int] NOT NULL,
	[IconUrl] [varchar](500) NULL,
	[Color] [varchar](20) NULL,
	[DateInsert] [smalldatetime] NOT NULL,
	[DateModif] [smalldatetime] NULL,
	[Uuid] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_IngredientType] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[IdentityInboxMessage](
	[EventId] [uniqueidentifier] NOT NULL,
	[EventType] [varchar](100) NOT NULL,
	[ProcessedAtUtc] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_IdentityInboxMessage] PRIMARY KEY CLUSTERED
(
	[EventId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FoodCategory](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](100) NOT NULL,
	[Description] [varchar](250) NULL,
	[IsActive] [bit] NOT NULL,
	[Code] [varchar](30) NOT NULL,
	[SortOrder] [int] NOT NULL,
	[IconUrl] [varchar](500) NULL,
	[Color] [varchar](20) NULL,
	[DateInsert] [smalldatetime] NOT NULL,
	[DateModif] [smalldatetime] NULL,
	[Uuid] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_FoodCategory] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Difficulty](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](50) NOT NULL,
	[SortOrder] [int] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[Code] [varchar](30) NOT NULL,
	[IconUrl] [varchar](500) NULL,
	[Color] [varchar](20) NULL,
	[DateInsert] [smalldatetime] NOT NULL,
	[DateModif] [smalldatetime] NULL,
	[Uuid] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_Difficulty] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DailyRecipeStats](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[StatsDate] [datetime2](7) NOT NULL,
	[GeneratedAtUtc] [datetime2](0) NOT NULL,
	[TotalRecipes] [int] NOT NULL,
	[TotalPublishedRecipes] [int] NOT NULL,
	[NewRecipesToday] [int] NOT NULL,
	[TotalLikesToday] [int] NOT NULL,
	[TopLikedRecipesJson] [nvarchar](max) NULL,
	[RecentRecipesJson] [nvarchar](max) NULL,
	[CuisineBreakdownJson] [nvarchar](max) NULL,
	[DifficultyBreakdownJson] [nvarchar](max) NULL,
	[Uuid] [uniqueidentifier] NOT NULL,
	[Code] [varchar](20) NOT NULL,
	[ImageUrl] [varchar](500) NOT NULL,
 CONSTRAINT [PK_DailyRecipeStats] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Cuisine](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](100) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[Code] [varchar](30) NOT NULL,
	[SortOrder] [int] NOT NULL,
	[IconUrl] [varchar](500) NULL,
	[Color] [varchar](20) NULL,
	[DateInsert] [smalldatetime] NOT NULL,
	[DateModif] [smalldatetime] NULL,
	[Uuid] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_Cuisine] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PresetAvatarImage](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Code] [varchar](50) NOT NULL,
	[Label] [varchar](100) NOT NULL,
	[ImageUrl] [varchar](500) NOT NULL,
	[BackgroundColor] [varchar](20) NULL,
	[SortOrder] [int] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[Uuid] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_PresetAvatarImage] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MoroccanRegion](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Code] [varchar](20) NOT NULL,
	[Name] [varchar](150) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[SortOrder] [int] NOT NULL,
	[DateInsert] [smalldatetime] NOT NULL,
	[DateModif] [smalldatetime] NULL,
	[Uuid] [uniqueidentifier] NOT NULL,
	[ImageUrl] [varchar](500) NOT NULL,
 CONSTRAINT [PK_MoroccanRegion] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Unit](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Code] [varchar](20) NOT NULL,
	[Label] [varchar](50) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[SortOrder] [int] NOT NULL,
	[DateInsert] [smalldatetime] NOT NULL,
	[DateModif] [smalldatetime] NULL,
	[Uuid] [uniqueidentifier] NOT NULL,
	[ImageUrl] [varchar](500) NOT NULL,
 CONSTRAINT [PK_Unit] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Ingredient](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](150) NOT NULL,
	[IngredientTypeId] [int] NOT NULL,
	[DefaultUnitId] [int] NULL,
	[CaloriesPer100g] [decimal](10, 2) NULL,
	[IsActive] [bit] NOT NULL,
	[Uuid] [uniqueidentifier] NOT NULL,
	[Code] [varchar](20) NOT NULL,
	[ImageUrl] [varchar](500) NOT NULL,
 CONSTRAINT [PK_Ingredient] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MoroccanCity](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RegionId] [int] NOT NULL,
	[Name] [varchar](150) NOT NULL,
	[Slug] [varchar](170) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[SortOrder] [int] NOT NULL,
	[DateInsert] [smalldatetime] NOT NULL,
	[DateModif] [smalldatetime] NULL,
	[Uuid] [uniqueidentifier] NOT NULL,
	[Code] [varchar](30) NOT NULL,
	[ImageUrl] [varchar](500) NOT NULL,
 CONSTRAINT [PK_MoroccanCity] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AppUser](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserName] [varchar](50) NOT NULL,
	[FirstName] [nvarchar](100) NOT NULL,
	[LastName] [nvarchar](100) NOT NULL,
	[Email] [nvarchar](256) NOT NULL,
	[PasswordHash] [varbinary](1024) NOT NULL,
	[PasswordSalt] [varbinary](1024) NOT NULL,
	[ProfileImageUrl] [varchar](500) NULL,
	[PresetAvatarImageId] [int] NULL,
	[DateInsert] [smalldatetime] NOT NULL,
	[DateModif] [smalldatetime] NULL,
	[DeletedAt] [smalldatetime] NULL,
	[DeletedByUserId] [int] NULL,
	[DeleteReason] [varchar](500) NOT NULL,
	[Role] [varchar](20) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[Uuid] [uniqueidentifier] NOT NULL,
	[Code] [varchar](20) NOT NULL,
	[Color] [varchar](20) NULL,
 CONSTRAINT [PK_AppUser] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ChefProfile](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[DisplayName] [varchar](120) NOT NULL,
	[Bio] [varchar](1000) NULL,
	[Specialty] [varchar](150) NULL,
	[YearsOfExperience] [int] NULL,
	[IsVerified] [bit] NOT NULL,
	[DateInsert] [smalldatetime] NOT NULL,
	[DeletedAt] [smalldatetime] NULL,
	[DeletedByUserId] [int] NULL,
	[DeleteReason] [varchar](500) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[DateModif] [smalldatetime] NULL,
	[Uuid] [uniqueidentifier] NOT NULL,
	[Code] [varchar](20) NOT NULL,
	[Color] [varchar](20) NULL,
	[ImageUrl] [varchar](500) NOT NULL,
 CONSTRAINT [PK_ChefProfile] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AppUserRefreshToken](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AppUserId] [int] NOT NULL,
	[TokenHash] [varchar](128) NOT NULL,
	[ExpiresAtUtc] [datetime2](7) NOT NULL,
	[CreatedAtUtc] [datetime2](7) NOT NULL,
	[RevokedAtUtc] [datetime2](7) NULL,
	[ReplacedByTokenHash] [varchar](128) NULL,
	[Uuid] [uniqueidentifier] NOT NULL,
	[Code] [varchar](20) NOT NULL,
 CONSTRAINT [PK_AppUserRefreshToken] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AppUserExternalLogin](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AppUserId] [int] NOT NULL,
	[Provider] [varchar](50) NOT NULL,
	[ProviderSubject] [varchar](200) NOT NULL,
	[DateInsert] [smalldatetime] NOT NULL,
	[LastSignInAt] [smalldatetime] NOT NULL,
	[Uuid] [uniqueidentifier] NOT NULL,
	[Code] [varchar](20) NOT NULL,
 CONSTRAINT [PK_AppUserExternalLogin] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[IngredientImage](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[IngredientId] [int] NOT NULL,
	[ImageUrl] [varchar](500) NOT NULL,
	[AltText] [varchar](250) NULL,
	[IsPrimary] [bit] NOT NULL,
	[SortOrder] [int] NOT NULL,
	[Uuid] [uniqueidentifier] NOT NULL,
	[Code] [varchar](20) NOT NULL,
 CONSTRAINT [PK_IngredientImage] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Recipe](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ChefId] [int] NOT NULL,
	[DifficultyId] [int] NOT NULL,
	[CuisineId] [int] NULL,
	[CityId] [int] NULL,
	[Title] [varchar](200) NOT NULL,
	[Slug] [varchar](250) NOT NULL,
	[Summary] [varchar](1000) NULL,
	[PrepTimeMinutes] [int] NOT NULL,
	[CookTimeMinutes] [int] NOT NULL,
	[Servings] [int] NOT NULL,
	[CoverImageUrl] [varchar](500) NULL,
	[Status] [varchar](20) NOT NULL,
	[Visibility] [varchar](20) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[DateInsert] [smalldatetime] NOT NULL,
	[DateModif] [smalldatetime] NULL,
	[PublishedAt] [smalldatetime] NULL,
	[DeletedAt] [smalldatetime] NULL,
	[DeletedByUserId] [int] NULL,
	[DeleteReason] [varchar](500) NOT NULL,
	[Uuid] [uniqueidentifier] NOT NULL,
	[Code] [varchar](20) NOT NULL,
	[Color] [varchar](20) NULL,
 CONSTRAINT [PK_Recipe] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecipeStep](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RecipeId] [int] NOT NULL,
	[StepNumber] [int] NOT NULL,
	[Title] [varchar](150) NULL,
	[Instruction] [varchar](2000) NOT NULL,
	[DurationMinutes] [int] NULL,
	[Uuid] [uniqueidentifier] NOT NULL,
	[Code] [varchar](20) NOT NULL,
	[ImageUrl] [varchar](500) NULL,
 CONSTRAINT [PK_RecipeStep] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecipeShare](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RecipeId] [int] NOT NULL,
	[SharedByUserId] [int] NOT NULL,
	[SharedWithUserId] [int] NULL,
	[ShareChannel] [varchar](30) NOT NULL,
	[ShareMessage] [varchar](500) NULL,
	[SharedAt] [smalldatetime] NOT NULL,
	[Uuid] [uniqueidentifier] NOT NULL,
	[Code] [varchar](20) NOT NULL,
	[ImageUrl] [varchar](500) NULL,
 CONSTRAINT [PK_RecipeShare] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecipeNutrition](
	[RecipeId] [int] NOT NULL,
	[CaloriesPerServing] [decimal](10, 2) NOT NULL,
	[ProteinGrams] [decimal](10, 2) NULL,
	[CarbsGrams] [decimal](10, 2) NULL,
	[FatGrams] [decimal](10, 2) NULL,
	[Uuid] [uniqueidentifier] NOT NULL,
	[Color] [varchar](20) NULL,
	[ImageUrl] [varchar](500) NOT NULL,
 CONSTRAINT [PK_RecipeNutrition] PRIMARY KEY CLUSTERED
(
	[RecipeId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecipeLike](
	[RecipeId] [int] NOT NULL,
	[UserId] [int] NOT NULL,
	[LikedAt] [smalldatetime] NOT NULL,
 CONSTRAINT [PK_RecipeLike] PRIMARY KEY CLUSTERED
(
	[RecipeId] ASC,
	[UserId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecipeIngredient](
	[RecipeId] [int] NOT NULL,
	[IngredientId] [int] NOT NULL,
	[Quantity] [decimal](10, 2) NOT NULL,
	[UnitId] [int] NULL,
	[IsOptional] [bit] NOT NULL,
	[Notes] [varchar](250) NULL,
	[SortOrder] [int] NOT NULL,
	[Uuid] [uniqueidentifier] NOT NULL,
	[Code] [varchar](20) NOT NULL,
	[ImageUrl] [varchar](500) NULL,
 CONSTRAINT [PK_RecipeIngredient] PRIMARY KEY CLUSTERED
(
	[RecipeId] ASC,
	[IngredientId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecipeImage](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RecipeId] [int] NOT NULL,
	[ImageUrl] [varchar](500) NOT NULL,
	[AltText] [varchar](250) NULL,
	[IsPrimary] [bit] NOT NULL,
	[SortOrder] [int] NOT NULL,
	[Uuid] [uniqueidentifier] NOT NULL,
	[Code] [varchar](20) NOT NULL,
 CONSTRAINT [PK_RecipeImage] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecipeCategory](
	[RecipeId] [int] NOT NULL,
	[FoodCategoryId] [int] NOT NULL,
 CONSTRAINT [PK_RecipeCategory] PRIMARY KEY CLUSTERED
(
	[RecipeId] ASC,
	[FoodCategoryId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
SET IDENTITY_INSERT [dbo].[IngredientType] ON

GO
INSERT [dbo].[IngredientType] ([Id], [Code], [Label], [IsActive], [SortOrder], [IconUrl], [Color], [DateInsert], [DateModif], [Uuid]) VALUES (1, N'PRINCIPAL', N'Ingredient principal', 1, 0, N'https://images.unsplash.com/photo-1506976785307-8732e854ad03?w=400&fit=crop', NULL, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'c6b03346-13cb-f111-b4f1-c858b3a6df82')
GO
INSERT [dbo].[IngredientType] ([Id], [Code], [Label], [IsActive], [SortOrder], [IconUrl], [Color], [DateInsert], [DateModif], [Uuid]) VALUES (2, N'ASSAISONNEMENT', N'Assaisonnement', 1, 0, N'https://images.unsplash.com/photo-1506976785307-8732e854ad03?w=400&fit=crop', NULL, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'c4b03346-13cb-f111-b4f1-c858b3a6df82')
GO
INSERT [dbo].[IngredientType] ([Id], [Code], [Label], [IsActive], [SortOrder], [IconUrl], [Color], [DateInsert], [DateModif], [Uuid]) VALUES (3, N'SAUCE', N'Sauce', 1, 0, N'https://images.unsplash.com/photo-1506976785307-8732e854ad03?w=400&fit=crop', NULL, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'c7b03346-13cb-f111-b4f1-c858b3a6df82')
GO
INSERT [dbo].[IngredientType] ([Id], [Code], [Label], [IsActive], [SortOrder], [IconUrl], [Color], [DateInsert], [DateModif], [Uuid]) VALUES (4, N'GARNITURE', N'Garniture', 1, 0, N'https://images.unsplash.com/photo-1506976785307-8732e854ad03?w=400&fit=crop', NULL, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'c5b03346-13cb-f111-b4f1-c858b3a6df82')
GO
SET IDENTITY_INSERT [dbo].[IngredientType] OFF
GO
SET IDENTITY_INSERT [dbo].[FoodCategory] ON

GO
INSERT [dbo].[FoodCategory] ([Id], [Name], [Description], [IsActive], [Code], [SortOrder], [IconUrl], [Color], [DateInsert], [DateModif], [Uuid]) VALUES (1, N'Petit-dejeuner', N'Repas du matin', 1, N'PETIT_DEJEUNER', 0, N'https://images.unsplash.com/photo-1476224203421-9ac39bcb3327?w=400&fit=crop', NULL, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'60b03346-13cb-f111-b4f1-c858b3a6df82')
GO
INSERT [dbo].[FoodCategory] ([Id], [Name], [Description], [IsActive], [Code], [SortOrder], [IconUrl], [Color], [DateInsert], [DateModif], [Uuid]) VALUES (2, N'Dejeuner', N'Repas du midi', 1, N'DEJEUNER', 0, N'https://images.unsplash.com/photo-1476224203421-9ac39bcb3327?w=400&fit=crop', NULL, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'5db03346-13cb-f111-b4f1-c858b3a6df82')
GO
INSERT [dbo].[FoodCategory] ([Id], [Name], [Description], [IsActive], [Code], [SortOrder], [IconUrl], [Color], [DateInsert], [DateModif], [Uuid]) VALUES (3, N'Diner', N'Repas du soir', 1, N'DINER', 0, N'https://images.unsplash.com/photo-1476224203421-9ac39bcb3327?w=400&fit=crop', NULL, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'5fb03346-13cb-f111-b4f1-c858b3a6df82')
GO
INSERT [dbo].[FoodCategory] ([Id], [Name], [Description], [IsActive], [Code], [SortOrder], [IconUrl], [Color], [DateInsert], [DateModif], [Uuid]) VALUES (4, N'Salade', N'Salades froides ou chaudes', 1, N'SALADE', 0, N'https://images.unsplash.com/photo-1476224203421-9ac39bcb3327?w=400&fit=crop', NULL, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'62b03346-13cb-f111-b4f1-c858b3a6df82')
GO
INSERT [dbo].[FoodCategory] ([Id], [Name], [Description], [IsActive], [Code], [SortOrder], [IconUrl], [Color], [DateInsert], [DateModif], [Uuid]) VALUES (5, N'Plat principal', N'Plats principaux', 1, N'PLAT_PRINCIPAL', 0, N'https://images.unsplash.com/photo-1476224203421-9ac39bcb3327?w=400&fit=crop', NULL, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'61b03346-13cb-f111-b4f1-c858b3a6df82')
GO
INSERT [dbo].[FoodCategory] ([Id], [Name], [Description], [IsActive], [Code], [SortOrder], [IconUrl], [Color], [DateInsert], [DateModif], [Uuid]) VALUES (6, N'Soupe', N'Soupes et bouillons', 1, N'SOUPE', 0, N'https://images.unsplash.com/photo-1476224203421-9ac39bcb3327?w=400&fit=crop', NULL, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'63b03346-13cb-f111-b4f1-c858b3a6df82')
GO
INSERT [dbo].[FoodCategory] ([Id], [Name], [Description], [IsActive], [Code], [SortOrder], [IconUrl], [Color], [DateInsert], [DateModif], [Uuid]) VALUES (7, N'Dessert', N'Preparations sucrees', 1, N'DESSERT', 0, N'https://images.unsplash.com/photo-1476224203421-9ac39bcb3327?w=400&fit=crop', NULL, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'5eb03346-13cb-f111-b4f1-c858b3a6df82')
GO
INSERT [dbo].[FoodCategory] ([Id], [Name], [Description], [IsActive], [Code], [SortOrder], [IconUrl], [Color], [DateInsert], [DateModif], [Uuid]) VALUES (8, N'Boisson', N'Jus, smoothies, cocktails et boissons chaudes', 1, N'BOISSON', 0, N'https://images.unsplash.com/photo-1476224203421-9ac39bcb3327?w=400&fit=crop', NULL, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'5bb03346-13cb-f111-b4f1-c858b3a6df82')
GO
INSERT [dbo].[FoodCategory] ([Id], [Name], [Description], [IsActive], [Code], [SortOrder], [IconUrl], [Color], [DateInsert], [DateModif], [Uuid]) VALUES (9, N'Collation', N'Petite faim entre les repas', 1, N'COLLATION', 0, N'https://images.unsplash.com/photo-1476224203421-9ac39bcb3327?w=400&fit=crop', NULL, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'5cb03346-13cb-f111-b4f1-c858b3a6df82')
GO
SET IDENTITY_INSERT [dbo].[FoodCategory] OFF
GO
SET IDENTITY_INSERT [dbo].[Difficulty] ON

GO
INSERT [dbo].[Difficulty] ([Id], [Name], [SortOrder], [IsActive], [Code], [IconUrl], [Color], [DateInsert], [DateModif], [Uuid]) VALUES (1, N'Difficile', 3, 1, N'DIFFICILE', N'https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?w=400&fit=crop', NULL, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'56b03346-13cb-f111-b4f1-c858b3a6df82')
GO
INSERT [dbo].[Difficulty] ([Id], [Name], [SortOrder], [IsActive], [Code], [IconUrl], [Color], [DateInsert], [DateModif], [Uuid]) VALUES (2, N'Exotic', 5, 1, N'EXOTIC', N'https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?w=400&fit=crop', NULL, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'57b03346-13cb-f111-b4f1-c858b3a6df82')
GO
INSERT [dbo].[Difficulty] ([Id], [Name], [SortOrder], [IsActive], [Code], [IconUrl], [Color], [DateInsert], [DateModif], [Uuid]) VALUES (3, N'Facile', 1, 1, N'FACILE', N'https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?w=400&fit=crop', NULL, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'58b03346-13cb-f111-b4f1-c858b3a6df82')
GO
INSERT [dbo].[Difficulty] ([Id], [Name], [SortOrder], [IsActive], [Code], [IconUrl], [Color], [DateInsert], [DateModif], [Uuid]) VALUES (4, N'Moyen', 2, 1, N'MOYEN', N'https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?w=400&fit=crop', NULL, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'59b03346-13cb-f111-b4f1-c858b3a6df82')
GO
INSERT [dbo].[Difficulty] ([Id], [Name], [SortOrder], [IsActive], [Code], [IconUrl], [Color], [DateInsert], [DateModif], [Uuid]) VALUES (5, N'Tres difficile', 4, 1, N'TRES_DIFFICILE', N'https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?w=400&fit=crop', NULL, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'5ab03346-13cb-f111-b4f1-c858b3a6df82')
GO
SET IDENTITY_INSERT [dbo].[Difficulty] OFF
GO
SET IDENTITY_INSERT [dbo].[Cuisine] ON

GO
INSERT [dbo].[Cuisine] ([Id], [Name], [IsActive], [Code], [SortOrder], [IconUrl], [Color], [DateInsert], [DateModif], [Uuid]) VALUES (1, N'Internationale', 1, N'INTERNATIONALE', 0, N'https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=400&fit=crop', NULL, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'66b03346-13cb-f111-b4f1-c858b3a6df82')
GO
INSERT [dbo].[Cuisine] ([Id], [Name], [IsActive], [Code], [SortOrder], [IconUrl], [Color], [DateInsert], [DateModif], [Uuid]) VALUES (2, N'Italienne', 1, N'ITALIENNE', 0, N'https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=400&fit=crop', NULL, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'67b03346-13cb-f111-b4f1-c858b3a6df82')
GO
INSERT [dbo].[Cuisine] ([Id], [Name], [IsActive], [Code], [SortOrder], [IconUrl], [Color], [DateInsert], [DateModif], [Uuid]) VALUES (3, N'Francaise', 1, N'FRANCAISE', 0, N'https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=400&fit=crop', NULL, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'64b03346-13cb-f111-b4f1-c858b3a6df82')
GO
INSERT [dbo].[Cuisine] ([Id], [Name], [IsActive], [Code], [SortOrder], [IconUrl], [Color], [DateInsert], [DateModif], [Uuid]) VALUES (4, N'Marocaine', 1, N'MAROCAINE', 0, N'https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=400&fit=crop', NULL, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'69b03346-13cb-f111-b4f1-c858b3a6df82')
GO
INSERT [dbo].[Cuisine] ([Id], [Name], [IsActive], [Code], [SortOrder], [IconUrl], [Color], [DateInsert], [DateModif], [Uuid]) VALUES (5, N'Mexicaine', 1, N'MEXICAINE', 0, N'https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=400&fit=crop', NULL, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'6ab03346-13cb-f111-b4f1-c858b3a6df82')
GO
INSERT [dbo].[Cuisine] ([Id], [Name], [IsActive], [Code], [SortOrder], [IconUrl], [Color], [DateInsert], [DateModif], [Uuid]) VALUES (6, N'Indienne', 1, N'INDIENNE', 0, N'https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=400&fit=crop', NULL, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'65b03346-13cb-f111-b4f1-c858b3a6df82')
GO
INSERT [dbo].[Cuisine] ([Id], [Name], [IsActive], [Code], [SortOrder], [IconUrl], [Color], [DateInsert], [DateModif], [Uuid]) VALUES (7, N'Japonaise', 1, N'JAPONAISE', 0, N'https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=400&fit=crop', NULL, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'68b03346-13cb-f111-b4f1-c858b3a6df82')
GO
SET IDENTITY_INSERT [dbo].[Cuisine] OFF
GO
SET IDENTITY_INSERT [dbo].[PresetAvatarImage] ON

GO
INSERT [dbo].[PresetAvatarImage] ([Id], [Code], [Label], [ImageUrl], [BackgroundColor], [SortOrder], [IsActive], [Uuid]) VALUES (1, N'avatar-chef-rouge', N'Chef Rouge', N'https://example.com/avatars/avatar-chef-rouge.png', N'#FDE2E4', 1, 1, N'54b03346-13cb-f111-b4f1-c858b3a6df82')
GO
INSERT [dbo].[PresetAvatarImage] ([Id], [Code], [Label], [ImageUrl], [BackgroundColor], [SortOrder], [IsActive], [Uuid]) VALUES (2, N'avatar-chef-bleu', N'Chef Bleu', N'https://example.com/avatars/avatar-chef-bleu.png', N'#DCEBFF', 2, 1, N'50b03346-13cb-f111-b4f1-c858b3a6df82')
GO
INSERT [dbo].[PresetAvatarImage] ([Id], [Code], [Label], [ImageUrl], [BackgroundColor], [SortOrder], [IsActive], [Uuid]) VALUES (3, N'avatar-chef-vert', N'Chef Vert', N'https://example.com/avatars/avatar-chef-vert.png', N'#DDF3E4', 3, 1, N'55b03346-13cb-f111-b4f1-c858b3a6df82')
GO
INSERT [dbo].[PresetAvatarImage] ([Id], [Code], [Label], [ImageUrl], [BackgroundColor], [SortOrder], [IsActive], [Uuid]) VALUES (4, N'avatar-chef-jaune', N'Chef Jaune', N'https://example.com/avatars/avatar-chef-jaune.png', N'#FFF0C9', 4, 1, N'51b03346-13cb-f111-b4f1-c858b3a6df82')
GO
INSERT [dbo].[PresetAvatarImage] ([Id], [Code], [Label], [ImageUrl], [BackgroundColor], [SortOrder], [IsActive], [Uuid]) VALUES (5, N'avatar-chef-rose', N'Chef Rose', N'https://example.com/avatars/avatar-chef-rose.png', N'#F9D9E8', 5, 1, N'53b03346-13cb-f111-b4f1-c858b3a6df82')
GO
INSERT [dbo].[PresetAvatarImage] ([Id], [Code], [Label], [ImageUrl], [BackgroundColor], [SortOrder], [IsActive], [Uuid]) VALUES (6, N'avatar-chef-noir', N'Chef Noir', N'https://example.com/avatars/avatar-chef-noir.png', N'#E3E3E3', 6, 1, N'52b03346-13cb-f111-b4f1-c858b3a6df82')
GO
SET IDENTITY_INSERT [dbo].[PresetAvatarImage] OFF
GO
SET IDENTITY_INSERT [dbo].[MoroccanRegion] ON

GO
INSERT [dbo].[MoroccanRegion] ([Id], [Code], [Name], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [ImageUrl]) VALUES (1, N'TTA', N'Tanger-Tetouan-Al Hoceima', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'76b03346-13cb-f111-b4f1-c858b3a6df82', N'https://images.unsplash.com/photo-1548017544-2e7a30ae42ca?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanRegion] ([Id], [Code], [Name], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [ImageUrl]) VALUES (2, N'ORI', N'Oriental', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'73b03346-13cb-f111-b4f1-c858b3a6df82', N'https://images.unsplash.com/photo-1548017544-2e7a30ae42ca?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanRegion] ([Id], [Code], [Name], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [ImageUrl]) VALUES (3, N'FM', N'Fes-Meknes', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'6fb03346-13cb-f111-b4f1-c858b3a6df82', N'https://images.unsplash.com/photo-1548017544-2e7a30ae42ca?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanRegion] ([Id], [Code], [Name], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [ImageUrl]) VALUES (4, N'RSK', N'Rabat-Sale-Kenitra', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'74b03346-13cb-f111-b4f1-c858b3a6df82', N'https://images.unsplash.com/photo-1548017544-2e7a30ae42ca?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanRegion] ([Id], [Code], [Name], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [ImageUrl]) VALUES (5, N'BMK', N'Beni Mellal-Khenifra', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'6bb03346-13cb-f111-b4f1-c858b3a6df82', N'https://images.unsplash.com/photo-1548017544-2e7a30ae42ca?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanRegion] ([Id], [Code], [Name], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [ImageUrl]) VALUES (6, N'CS', N'Casablanca-Settat', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'6cb03346-13cb-f111-b4f1-c858b3a6df82', N'https://images.unsplash.com/photo-1548017544-2e7a30ae42ca?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanRegion] ([Id], [Code], [Name], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [ImageUrl]) VALUES (7, N'MS', N'Marrakech-Safi', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'72b03346-13cb-f111-b4f1-c858b3a6df82', N'https://images.unsplash.com/photo-1548017544-2e7a30ae42ca?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanRegion] ([Id], [Code], [Name], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [ImageUrl]) VALUES (8, N'DT', N'Draa-Tafilalet', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'6eb03346-13cb-f111-b4f1-c858b3a6df82', N'https://images.unsplash.com/photo-1548017544-2e7a30ae42ca?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanRegion] ([Id], [Code], [Name], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [ImageUrl]) VALUES (9, N'SM', N'Souss-Massa', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'75b03346-13cb-f111-b4f1-c858b3a6df82', N'https://images.unsplash.com/photo-1548017544-2e7a30ae42ca?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanRegion] ([Id], [Code], [Name], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [ImageUrl]) VALUES (10, N'GON', N'Guelmim-Oued Noun', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'70b03346-13cb-f111-b4f1-c858b3a6df82', N'https://images.unsplash.com/photo-1548017544-2e7a30ae42ca?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanRegion] ([Id], [Code], [Name], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [ImageUrl]) VALUES (11, N'LSH', N'Laayoune-Sakia El Hamra', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'71b03346-13cb-f111-b4f1-c858b3a6df82', N'https://images.unsplash.com/photo-1548017544-2e7a30ae42ca?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanRegion] ([Id], [Code], [Name], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [ImageUrl]) VALUES (12, N'DOE', N'Dakhla-Oued Ed-Dahab', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'6db03346-13cb-f111-b4f1-c858b3a6df82', N'https://images.unsplash.com/photo-1548017544-2e7a30ae42ca?w=400&fit=crop')
GO
SET IDENTITY_INSERT [dbo].[MoroccanRegion] OFF
GO
SET IDENTITY_INSERT [dbo].[Unit] ON

GO
INSERT [dbo].[Unit] ([Id], [Code], [Label], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [ImageUrl]) VALUES (1, N'g', N'Gramme', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'beb03346-13cb-f111-b4f1-c858b3a6df82', N'https://images.unsplash.com/photo-1466637574441-749b8f19452f?w=400&fit=crop')
GO
INSERT [dbo].[Unit] ([Id], [Code], [Label], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [ImageUrl]) VALUES (2, N'kg', N'Kilogramme', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'bfb03346-13cb-f111-b4f1-c858b3a6df82', N'https://images.unsplash.com/photo-1466637574441-749b8f19452f?w=400&fit=crop')
GO
INSERT [dbo].[Unit] ([Id], [Code], [Label], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [ImageUrl]) VALUES (3, N'ml', N'Millilitre', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'c1b03346-13cb-f111-b4f1-c858b3a6df82', N'https://images.unsplash.com/photo-1466637574441-749b8f19452f?w=400&fit=crop')
GO
INSERT [dbo].[Unit] ([Id], [Code], [Label], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [ImageUrl]) VALUES (4, N'l', N'Litre', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'c0b03346-13cb-f111-b4f1-c858b3a6df82', N'https://images.unsplash.com/photo-1466637574441-749b8f19452f?w=400&fit=crop')
GO
INSERT [dbo].[Unit] ([Id], [Code], [Label], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [ImageUrl]) VALUES (5, N'cc', N'Cuillere a cafe', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'bcb03346-13cb-f111-b4f1-c858b3a6df82', N'https://images.unsplash.com/photo-1466637574441-749b8f19452f?w=400&fit=crop')
GO
INSERT [dbo].[Unit] ([Id], [Code], [Label], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [ImageUrl]) VALUES (6, N'cs', N'Cuillere a soupe', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'bdb03346-13cb-f111-b4f1-c858b3a6df82', N'https://images.unsplash.com/photo-1466637574441-749b8f19452f?w=400&fit=crop')
GO
INSERT [dbo].[Unit] ([Id], [Code], [Label], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [ImageUrl]) VALUES (7, N'tasse', N'Tasse', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'c3b03346-13cb-f111-b4f1-c858b3a6df82', N'https://images.unsplash.com/photo-1466637574441-749b8f19452f?w=400&fit=crop')
GO
INSERT [dbo].[Unit] ([Id], [Code], [Label], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [ImageUrl]) VALUES (8, N'piece', N'Piece', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'c2b03346-13cb-f111-b4f1-c858b3a6df82', N'https://images.unsplash.com/photo-1466637574441-749b8f19452f?w=400&fit=crop')
GO
SET IDENTITY_INSERT [dbo].[Unit] OFF
GO
SET IDENTITY_INSERT [dbo].[Ingredient] ON

GO
INSERT [dbo].[Ingredient] ([Id], [Name], [IngredientTypeId], [DefaultUnitId], [CaloriesPer100g], [IsActive], [Uuid], [Code], [ImageUrl]) VALUES (1, N'Tomate', 1, 1, CAST(18.00 AS Decimal(10, 2)), 1, N'ddb03346-13cb-f111-b4f1-c858b3a6df82', N'ING_AA3E507D', N'https://images.unsplash.com/photo-1506976785307-8732e854ad03?w=400&fit=crop')
GO
INSERT [dbo].[Ingredient] ([Id], [Name], [IngredientTypeId], [DefaultUnitId], [CaloriesPer100g], [IsActive], [Uuid], [Code], [ImageUrl]) VALUES (2, N'Concombre', 1, 1, CAST(15.00 AS Decimal(10, 2)), 1, N'cbb03346-13cb-f111-b4f1-c858b3a6df82', N'ING_708827D5', N'https://images.unsplash.com/photo-1506976785307-8732e854ad03?w=400&fit=crop')
GO
INSERT [dbo].[Ingredient] ([Id], [Name], [IngredientTypeId], [DefaultUnitId], [CaloriesPer100g], [IsActive], [Uuid], [Code], [ImageUrl]) VALUES (3, N'Laitue', 1, 1, CAST(15.00 AS Decimal(10, 2)), 1, N'd3b03346-13cb-f111-b4f1-c858b3a6df82', N'ING_4E925D9A', N'https://images.unsplash.com/photo-1506976785307-8732e854ad03?w=400&fit=crop')
GO
INSERT [dbo].[Ingredient] ([Id], [Name], [IngredientTypeId], [DefaultUnitId], [CaloriesPer100g], [IsActive], [Uuid], [Code], [ImageUrl]) VALUES (4, N'Fromage feta', 1, 1, CAST(264.00 AS Decimal(10, 2)), 1, N'cfb03346-13cb-f111-b4f1-c858b3a6df82', N'ING_91A020FA', N'https://images.unsplash.com/photo-1506976785307-8732e854ad03?w=400&fit=crop')
GO
INSERT [dbo].[Ingredient] ([Id], [Name], [IngredientTypeId], [DefaultUnitId], [CaloriesPer100g], [IsActive], [Uuid], [Code], [ImageUrl]) VALUES (5, N'Avocat', 1, 8, CAST(160.00 AS Decimal(10, 2)), 1, N'c8b03346-13cb-f111-b4f1-c858b3a6df82', N'ING_F769323A', N'https://images.unsplash.com/photo-1506976785307-8732e854ad03?w=400&fit=crop')
GO
INSERT [dbo].[Ingredient] ([Id], [Name], [IngredientTypeId], [DefaultUnitId], [CaloriesPer100g], [IsActive], [Uuid], [Code], [ImageUrl]) VALUES (6, N'Pain', 1, 8, CAST(265.00 AS Decimal(10, 2)), 1, N'd9b03346-13cb-f111-b4f1-c858b3a6df82', N'ING_4B838CA9', N'https://images.unsplash.com/photo-1506976785307-8732e854ad03?w=400&fit=crop')
GO
INSERT [dbo].[Ingredient] ([Id], [Name], [IngredientTypeId], [DefaultUnitId], [CaloriesPer100g], [IsActive], [Uuid], [Code], [ImageUrl]) VALUES (7, N'Oeuf', 1, 8, CAST(155.00 AS Decimal(10, 2)), 1, N'd8b03346-13cb-f111-b4f1-c858b3a6df82', N'ING_43B537C3', N'https://images.unsplash.com/photo-1506976785307-8732e854ad03?w=400&fit=crop')
GO
INSERT [dbo].[Ingredient] ([Id], [Name], [IngredientTypeId], [DefaultUnitId], [CaloriesPer100g], [IsActive], [Uuid], [Code], [ImageUrl]) VALUES (8, N'Lait', 1, 3, CAST(42.00 AS Decimal(10, 2)), 1, N'd2b03346-13cb-f111-b4f1-c858b3a6df82', N'ING_459F83E0', N'https://images.unsplash.com/photo-1506976785307-8732e854ad03?w=400&fit=crop')
GO
INSERT [dbo].[Ingredient] ([Id], [Name], [IngredientTypeId], [DefaultUnitId], [CaloriesPer100g], [IsActive], [Uuid], [Code], [ImageUrl]) VALUES (9, N'Banane', 1, 8, CAST(89.00 AS Decimal(10, 2)), 1, N'c9b03346-13cb-f111-b4f1-c858b3a6df82', N'ING_FD745FAF', N'https://images.unsplash.com/photo-1506976785307-8732e854ad03?w=400&fit=crop')
GO
INSERT [dbo].[Ingredient] ([Id], [Name], [IngredientTypeId], [DefaultUnitId], [CaloriesPer100g], [IsActive], [Uuid], [Code], [ImageUrl]) VALUES (10, N'Mangue', 1, 8, CAST(60.00 AS Decimal(10, 2)), 1, N'd5b03346-13cb-f111-b4f1-c858b3a6df82', N'ING_E63208BF', N'https://images.unsplash.com/photo-1506976785307-8732e854ad03?w=400&fit=crop')
GO
INSERT [dbo].[Ingredient] ([Id], [Name], [IngredientTypeId], [DefaultUnitId], [CaloriesPer100g], [IsActive], [Uuid], [Code], [ImageUrl]) VALUES (11, N'Yaourt', 1, 1, CAST(59.00 AS Decimal(10, 2)), 1, N'deb03346-13cb-f111-b4f1-c858b3a6df82', N'ING_343ADC35', N'https://images.unsplash.com/photo-1506976785307-8732e854ad03?w=400&fit=crop')
GO
INSERT [dbo].[Ingredient] ([Id], [Name], [IngredientTypeId], [DefaultUnitId], [CaloriesPer100g], [IsActive], [Uuid], [Code], [ImageUrl]) VALUES (12, N'Flocons d avoine', 1, 1, CAST(389.00 AS Decimal(10, 2)), 1, N'cdb03346-13cb-f111-b4f1-c858b3a6df82', N'ING_C4A4C3C0', N'https://images.unsplash.com/photo-1506976785307-8732e854ad03?w=400&fit=crop')
GO
INSERT [dbo].[Ingredient] ([Id], [Name], [IngredientTypeId], [DefaultUnitId], [CaloriesPer100g], [IsActive], [Uuid], [Code], [ImageUrl]) VALUES (13, N'Farine', 1, 1, CAST(364.00 AS Decimal(10, 2)), 1, N'ccb03346-13cb-f111-b4f1-c858b3a6df82', N'ING_E2FBA029', N'https://images.unsplash.com/photo-1506976785307-8732e854ad03?w=400&fit=crop')
GO
INSERT [dbo].[Ingredient] ([Id], [Name], [IngredientTypeId], [DefaultUnitId], [CaloriesPer100g], [IsActive], [Uuid], [Code], [ImageUrl]) VALUES (14, N'Beurre', 1, 1, CAST(717.00 AS Decimal(10, 2)), 1, N'cab03346-13cb-f111-b4f1-c858b3a6df82', N'ING_CCB9C969', N'https://images.unsplash.com/photo-1506976785307-8732e854ad03?w=400&fit=crop')
GO
INSERT [dbo].[Ingredient] ([Id], [Name], [IngredientTypeId], [DefaultUnitId], [CaloriesPer100g], [IsActive], [Uuid], [Code], [ImageUrl]) VALUES (15, N'Fraises', 1, 1, CAST(32.00 AS Decimal(10, 2)), 1, N'ceb03346-13cb-f111-b4f1-c858b3a6df82', N'ING_BDC94BAA', N'https://images.unsplash.com/photo-1506976785307-8732e854ad03?w=400&fit=crop')
GO
INSERT [dbo].[Ingredient] ([Id], [Name], [IngredientTypeId], [DefaultUnitId], [CaloriesPer100g], [IsActive], [Uuid], [Code], [ImageUrl]) VALUES (16, N'Sel', 2, 5, CAST(0.00 AS Decimal(10, 2)), 1, N'dbb03346-13cb-f111-b4f1-c858b3a6df82', N'ING_BC46302B', N'https://images.unsplash.com/photo-1506976785307-8732e854ad03?w=400&fit=crop')
GO
INSERT [dbo].[Ingredient] ([Id], [Name], [IngredientTypeId], [DefaultUnitId], [CaloriesPer100g], [IsActive], [Uuid], [Code], [ImageUrl]) VALUES (17, N'Poivre noir', 2, 5, CAST(251.00 AS Decimal(10, 2)), 1, N'dab03346-13cb-f111-b4f1-c858b3a6df82', N'ING_D3677C8E', N'https://images.unsplash.com/photo-1506976785307-8732e854ad03?w=400&fit=crop')
GO
INSERT [dbo].[Ingredient] ([Id], [Name], [IngredientTypeId], [DefaultUnitId], [CaloriesPer100g], [IsActive], [Uuid], [Code], [ImageUrl]) VALUES (18, N'Sucre', 2, 1, CAST(387.00 AS Decimal(10, 2)), 1, N'dcb03346-13cb-f111-b4f1-c858b3a6df82', N'ING_30F6C263', N'https://images.unsplash.com/photo-1506976785307-8732e854ad03?w=400&fit=crop')
GO
INSERT [dbo].[Ingredient] ([Id], [Name], [IngredientTypeId], [DefaultUnitId], [CaloriesPer100g], [IsActive], [Uuid], [Code], [ImageUrl]) VALUES (19, N'Levure chimique', 2, 1, CAST(53.00 AS Decimal(10, 2)), 1, N'd4b03346-13cb-f111-b4f1-c858b3a6df82', N'ING_3C7A3CED', N'https://images.unsplash.com/photo-1506976785307-8732e854ad03?w=400&fit=crop')
GO
INSERT [dbo].[Ingredient] ([Id], [Name], [IngredientTypeId], [DefaultUnitId], [CaloriesPer100g], [IsActive], [Uuid], [Code], [ImageUrl]) VALUES (20, N'Huile d olive', 3, 3, CAST(884.00 AS Decimal(10, 2)), 1, N'd0b03346-13cb-f111-b4f1-c858b3a6df82', N'ING_9A7F096D', N'https://images.unsplash.com/photo-1506976785307-8732e854ad03?w=400&fit=crop')
GO
INSERT [dbo].[Ingredient] ([Id], [Name], [IngredientTypeId], [DefaultUnitId], [CaloriesPer100g], [IsActive], [Uuid], [Code], [ImageUrl]) VALUES (21, N'Jus de citron', 3, 3, CAST(22.00 AS Decimal(10, 2)), 1, N'd1b03346-13cb-f111-b4f1-c858b3a6df82', N'ING_CDFCC9FE', N'https://images.unsplash.com/photo-1506976785307-8732e854ad03?w=400&fit=crop')
GO
INSERT [dbo].[Ingredient] ([Id], [Name], [IngredientTypeId], [DefaultUnitId], [CaloriesPer100g], [IsActive], [Uuid], [Code], [ImageUrl]) VALUES (22, N'Miel', 3, 6, CAST(304.00 AS Decimal(10, 2)), 1, N'd7b03346-13cb-f111-b4f1-c858b3a6df82', N'ING_2F0CC762', N'https://images.unsplash.com/photo-1506976785307-8732e854ad03?w=400&fit=crop')
GO
INSERT [dbo].[Ingredient] ([Id], [Name], [IngredientTypeId], [DefaultUnitId], [CaloriesPer100g], [IsActive], [Uuid], [Code], [ImageUrl]) VALUES (23, N'Menthe', 4, 1, CAST(44.00 AS Decimal(10, 2)), 1, N'd6b03346-13cb-f111-b4f1-c858b3a6df82', N'ING_7CC5BBFE', N'https://images.unsplash.com/photo-1506976785307-8732e854ad03?w=400&fit=crop')
GO
SET IDENTITY_INSERT [dbo].[Ingredient] OFF
GO
SET IDENTITY_INSERT [dbo].[MoroccanCity] ON

GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (1, 1, N'Tanger', N'tanger', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'77b03346-13cb-f111-b4f1-c858b3a6df82', N'TANGER', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (2, 1, N'Tetouan', N'tetouan', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'78b03346-13cb-f111-b4f1-c858b3a6df82', N'TETOUAN', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (3, 1, N'Al Hoceima', N'al-hoceima', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'79b03346-13cb-f111-b4f1-c858b3a6df82', N'AL_HOCEIMA', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (4, 1, N'Larache', N'larache', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'7ab03346-13cb-f111-b4f1-c858b3a6df82', N'LARACHE', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (5, 1, N'Ksar El Kebir', N'ksar-el-kebir', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'7bb03346-13cb-f111-b4f1-c858b3a6df82', N'KSAR_EL_KEBIR', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (6, 1, N'Chefchaouen', N'chefchaouen', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'7cb03346-13cb-f111-b4f1-c858b3a6df82', N'CHEFCHAOUEN', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (7, 1, N'Asilah', N'asilah', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'7db03346-13cb-f111-b4f1-c858b3a6df82', N'ASILAH', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (8, 1, N'Martil', N'martil', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'7eb03346-13cb-f111-b4f1-c858b3a6df82', N'MARTIL', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (9, 2, N'Oujda', N'oujda', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'7fb03346-13cb-f111-b4f1-c858b3a6df82', N'OUJDA', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (10, 2, N'Nador', N'nador', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'80b03346-13cb-f111-b4f1-c858b3a6df82', N'NADOR', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (11, 2, N'Berkane', N'berkane', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'81b03346-13cb-f111-b4f1-c858b3a6df82', N'BERKANE', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (12, 2, N'Taourirt', N'taourirt', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'82b03346-13cb-f111-b4f1-c858b3a6df82', N'TAOURIRT', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (13, 2, N'Jerada', N'jerada', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'83b03346-13cb-f111-b4f1-c858b3a6df82', N'JERADA', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (14, 2, N'Saidia', N'saidia', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'84b03346-13cb-f111-b4f1-c858b3a6df82', N'SAIDIA', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (15, 3, N'Fes', N'fes', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'85b03346-13cb-f111-b4f1-c858b3a6df82', N'FES', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (16, 3, N'Meknes', N'meknes', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'86b03346-13cb-f111-b4f1-c858b3a6df82', N'MEKNES', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (17, 3, N'Taza', N'taza', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'87b03346-13cb-f111-b4f1-c858b3a6df82', N'TAZA', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (18, 3, N'Sefrou', N'sefrou', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'88b03346-13cb-f111-b4f1-c858b3a6df82', N'SEFROU', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (19, 3, N'Ifrane', N'ifrane', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'89b03346-13cb-f111-b4f1-c858b3a6df82', N'IFRANE', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (20, 3, N'Azrou', N'azrou', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'8ab03346-13cb-f111-b4f1-c858b3a6df82', N'AZROU', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (21, 4, N'Rabat', N'rabat', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'8bb03346-13cb-f111-b4f1-c858b3a6df82', N'RABAT', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (22, 4, N'Sale', N'sale', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'8cb03346-13cb-f111-b4f1-c858b3a6df82', N'SALE', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (23, 4, N'Kenitra', N'kenitra', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'8db03346-13cb-f111-b4f1-c858b3a6df82', N'KENITRA', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (24, 4, N'Temara', N'temara', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'8eb03346-13cb-f111-b4f1-c858b3a6df82', N'TEMARA', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (25, 4, N'Skhirat', N'skhirat', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'8fb03346-13cb-f111-b4f1-c858b3a6df82', N'SKHIRAT', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (26, 4, N'Tiflet', N'tiflet', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'90b03346-13cb-f111-b4f1-c858b3a6df82', N'TIFLET', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (27, 4, N'Sidi Kacem', N'sidi-kacem', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'91b03346-13cb-f111-b4f1-c858b3a6df82', N'SIDI_KACEM', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (28, 5, N'Beni Mellal', N'beni-mellal', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'92b03346-13cb-f111-b4f1-c858b3a6df82', N'BENI_MELLAL', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (29, 5, N'Khenifra', N'khenifra', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'93b03346-13cb-f111-b4f1-c858b3a6df82', N'KHENIFRA', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (30, 5, N'Khouribga', N'khouribga', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'94b03346-13cb-f111-b4f1-c858b3a6df82', N'KHOURIBGA', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (31, 5, N'Fkih Ben Salah', N'fkih-ben-salah', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'95b03346-13cb-f111-b4f1-c858b3a6df82', N'FKIH_BEN_SALAH', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (32, 5, N'Azilal', N'azilal', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'96b03346-13cb-f111-b4f1-c858b3a6df82', N'AZILAL', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (33, 5, N'Oued Zem', N'oued-zem', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'97b03346-13cb-f111-b4f1-c858b3a6df82', N'OUED_ZEM', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (34, 6, N'Casablanca', N'casablanca', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'98b03346-13cb-f111-b4f1-c858b3a6df82', N'CASABLANCA', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (35, 6, N'Mohammedia', N'mohammedia', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'99b03346-13cb-f111-b4f1-c858b3a6df82', N'MOHAMMEDIA', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (36, 6, N'Settat', N'settat', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'9ab03346-13cb-f111-b4f1-c858b3a6df82', N'SETTAT', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (37, 6, N'El Jadida', N'el-jadida', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'9bb03346-13cb-f111-b4f1-c858b3a6df82', N'EL_JADIDA', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (38, 6, N'Berrechid', N'berrechid', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'9cb03346-13cb-f111-b4f1-c858b3a6df82', N'BERRECHID', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (39, 6, N'Benslimane', N'benslimane', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'9db03346-13cb-f111-b4f1-c858b3a6df82', N'BENSLIMANE', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (40, 6, N'Dar Bouazza', N'dar-bouazza', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'9eb03346-13cb-f111-b4f1-c858b3a6df82', N'DAR_BOUAZZA', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (41, 7, N'Marrakech', N'marrakech', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'9fb03346-13cb-f111-b4f1-c858b3a6df82', N'MARRAKECH', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (42, 7, N'Safi', N'safi', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'a0b03346-13cb-f111-b4f1-c858b3a6df82', N'SAFI', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (43, 7, N'Essaouira', N'essaouira', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'a1b03346-13cb-f111-b4f1-c858b3a6df82', N'ESSAOUIRA', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (44, 7, N'Ben Guerir', N'ben-guerir', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'a2b03346-13cb-f111-b4f1-c858b3a6df82', N'BEN_GUERIR', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (45, 7, N'Youssoufia', N'youssoufia', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'a3b03346-13cb-f111-b4f1-c858b3a6df82', N'YOUSSOUFIA', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (46, 7, N'Chichaoua', N'chichaoua', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'a4b03346-13cb-f111-b4f1-c858b3a6df82', N'CHICHAOUA', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (47, 8, N'Ouarzazate', N'ouarzazate', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'a5b03346-13cb-f111-b4f1-c858b3a6df82', N'OUARZAZATE', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (48, 8, N'Errachidia', N'errachidia', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'a6b03346-13cb-f111-b4f1-c858b3a6df82', N'ERRACHIDIA', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (49, 8, N'Midelt', N'midelt', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'a7b03346-13cb-f111-b4f1-c858b3a6df82', N'MIDELT', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (50, 8, N'Tinghir', N'tinghir', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'a8b03346-13cb-f111-b4f1-c858b3a6df82', N'TINGHIR', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (51, 8, N'Zagora', N'zagora', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'a9b03346-13cb-f111-b4f1-c858b3a6df82', N'ZAGORA', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (52, 8, N'Rissani', N'rissani', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'aab03346-13cb-f111-b4f1-c858b3a6df82', N'RISSANI', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (53, 9, N'Agadir', N'agadir', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'abb03346-13cb-f111-b4f1-c858b3a6df82', N'AGADIR', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (54, 9, N'Inezgane', N'inezgane', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'acb03346-13cb-f111-b4f1-c858b3a6df82', N'INEZGANE', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (55, 9, N'Ait Melloul', N'ait-melloul', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'adb03346-13cb-f111-b4f1-c858b3a6df82', N'AIT_MELLOUL', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (56, 9, N'Taroudant', N'taroudant', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'aeb03346-13cb-f111-b4f1-c858b3a6df82', N'TAROUDANT', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (57, 9, N'Tiznit', N'tiznit', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'afb03346-13cb-f111-b4f1-c858b3a6df82', N'TIZNIT', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (58, 9, N'Tata', N'tata', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'b0b03346-13cb-f111-b4f1-c858b3a6df82', N'TATA', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (59, 10, N'Guelmim', N'guelmim', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'b1b03346-13cb-f111-b4f1-c858b3a6df82', N'GUELMIM', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (60, 10, N'Tan-Tan', N'tan-tan', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'b2b03346-13cb-f111-b4f1-c858b3a6df82', N'TAN_TAN', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (61, 10, N'Sidi Ifni', N'sidi-ifni', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'b3b03346-13cb-f111-b4f1-c858b3a6df82', N'SIDI_IFNI', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (62, 10, N'Assa', N'assa', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'b4b03346-13cb-f111-b4f1-c858b3a6df82', N'ASSA', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (63, 11, N'Laayoune', N'laayoune', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'b5b03346-13cb-f111-b4f1-c858b3a6df82', N'LAAYOUNE', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (64, 11, N'Boujdour', N'boujdour', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'b6b03346-13cb-f111-b4f1-c858b3a6df82', N'BOUJDOUR', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (65, 11, N'Tarfaya', N'tarfaya', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'b7b03346-13cb-f111-b4f1-c858b3a6df82', N'TARFAYA', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (66, 11, N'Es-Smara', N'es-smara', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'b8b03346-13cb-f111-b4f1-c858b3a6df82', N'ES_SMARA', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (67, 12, N'Dakhla', N'dakhla', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'b9b03346-13cb-f111-b4f1-c858b3a6df82', N'DAKHLA', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (68, 12, N'Aousserd', N'aousserd', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'bab03346-13cb-f111-b4f1-c858b3a6df82', N'AOUSSERD', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
INSERT [dbo].[MoroccanCity] ([Id], [RegionId], [Name], [Slug], [IsActive], [SortOrder], [DateInsert], [DateModif], [Uuid], [Code], [ImageUrl]) VALUES (69, 12, N'Bir Gandouz', N'bir-gandouz', 1, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, N'bbb03346-13cb-f111-b4f1-c858b3a6df82', N'BIR_GANDOUZ', N'https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop')
GO
SET IDENTITY_INSERT [dbo].[MoroccanCity] OFF
GO
SET IDENTITY_INSERT [dbo].[AppUser] ON

GO
INSERT [dbo].[AppUser] ([Id], [UserName], [FirstName], [LastName], [Email], [PasswordHash], [PasswordSalt], [ProfileImageUrl], [PresetAvatarImageId], [DateInsert], [DateModif], [DeletedAt], [DeletedByUserId], [DeleteReason], [Role], [IsActive], [Uuid], [Code], [Color]) VALUES (1, N'chef_LOL', N'LOL', N'Alaoui', N'LOL@example.com', 0x2CBB346C16B552B4E1F1FD8625BB69513BAA4624179A4555F00C8A80C1737039, 0xD30ECBF67A0A320A094852F7749AA8FB, N'https://images.unsplash.com/photo-1511367461989-f85a21fda167?w=400&fit=crop', 2, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, NULL, NULL, N'', N'CHEF', 1, N'48b03346-13cb-f111-b4f1-c858b3a6df82', N'USR_39B3BF5A', NULL)
GO
INSERT [dbo].[AppUser] ([Id], [UserName], [FirstName], [LastName], [Email], [PasswordHash], [PasswordSalt], [ProfileImageUrl], [PresetAvatarImageId], [DateInsert], [DateModif], [DeletedAt], [DeletedByUserId], [DeleteReason], [Role], [IsActive], [Uuid], [Code], [Color]) VALUES (2, N'nora_gourmande', N'Nora', N'Karim', N'nora@example.com', 0x8FD9DE2B79768097B5FD85D79E835372F0B41675DB0711E9D5F0C7623755C154, 0x013A8D07CC91A531097A7491991F922C, N'https://images.unsplash.com/photo-1511367461989-f85a21fda167?w=400&fit=crop', 4, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, NULL, NULL, N'', N'USER', 1, N'4bb03346-13cb-f111-b4f1-c858b3a6df82', N'USR_89539284', NULL)
GO
INSERT [dbo].[AppUser] ([Id], [UserName], [FirstName], [LastName], [Email], [PasswordHash], [PasswordSalt], [ProfileImageUrl], [PresetAvatarImageId], [DateInsert], [DateModif], [DeletedAt], [DeletedByUserId], [DeleteReason], [Role], [IsActive], [Uuid], [Code], [Color]) VALUES (3, N'chef_samia', N'Samia', N'Bennani', N'samia@example.com', 0xD668DB854A70F8BD3DAF405020793D4473DEDE6F73A800847B706B556FBDB2BE, 0x62649150219B91828157808840623BAD, N'https://images.unsplash.com/photo-1511367461989-f85a21fda167?w=400&fit=crop', 5, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, NULL, NULL, N'', N'SUPER_ADMIN', 1, N'4ab03346-13cb-f111-b4f1-c858b3a6df82', N'USR_ADB6E4A7', NULL)
GO
INSERT [dbo].[AppUser] ([Id], [UserName], [FirstName], [LastName], [Email], [PasswordHash], [PasswordSalt], [ProfileImageUrl], [PresetAvatarImageId], [DateInsert], [DateModif], [DeletedAt], [DeletedByUserId], [DeleteReason], [Role], [IsActive], [Uuid], [Code], [Color]) VALUES (4, N'sara_saveurs', N'Sara', N'El Idrissi', N'sara@example.com', 0xDC36DD400A8463B5EBD7630614FC0F77F4C5D70A2C25FE6EA759E25D36CC015F, 0x947E9956DBFF3EEB694461B3BBE84152, N'https://images.unsplash.com/photo-1511367461989-f85a21fda167?w=400&fit=crop', 1, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, NULL, NULL, N'', N'CHEF', 1, N'49b03346-13cb-f111-b4f1-c858b3a6df82', N'USR_C8FA5F72', NULL)
GO
INSERT [dbo].[AppUser] ([Id], [UserName], [FirstName], [LastName], [Email], [PasswordHash], [PasswordSalt], [ProfileImageUrl], [PresetAvatarImageId], [DateInsert], [DateModif], [DeletedAt], [DeletedByUserId], [DeleteReason], [Role], [IsActive], [Uuid], [Code], [Color]) VALUES (5, N'adam_cuisine', N'Adam', N'Lahlou', N'adam@example.com', 0x310571D0A48835F7FB88041DE5EA30E3CA9EF5300C64F13FD0B87542A02FC2CD, 0x592A7D6B2BF28F9C5CF88A6F7592DEF4, N'https://images.unsplash.com/photo-1511367461989-f85a21fda167?w=400&fit=crop', 3, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, NULL, NULL, N'', N'USER', 1, N'4cb03346-13cb-f111-b4f1-c858b3a6df82', N'USR_793BDE94', NULL)
GO
SET IDENTITY_INSERT [dbo].[AppUser] OFF
GO
SET IDENTITY_INSERT [dbo].[ChefProfile] ON

GO
INSERT [dbo].[ChefProfile] ([Id], [UserId], [DisplayName], [Bio], [Specialty], [YearsOfExperience], [IsVerified], [DateInsert], [DeletedAt], [DeletedByUserId], [DeleteReason], [IsActive], [DateModif], [Uuid], [Code], [Color], [ImageUrl]) VALUES (1, 1, N'Chef LOL', N'Createur de brunchs et de repas rapides.', N'Petit-dejeuner', 8, 1, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, NULL, N'', 1, NULL, N'4db03346-13cb-f111-b4f1-c858b3a6df82', N'CHF_A1BEF665', NULL, N'https://images.unsplash.com/photo-1577219491135-ce391730fb2c?w=400&fit=crop')
GO
INSERT [dbo].[ChefProfile] ([Id], [UserId], [DisplayName], [Bio], [Specialty], [YearsOfExperience], [IsVerified], [DateInsert], [DeletedAt], [DeletedByUserId], [DeleteReason], [IsActive], [DateModif], [Uuid], [Code], [Color], [ImageUrl]) VALUES (2, 3, N'Chef Samia', N'Cheffe maison orientee repas frais et equilibres.', N'Salades et boissons', 6, 1, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, NULL, N'', 1, NULL, N'4eb03346-13cb-f111-b4f1-c858b3a6df82', N'CHF_9D3783F7', NULL, N'https://images.unsplash.com/photo-1577219491135-ce391730fb2c?w=400&fit=crop')
GO
INSERT [dbo].[ChefProfile] ([Id], [UserId], [DisplayName], [Bio], [Specialty], [YearsOfExperience], [IsVerified], [DateInsert], [DeletedAt], [DeletedByUserId], [DeleteReason], [IsActive], [DateModif], [Uuid], [Code], [Color], [ImageUrl]) VALUES (3, 4, N'Chef Sara', N'Passionnee de desserts simples et modernes.', N'Desserts et collations', 5, 0, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, NULL, N'', 1, NULL, N'4fb03346-13cb-f111-b4f1-c858b3a6df82', N'CHF_8F70A1E7', NULL, N'https://images.unsplash.com/photo-1577219491135-ce391730fb2c?w=400&fit=crop')
GO
SET IDENTITY_INSERT [dbo].[ChefProfile] OFF
GO
SET IDENTITY_INSERT [dbo].[IngredientImage] ON

GO
INSERT [dbo].[IngredientImage] ([Id], [IngredientId], [ImageUrl], [AltText], [IsPrimary], [SortOrder], [Uuid], [Code]) VALUES (1, 1, N'https://example.com/images/ingredients/tomate.jpg', N'Tomate fraiche', 1, 1, N'dfb03346-13cb-f111-b4f1-c858b3a6df82', N'IGI_65D7E56F')
GO
INSERT [dbo].[IngredientImage] ([Id], [IngredientId], [ImageUrl], [AltText], [IsPrimary], [SortOrder], [Uuid], [Code]) VALUES (2, 2, N'https://example.com/images/ingredients/concombre.jpg', N'Concombre frais', 1, 1, N'e0b03346-13cb-f111-b4f1-c858b3a6df82', N'IGI_3F32419E')
GO
INSERT [dbo].[IngredientImage] ([Id], [IngredientId], [ImageUrl], [AltText], [IsPrimary], [SortOrder], [Uuid], [Code]) VALUES (3, 3, N'https://example.com/images/ingredients/laitue.jpg', N'Laitue verte', 1, 1, N'e1b03346-13cb-f111-b4f1-c858b3a6df82', N'IGI_264CE020')
GO
INSERT [dbo].[IngredientImage] ([Id], [IngredientId], [ImageUrl], [AltText], [IsPrimary], [SortOrder], [Uuid], [Code]) VALUES (4, 4, N'https://example.com/images/ingredients/feta.jpg', N'Fromage feta emiette', 1, 1, N'e2b03346-13cb-f111-b4f1-c858b3a6df82', N'IGI_7B8EBA93')
GO
INSERT [dbo].[IngredientImage] ([Id], [IngredientId], [ImageUrl], [AltText], [IsPrimary], [SortOrder], [Uuid], [Code]) VALUES (5, 20, N'https://example.com/images/ingredients/huile-olive.jpg', N'Huile d olive', 1, 1, N'f2b03346-13cb-f111-b4f1-c858b3a6df82', N'IGI_FB66BB7A')
GO
INSERT [dbo].[IngredientImage] ([Id], [IngredientId], [ImageUrl], [AltText], [IsPrimary], [SortOrder], [Uuid], [Code]) VALUES (6, 21, N'https://example.com/images/ingredients/jus-citron.jpg', N'Jus de citron frais', 1, 1, N'f3b03346-13cb-f111-b4f1-c858b3a6df82', N'IGI_1DF0F1FF')
GO
INSERT [dbo].[IngredientImage] ([Id], [IngredientId], [ImageUrl], [AltText], [IsPrimary], [SortOrder], [Uuid], [Code]) VALUES (7, 16, N'https://example.com/images/ingredients/sel.jpg', N'Sel fin', 1, 1, N'eeb03346-13cb-f111-b4f1-c858b3a6df82', N'IGI_E30C5875')
GO
INSERT [dbo].[IngredientImage] ([Id], [IngredientId], [ImageUrl], [AltText], [IsPrimary], [SortOrder], [Uuid], [Code]) VALUES (8, 17, N'https://example.com/images/ingredients/poivre-noir.jpg', N'Poivre noir moulu', 1, 1, N'efb03346-13cb-f111-b4f1-c858b3a6df82', N'IGI_06558DF7')
GO
INSERT [dbo].[IngredientImage] ([Id], [IngredientId], [ImageUrl], [AltText], [IsPrimary], [SortOrder], [Uuid], [Code]) VALUES (9, 5, N'https://example.com/images/ingredients/avocat.jpg', N'Avocat mur', 1, 1, N'e3b03346-13cb-f111-b4f1-c858b3a6df82', N'IGI_5CA6B6C3')
GO
INSERT [dbo].[IngredientImage] ([Id], [IngredientId], [ImageUrl], [AltText], [IsPrimary], [SortOrder], [Uuid], [Code]) VALUES (10, 6, N'https://example.com/images/ingredients/pain.jpg', N'Tranches de pain', 1, 1, N'e4b03346-13cb-f111-b4f1-c858b3a6df82', N'IGI_BD474805')
GO
INSERT [dbo].[IngredientImage] ([Id], [IngredientId], [ImageUrl], [AltText], [IsPrimary], [SortOrder], [Uuid], [Code]) VALUES (11, 7, N'https://example.com/images/ingredients/oeuf.jpg', N'Oeuf frais', 1, 1, N'e5b03346-13cb-f111-b4f1-c858b3a6df82', N'IGI_74C2B15F')
GO
INSERT [dbo].[IngredientImage] ([Id], [IngredientId], [ImageUrl], [AltText], [IsPrimary], [SortOrder], [Uuid], [Code]) VALUES (12, 8, N'https://example.com/images/ingredients/lait.jpg', N'Verre de lait', 1, 1, N'e6b03346-13cb-f111-b4f1-c858b3a6df82', N'IGI_1906771B')
GO
INSERT [dbo].[IngredientImage] ([Id], [IngredientId], [ImageUrl], [AltText], [IsPrimary], [SortOrder], [Uuid], [Code]) VALUES (13, 9, N'https://example.com/images/ingredients/banane.jpg', N'Banane jaune', 1, 1, N'e7b03346-13cb-f111-b4f1-c858b3a6df82', N'IGI_1FCED612')
GO
INSERT [dbo].[IngredientImage] ([Id], [IngredientId], [ImageUrl], [AltText], [IsPrimary], [SortOrder], [Uuid], [Code]) VALUES (14, 10, N'https://example.com/images/ingredients/mangue.jpg', N'Mangue coupee', 1, 1, N'e8b03346-13cb-f111-b4f1-c858b3a6df82', N'IGI_431A3909')
GO
INSERT [dbo].[IngredientImage] ([Id], [IngredientId], [ImageUrl], [AltText], [IsPrimary], [SortOrder], [Uuid], [Code]) VALUES (15, 11, N'https://example.com/images/ingredients/yaourt.jpg', N'Yaourt nature', 1, 1, N'e9b03346-13cb-f111-b4f1-c858b3a6df82', N'IGI_3872F3D8')
GO
INSERT [dbo].[IngredientImage] ([Id], [IngredientId], [ImageUrl], [AltText], [IsPrimary], [SortOrder], [Uuid], [Code]) VALUES (16, 22, N'https://example.com/images/ingredients/miel.jpg', N'Miel liquide', 1, 1, N'f4b03346-13cb-f111-b4f1-c858b3a6df82', N'IGI_32691073')
GO
INSERT [dbo].[IngredientImage] ([Id], [IngredientId], [ImageUrl], [AltText], [IsPrimary], [SortOrder], [Uuid], [Code]) VALUES (17, 23, N'https://example.com/images/ingredients/menthe.jpg', N'Feuilles de menthe', 1, 1, N'f5b03346-13cb-f111-b4f1-c858b3a6df82', N'IGI_B0B8F085')
GO
INSERT [dbo].[IngredientImage] ([Id], [IngredientId], [ImageUrl], [AltText], [IsPrimary], [SortOrder], [Uuid], [Code]) VALUES (18, 12, N'https://example.com/images/ingredients/flocons-avoine.jpg', N'Flocons d avoine', 1, 1, N'eab03346-13cb-f111-b4f1-c858b3a6df82', N'IGI_8C9F8A0C')
GO
INSERT [dbo].[IngredientImage] ([Id], [IngredientId], [ImageUrl], [AltText], [IsPrimary], [SortOrder], [Uuid], [Code]) VALUES (19, 13, N'https://example.com/images/ingredients/farine.jpg', N'Farine blanche', 1, 1, N'ebb03346-13cb-f111-b4f1-c858b3a6df82', N'IGI_30872E05')
GO
INSERT [dbo].[IngredientImage] ([Id], [IngredientId], [ImageUrl], [AltText], [IsPrimary], [SortOrder], [Uuid], [Code]) VALUES (20, 18, N'https://example.com/images/ingredients/sucre.jpg', N'Sucre en poudre', 1, 1, N'f0b03346-13cb-f111-b4f1-c858b3a6df82', N'IGI_CD1E5344')
GO
INSERT [dbo].[IngredientImage] ([Id], [IngredientId], [ImageUrl], [AltText], [IsPrimary], [SortOrder], [Uuid], [Code]) VALUES (21, 14, N'https://example.com/images/ingredients/beurre.jpg', N'Beurre doux', 1, 1, N'ecb03346-13cb-f111-b4f1-c858b3a6df82', N'IGI_72D32AA8')
GO
INSERT [dbo].[IngredientImage] ([Id], [IngredientId], [ImageUrl], [AltText], [IsPrimary], [SortOrder], [Uuid], [Code]) VALUES (22, 19, N'https://example.com/images/ingredients/levure.jpg', N'Levure chimique', 1, 1, N'f1b03346-13cb-f111-b4f1-c858b3a6df82', N'IGI_38AAB33E')
GO
INSERT [dbo].[IngredientImage] ([Id], [IngredientId], [ImageUrl], [AltText], [IsPrimary], [SortOrder], [Uuid], [Code]) VALUES (23, 15, N'https://example.com/images/ingredients/fraises.jpg', N'Fraises fraiches', 1, 1, N'edb03346-13cb-f111-b4f1-c858b3a6df82', N'IGI_E3C6915B')
GO
SET IDENTITY_INSERT [dbo].[IngredientImage] OFF
GO
SET IDENTITY_INSERT [dbo].[Recipe] ON

GO
INSERT [dbo].[Recipe] ([Id], [ChefId], [DifficultyId], [CuisineId], [CityId], [Title], [Slug], [Summary], [PrepTimeMinutes], [CookTimeMinutes], [Servings], [CoverImageUrl], [Status], [Visibility], [IsActive], [DateInsert], [DateModif], [PublishedAt], [DeletedAt], [DeletedByUserId], [DeleteReason], [Uuid], [Code], [Color]) VALUES (1, 3, 4, 3, 2, N'Pancakes aux fraises', N'pancakes-aux-fraises', N'Pancakes moelleux servis avec fraises fraiches.', 15, 10, 4, N'https://example.com/images/recipes/pancakes-fraises-1.jpg', N'PUBLISHED', N'PUBLIC', 1, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, NULL, N'', N'f6b03346-13cb-f111-b4f1-c858b3a6df82', N'RCP_D4E27407', NULL)
GO
INSERT [dbo].[Recipe] ([Id], [ChefId], [DifficultyId], [CuisineId], [CityId], [Title], [Slug], [Summary], [PrepTimeMinutes], [CookTimeMinutes], [Servings], [CoverImageUrl], [Status], [Visibility], [IsActive], [DateInsert], [DateModif], [PublishedAt], [DeletedAt], [DeletedByUserId], [DeleteReason], [Uuid], [Code], [Color]) VALUES (2, 2, 3, 1, 34, N'Salade grecque fraiche', N'salade-grecque-fraiche', N'Salade fraiche avec legumes croquants, feta et sauce citronnee.', 15, 0, 2, N'https://example.com/images/recipes/salade-grecque-1.jpg', N'PUBLISHED', N'PUBLIC', 1, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, NULL, N'', N'f7b03346-13cb-f111-b4f1-c858b3a6df82', N'RCP_3C2A1560', NULL)
GO
INSERT [dbo].[Recipe] ([Id], [ChefId], [DifficultyId], [CuisineId], [CityId], [Title], [Slug], [Summary], [PrepTimeMinutes], [CookTimeMinutes], [Servings], [CoverImageUrl], [Status], [Visibility], [IsActive], [DateInsert], [DateModif], [PublishedAt], [DeletedAt], [DeletedByUserId], [DeleteReason], [Uuid], [Code], [Color]) VALUES (3, 1, 3, 1, 21, N'Tartine avocat oeuf', N'tartine-avocat-oeuf', N'Petit-dejeuner rapide avec avocat cremeux et oeufs.', 10, 8, 2, N'https://example.com/images/recipes/tartine-avocat-1.jpg', N'PUBLISHED', N'PUBLIC', 1, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, NULL, N'', N'f8b03346-13cb-f111-b4f1-c858b3a6df82', N'RCP_ABEA75AE', NULL)
GO
INSERT [dbo].[Recipe] ([Id], [ChefId], [DifficultyId], [CuisineId], [CityId], [Title], [Slug], [Summary], [PrepTimeMinutes], [CookTimeMinutes], [Servings], [CoverImageUrl], [Status], [Visibility], [IsActive], [DateInsert], [DateModif], [PublishedAt], [DeletedAt], [DeletedByUserId], [DeleteReason], [Uuid], [Code], [Color]) VALUES (4, 2, 3, 1, 53, N'Smoothie mangue banane', N'smoothie-mangue-banane', N'Smoothie froid pour le matin ou la collation.', 10, 0, 2, N'https://example.com/images/recipes/smoothie-mangue-1.jpg', N'PUBLISHED', N'PUBLIC', 1, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, CAST(N'2026-10-18T16:45:00' AS SmallDateTime), NULL, NULL, N'', N'f9b03346-13cb-f111-b4f1-c858b3a6df82', N'RCP_D99148E7', NULL)
GO
SET IDENTITY_INSERT [dbo].[Recipe] OFF
GO
SET IDENTITY_INSERT [dbo].[RecipeStep] ON

GO
INSERT [dbo].[RecipeStep] ([Id], [RecipeId], [StepNumber], [Title], [Instruction], [DurationMinutes], [Uuid], [Code], [ImageUrl]) VALUES (1, 1, 1, N'Melanger les ingredients secs', N'Dans un saladier, melanger la farine, le sucre et la levure chimique.', 3, N'02b13346-13cb-f111-b4f1-c858b3a6df82', N'RST_4DDB3505', NULL)
GO
INSERT [dbo].[RecipeStep] ([Id], [RecipeId], [StepNumber], [Title], [Instruction], [DurationMinutes], [Uuid], [Code], [ImageUrl]) VALUES (2, 1, 2, N'Ajouter les liquides', N'Ajouter le lait, les oeufs et le beurre fondu puis fouetter jusqu a obtenir une pate lisse.', 4, N'03b13346-13cb-f111-b4f1-c858b3a6df82', N'RST_C54E1EA0', NULL)
GO
INSERT [dbo].[RecipeStep] ([Id], [RecipeId], [StepNumber], [Title], [Instruction], [DurationMinutes], [Uuid], [Code], [ImageUrl]) VALUES (3, 1, 3, N'Cuire les pancakes', N'Verser une petite louche de pate dans une poele chaude et cuire chaque face jusqu a coloration doree.', 8, N'04b13346-13cb-f111-b4f1-c858b3a6df82', N'RST_1709B9A5', NULL)
GO
INSERT [dbo].[RecipeStep] ([Id], [RecipeId], [StepNumber], [Title], [Instruction], [DurationMinutes], [Uuid], [Code], [ImageUrl]) VALUES (4, 1, 4, N'Dresser', N'Servir les pancakes avec les fraises fraiches sur le dessus.', 2, N'05b13346-13cb-f111-b4f1-c858b3a6df82', N'RST_26FBED10', NULL)
GO
INSERT [dbo].[RecipeStep] ([Id], [RecipeId], [StepNumber], [Title], [Instruction], [DurationMinutes], [Uuid], [Code], [ImageUrl]) VALUES (5, 2, 1, N'Preparer les legumes', N'Laver la tomate, le concombre et la laitue. Couper la tomate en des, le concombre en rondelles et emincer la laitue.', 5, N'06b13346-13cb-f111-b4f1-c858b3a6df82', N'RST_97B8A8BA', NULL)
GO
INSERT [dbo].[RecipeStep] ([Id], [RecipeId], [StepNumber], [Title], [Instruction], [DurationMinutes], [Uuid], [Code], [ImageUrl]) VALUES (6, 2, 2, N'Preparer la sauce', N'Dans un petit bol, melanger l huile d olive, le jus de citron, le sel et le poivre noir.', 2, N'07b13346-13cb-f111-b4f1-c858b3a6df82', N'RST_E86C9B1D', NULL)
GO
INSERT [dbo].[RecipeStep] ([Id], [RecipeId], [StepNumber], [Title], [Instruction], [DurationMinutes], [Uuid], [Code], [ImageUrl]) VALUES (7, 2, 3, N'Assembler la salade', N'Disposer les legumes dans un saladier, ajouter la feta emiettee puis verser la sauce.', 3, N'08b13346-13cb-f111-b4f1-c858b3a6df82', N'RST_2434AF54', NULL)
GO
INSERT [dbo].[RecipeStep] ([Id], [RecipeId], [StepNumber], [Title], [Instruction], [DurationMinutes], [Uuid], [Code], [ImageUrl]) VALUES (8, 2, 4, N'Servir', N'Melanger delicatement et servir immediatement.', 1, N'09b13346-13cb-f111-b4f1-c858b3a6df82', N'RST_065C01B2', NULL)
GO
INSERT [dbo].[RecipeStep] ([Id], [RecipeId], [StepNumber], [Title], [Instruction], [DurationMinutes], [Uuid], [Code], [ImageUrl]) VALUES (9, 3, 1, N'Griller le pain', N'Faire griller les tranches de pain jusqu a obtenir une belle couleur doree.', 3, N'0ab13346-13cb-f111-b4f1-c858b3a6df82', N'RST_2B474743', NULL)
GO
INSERT [dbo].[RecipeStep] ([Id], [RecipeId], [StepNumber], [Title], [Instruction], [DurationMinutes], [Uuid], [Code], [ImageUrl]) VALUES (10, 3, 2, N'Ecraser l avocat', N'Ecraser l avocat avec le sel et le poivre noir jusqu a obtention d une texture cremeuse.', 2, N'0bb13346-13cb-f111-b4f1-c858b3a6df82', N'RST_78C2C30D', NULL)
GO
INSERT [dbo].[RecipeStep] ([Id], [RecipeId], [StepNumber], [Title], [Instruction], [DurationMinutes], [Uuid], [Code], [ImageUrl]) VALUES (11, 3, 3, N'Cuire les oeufs', N'Cuire les oeufs selon votre preference, a l eau ou a la poele.', 5, N'0cb13346-13cb-f111-b4f1-c858b3a6df82', N'RST_CEEF8656', NULL)
GO
INSERT [dbo].[RecipeStep] ([Id], [RecipeId], [StepNumber], [Title], [Instruction], [DurationMinutes], [Uuid], [Code], [ImageUrl]) VALUES (12, 3, 4, N'Monter les tartines', N'Etaler l avocat sur le pain, ajouter les oeufs puis un filet d huile d olive si desire.', 2, N'0db13346-13cb-f111-b4f1-c858b3a6df82', N'RST_B5CF6AD9', NULL)
GO
INSERT [dbo].[RecipeStep] ([Id], [RecipeId], [StepNumber], [Title], [Instruction], [DurationMinutes], [Uuid], [Code], [ImageUrl]) VALUES (13, 4, 1, N'Preparer les fruits', N'Eplucher la mangue et la banane puis les couper en morceaux.', 3, N'0eb13346-13cb-f111-b4f1-c858b3a6df82', N'RST_7B8846F1', NULL)
GO
INSERT [dbo].[RecipeStep] ([Id], [RecipeId], [StepNumber], [Title], [Instruction], [DurationMinutes], [Uuid], [Code], [ImageUrl]) VALUES (14, 4, 2, N'Mixer', N'Mettre la mangue, la banane, le lait, le yaourt et le miel dans un blender puis mixer.', 3, N'0fb13346-13cb-f111-b4f1-c858b3a6df82', N'RST_A534D8EA', NULL)
GO
INSERT [dbo].[RecipeStep] ([Id], [RecipeId], [StepNumber], [Title], [Instruction], [DurationMinutes], [Uuid], [Code], [ImageUrl]) VALUES (15, 4, 3, N'Ajuster la texture', N'Ajouter les flocons d avoine si vous voulez un smoothie plus epais puis mixer de nouveau.', 1, N'10b13346-13cb-f111-b4f1-c858b3a6df82', N'RST_8FE7D9F3', NULL)
GO
INSERT [dbo].[RecipeStep] ([Id], [RecipeId], [StepNumber], [Title], [Instruction], [DurationMinutes], [Uuid], [Code], [ImageUrl]) VALUES (16, 4, 4, N'Servir frais', N'Verser dans des verres et decorer avec la menthe avant de servir.', 1, N'11b13346-13cb-f111-b4f1-c858b3a6df82', N'RST_4DFCD34D', NULL)
GO
SET IDENTITY_INSERT [dbo].[RecipeStep] OFF
GO
SET IDENTITY_INSERT [dbo].[RecipeShare] ON

GO
INSERT [dbo].[RecipeShare] ([Id], [RecipeId], [SharedByUserId], [SharedWithUserId], [ShareChannel], [ShareMessage], [SharedAt], [Uuid], [Code], [ImageUrl]) VALUES (1, 2, 3, 2, N'Application', N'Teste cette salade fraiche pour le dejeuner.', CAST(N'2026-10-18T16:45:00' AS SmallDateTime), N'17b13346-13cb-f111-b4f1-c858b3a6df82', N'RSH_EFC49DDF', NULL)
GO
INSERT [dbo].[RecipeShare] ([Id], [RecipeId], [SharedByUserId], [SharedWithUserId], [ShareChannel], [ShareMessage], [SharedAt], [Uuid], [Code], [ImageUrl]) VALUES (2, 4, 2, NULL, N'Lien', N'Je partage mon smoothie prefere.', CAST(N'2026-10-18T16:45:00' AS SmallDateTime), N'16b13346-13cb-f111-b4f1-c858b3a6df82', N'RSH_98C73FC1', NULL)
GO
INSERT [dbo].[RecipeShare] ([Id], [RecipeId], [SharedByUserId], [SharedWithUserId], [ShareChannel], [ShareMessage], [SharedAt], [Uuid], [Code], [ImageUrl]) VALUES (3, 1, 4, 5, N'Application', N'Recette ideale pour un brunch du week-end.', CAST(N'2026-10-18T16:45:00' AS SmallDateTime), N'18b13346-13cb-f111-b4f1-c858b3a6df82', N'RSH_53F3B4AD', NULL)
GO
SET IDENTITY_INSERT [dbo].[RecipeShare] OFF
GO
INSERT [dbo].[RecipeNutrition] ([RecipeId], [CaloriesPerServing], [ProteinGrams], [CarbsGrams], [FatGrams], [Uuid], [Color], [ImageUrl]) VALUES (1, CAST(340.00 AS Decimal(10, 2)), CAST(9.00 AS Decimal(10, 2)), CAST(46.00 AS Decimal(10, 2)), CAST(12.00 AS Decimal(10, 2)), N'12b13346-13cb-f111-b4f1-c858b3a6df82', NULL, N'https://images.unsplash.com/photo-1490645935967-10de6ba17061?w=400&fit=crop')
GO
INSERT [dbo].[RecipeNutrition] ([RecipeId], [CaloriesPerServing], [ProteinGrams], [CarbsGrams], [FatGrams], [Uuid], [Color], [ImageUrl]) VALUES (2, CAST(220.00 AS Decimal(10, 2)), CAST(7.00 AS Decimal(10, 2)), CAST(12.00 AS Decimal(10, 2)), CAST(16.00 AS Decimal(10, 2)), N'13b13346-13cb-f111-b4f1-c858b3a6df82', NULL, N'https://images.unsplash.com/photo-1490645935967-10de6ba17061?w=400&fit=crop')
GO
INSERT [dbo].[RecipeNutrition] ([RecipeId], [CaloriesPerServing], [ProteinGrams], [CarbsGrams], [FatGrams], [Uuid], [Color], [ImageUrl]) VALUES (3, CAST(310.00 AS Decimal(10, 2)), CAST(13.00 AS Decimal(10, 2)), CAST(24.00 AS Decimal(10, 2)), CAST(18.00 AS Decimal(10, 2)), N'14b13346-13cb-f111-b4f1-c858b3a6df82', NULL, N'https://images.unsplash.com/photo-1490645935967-10de6ba17061?w=400&fit=crop')
GO
INSERT [dbo].[RecipeNutrition] ([RecipeId], [CaloriesPerServing], [ProteinGrams], [CarbsGrams], [FatGrams], [Uuid], [Color], [ImageUrl]) VALUES (4, CAST(205.00 AS Decimal(10, 2)), CAST(6.00 AS Decimal(10, 2)), CAST(34.00 AS Decimal(10, 2)), CAST(5.00 AS Decimal(10, 2)), N'15b13346-13cb-f111-b4f1-c858b3a6df82', NULL, N'https://images.unsplash.com/photo-1490645935967-10de6ba17061?w=400&fit=crop')
GO
INSERT [dbo].[RecipeLike] ([RecipeId], [UserId], [LikedAt]) VALUES (1, 2, CAST(N'2026-10-18T16:45:00' AS SmallDateTime))
GO
INSERT [dbo].[RecipeLike] ([RecipeId], [UserId], [LikedAt]) VALUES (1, 5, CAST(N'2026-10-18T16:45:00' AS SmallDateTime))
GO
INSERT [dbo].[RecipeLike] ([RecipeId], [UserId], [LikedAt]) VALUES (2, 2, CAST(N'2026-10-18T16:45:00' AS SmallDateTime))
GO
INSERT [dbo].[RecipeLike] ([RecipeId], [UserId], [LikedAt]) VALUES (3, 3, CAST(N'2026-10-18T16:45:00' AS SmallDateTime))
GO
INSERT [dbo].[RecipeLike] ([RecipeId], [UserId], [LikedAt]) VALUES (4, 2, CAST(N'2026-10-18T16:45:00' AS SmallDateTime))
GO
INSERT [dbo].[RecipeIngredient] ([RecipeId], [IngredientId], [Quantity], [UnitId], [IsOptional], [Notes], [SortOrder], [Uuid], [Code], [ImageUrl]) VALUES (1, 7, CAST(2.00 AS Decimal(10, 2)), 8, 0, NULL, 3, N'1fb13346-13cb-f111-b4f1-c858b3a6df82', N'RIG_275FCCE6', NULL)
GO
INSERT [dbo].[RecipeIngredient] ([RecipeId], [IngredientId], [Quantity], [UnitId], [IsOptional], [Notes], [SortOrder], [Uuid], [Code], [ImageUrl]) VALUES (1, 8, CAST(250.00 AS Decimal(10, 2)), 3, 0, NULL, 2, N'21b13346-13cb-f111-b4f1-c858b3a6df82', N'RIG_B895262C', NULL)
GO
INSERT [dbo].[RecipeIngredient] ([RecipeId], [IngredientId], [Quantity], [UnitId], [IsOptional], [Notes], [SortOrder], [Uuid], [Code], [ImageUrl]) VALUES (1, 13, CAST(200.00 AS Decimal(10, 2)), 1, 0, NULL, 1, N'27b13346-13cb-f111-b4f1-c858b3a6df82', N'RIG_D2742C35', NULL)
GO
INSERT [dbo].[RecipeIngredient] ([RecipeId], [IngredientId], [Quantity], [UnitId], [IsOptional], [Notes], [SortOrder], [Uuid], [Code], [ImageUrl]) VALUES (1, 14, CAST(25.00 AS Decimal(10, 2)), 1, 0, N'Fondu', 5, N'28b13346-13cb-f111-b4f1-c858b3a6df82', N'RIG_2444D2B9', NULL)
GO
INSERT [dbo].[RecipeIngredient] ([RecipeId], [IngredientId], [Quantity], [UnitId], [IsOptional], [Notes], [SortOrder], [Uuid], [Code], [ImageUrl]) VALUES (1, 15, CAST(120.00 AS Decimal(10, 2)), 1, 1, N'Pour le service', 7, N'29b13346-13cb-f111-b4f1-c858b3a6df82', N'RIG_7747C55E', NULL)
GO
INSERT [dbo].[RecipeIngredient] ([RecipeId], [IngredientId], [Quantity], [UnitId], [IsOptional], [Notes], [SortOrder], [Uuid], [Code], [ImageUrl]) VALUES (1, 18, CAST(30.00 AS Decimal(10, 2)), 1, 0, NULL, 4, N'2eb13346-13cb-f111-b4f1-c858b3a6df82', N'RIG_E5176DB2', NULL)
GO
INSERT [dbo].[RecipeIngredient] ([RecipeId], [IngredientId], [Quantity], [UnitId], [IsOptional], [Notes], [SortOrder], [Uuid], [Code], [ImageUrl]) VALUES (1, 19, CAST(10.00 AS Decimal(10, 2)), 1, 0, NULL, 6, N'2fb13346-13cb-f111-b4f1-c858b3a6df82', N'RIG_19C6D5D8', NULL)
GO
INSERT [dbo].[RecipeIngredient] ([RecipeId], [IngredientId], [Quantity], [UnitId], [IsOptional], [Notes], [SortOrder], [Uuid], [Code], [ImageUrl]) VALUES (2, 1, CAST(200.00 AS Decimal(10, 2)), 1, 0, N'Coupee en des', 1, N'19b13346-13cb-f111-b4f1-c858b3a6df82', N'RIG_CFFDD2BF', NULL)
GO
INSERT [dbo].[RecipeIngredient] ([RecipeId], [IngredientId], [Quantity], [UnitId], [IsOptional], [Notes], [SortOrder], [Uuid], [Code], [ImageUrl]) VALUES (2, 2, CAST(150.00 AS Decimal(10, 2)), 1, 0, N'Coupe en rondelles', 2, N'1ab13346-13cb-f111-b4f1-c858b3a6df82', N'RIG_851B91B5', NULL)
GO
INSERT [dbo].[RecipeIngredient] ([RecipeId], [IngredientId], [Quantity], [UnitId], [IsOptional], [Notes], [SortOrder], [Uuid], [Code], [ImageUrl]) VALUES (2, 3, CAST(100.00 AS Decimal(10, 2)), 1, 0, N'Emincee', 3, N'1bb13346-13cb-f111-b4f1-c858b3a6df82', N'RIG_3BC7CBDB', NULL)
GO
INSERT [dbo].[RecipeIngredient] ([RecipeId], [IngredientId], [Quantity], [UnitId], [IsOptional], [Notes], [SortOrder], [Uuid], [Code], [ImageUrl]) VALUES (2, 4, CAST(80.00 AS Decimal(10, 2)), 1, 0, N'Emiettee', 4, N'1cb13346-13cb-f111-b4f1-c858b3a6df82', N'RIG_E32C275C', NULL)
GO
INSERT [dbo].[RecipeIngredient] ([RecipeId], [IngredientId], [Quantity], [UnitId], [IsOptional], [Notes], [SortOrder], [Uuid], [Code], [ImageUrl]) VALUES (2, 16, CAST(1.00 AS Decimal(10, 2)), 5, 0, NULL, 7, N'2ab13346-13cb-f111-b4f1-c858b3a6df82', N'RIG_12C2DC08', NULL)
GO
INSERT [dbo].[RecipeIngredient] ([RecipeId], [IngredientId], [Quantity], [UnitId], [IsOptional], [Notes], [SortOrder], [Uuid], [Code], [ImageUrl]) VALUES (2, 17, CAST(0.50 AS Decimal(10, 2)), 5, 0, NULL, 8, N'2cb13346-13cb-f111-b4f1-c858b3a6df82', N'RIG_E368CEA6', NULL)
GO
INSERT [dbo].[RecipeIngredient] ([RecipeId], [IngredientId], [Quantity], [UnitId], [IsOptional], [Notes], [SortOrder], [Uuid], [Code], [ImageUrl]) VALUES (2, 20, CAST(30.00 AS Decimal(10, 2)), 3, 0, N'Pour la sauce', 5, N'30b13346-13cb-f111-b4f1-c858b3a6df82', N'RIG_14DBC7F8', NULL)
GO
INSERT [dbo].[RecipeIngredient] ([RecipeId], [IngredientId], [Quantity], [UnitId], [IsOptional], [Notes], [SortOrder], [Uuid], [Code], [ImageUrl]) VALUES (2, 21, CAST(15.00 AS Decimal(10, 2)), 3, 0, N'Presse minute', 6, N'32b13346-13cb-f111-b4f1-c858b3a6df82', N'RIG_A56DAFC8', NULL)
GO
INSERT [dbo].[RecipeIngredient] ([RecipeId], [IngredientId], [Quantity], [UnitId], [IsOptional], [Notes], [SortOrder], [Uuid], [Code], [ImageUrl]) VALUES (3, 5, CAST(1.00 AS Decimal(10, 2)), 8, 0, N'Bien mur', 1, N'1db13346-13cb-f111-b4f1-c858b3a6df82', N'RIG_73A4CD2E', NULL)
GO
INSERT [dbo].[RecipeIngredient] ([RecipeId], [IngredientId], [Quantity], [UnitId], [IsOptional], [Notes], [SortOrder], [Uuid], [Code], [ImageUrl]) VALUES (3, 6, CAST(2.00 AS Decimal(10, 2)), 8, 0, N'Tranches de pain', 2, N'1eb13346-13cb-f111-b4f1-c858b3a6df82', N'RIG_FA4B57C4', NULL)
GO
INSERT [dbo].[RecipeIngredient] ([RecipeId], [IngredientId], [Quantity], [UnitId], [IsOptional], [Notes], [SortOrder], [Uuid], [Code], [ImageUrl]) VALUES (3, 7, CAST(2.00 AS Decimal(10, 2)), 8, 0, N'Bouilli ou poele', 3, N'20b13346-13cb-f111-b4f1-c858b3a6df82', N'RIG_5D717612', NULL)
GO
INSERT [dbo].[RecipeIngredient] ([RecipeId], [IngredientId], [Quantity], [UnitId], [IsOptional], [Notes], [SortOrder], [Uuid], [Code], [ImageUrl]) VALUES (3, 16, CAST(0.50 AS Decimal(10, 2)), 5, 0, NULL, 5, N'2bb13346-13cb-f111-b4f1-c858b3a6df82', N'RIG_739E4654', NULL)
GO
INSERT [dbo].[RecipeIngredient] ([RecipeId], [IngredientId], [Quantity], [UnitId], [IsOptional], [Notes], [SortOrder], [Uuid], [Code], [ImageUrl]) VALUES (3, 17, CAST(0.25 AS Decimal(10, 2)), 5, 0, NULL, 6, N'2db13346-13cb-f111-b4f1-c858b3a6df82', N'RIG_40DF783B', NULL)
GO
INSERT [dbo].[RecipeIngredient] ([RecipeId], [IngredientId], [Quantity], [UnitId], [IsOptional], [Notes], [SortOrder], [Uuid], [Code], [ImageUrl]) VALUES (3, 20, CAST(10.00 AS Decimal(10, 2)), 3, 1, N'Filet facultatif', 4, N'31b13346-13cb-f111-b4f1-c858b3a6df82', N'RIG_DEC205F2', NULL)
GO
INSERT [dbo].[RecipeIngredient] ([RecipeId], [IngredientId], [Quantity], [UnitId], [IsOptional], [Notes], [SortOrder], [Uuid], [Code], [ImageUrl]) VALUES (4, 8, CAST(250.00 AS Decimal(10, 2)), 3, 0, NULL, 3, N'22b13346-13cb-f111-b4f1-c858b3a6df82', N'RIG_6527BFDF', NULL)
GO
INSERT [dbo].[RecipeIngredient] ([RecipeId], [IngredientId], [Quantity], [UnitId], [IsOptional], [Notes], [SortOrder], [Uuid], [Code], [ImageUrl]) VALUES (4, 9, CAST(1.00 AS Decimal(10, 2)), 8, 0, N'Coupee en rondelles', 2, N'23b13346-13cb-f111-b4f1-c858b3a6df82', N'RIG_12210CE0', NULL)
GO
INSERT [dbo].[RecipeIngredient] ([RecipeId], [IngredientId], [Quantity], [UnitId], [IsOptional], [Notes], [SortOrder], [Uuid], [Code], [ImageUrl]) VALUES (4, 10, CAST(1.00 AS Decimal(10, 2)), 8, 0, N'Epluchee et coupee', 1, N'24b13346-13cb-f111-b4f1-c858b3a6df82', N'RIG_6282B623', NULL)
GO
INSERT [dbo].[RecipeIngredient] ([RecipeId], [IngredientId], [Quantity], [UnitId], [IsOptional], [Notes], [SortOrder], [Uuid], [Code], [ImageUrl]) VALUES (4, 11, CAST(120.00 AS Decimal(10, 2)), 1, 0, N'Nature', 4, N'25b13346-13cb-f111-b4f1-c858b3a6df82', N'RIG_849E838E', NULL)
GO
INSERT [dbo].[RecipeIngredient] ([RecipeId], [IngredientId], [Quantity], [UnitId], [IsOptional], [Notes], [SortOrder], [Uuid], [Code], [ImageUrl]) VALUES (4, 12, CAST(20.00 AS Decimal(10, 2)), 1, 1, N'Pour une texture plus epaisse', 7, N'26b13346-13cb-f111-b4f1-c858b3a6df82', N'RIG_F928B5CE', NULL)
GO
INSERT [dbo].[RecipeIngredient] ([RecipeId], [IngredientId], [Quantity], [UnitId], [IsOptional], [Notes], [SortOrder], [Uuid], [Code], [ImageUrl]) VALUES (4, 22, CAST(1.00 AS Decimal(10, 2)), 6, 1, N'Pour sucrer davantage', 5, N'33b13346-13cb-f111-b4f1-c858b3a6df82', N'RIG_6607DEF2', NULL)
GO
INSERT [dbo].[RecipeIngredient] ([RecipeId], [IngredientId], [Quantity], [UnitId], [IsOptional], [Notes], [SortOrder], [Uuid], [Code], [ImageUrl]) VALUES (4, 23, CAST(5.00 AS Decimal(10, 2)), 1, 1, N'Pour decorer', 6, N'34b13346-13cb-f111-b4f1-c858b3a6df82', N'RIG_320BB81D', NULL)
GO
SET IDENTITY_INSERT [dbo].[RecipeImage] ON

GO
INSERT [dbo].[RecipeImage] ([Id], [RecipeId], [ImageUrl], [AltText], [IsPrimary], [SortOrder], [Uuid], [Code]) VALUES (1, 2, N'https://example.com/images/recipes/salade-grecque-1.jpg', N'Vue principale de la salade grecque fraiche', 1, 1, N'fcb03346-13cb-f111-b4f1-c858b3a6df82', N'RIM_757FCD03')
GO
INSERT [dbo].[RecipeImage] ([Id], [RecipeId], [ImageUrl], [AltText], [IsPrimary], [SortOrder], [Uuid], [Code]) VALUES (2, 2, N'https://example.com/images/recipes/salade-grecque-2.jpg', N'Detail de la feta et des legumes', 0, 2, N'fdb03346-13cb-f111-b4f1-c858b3a6df82', N'RIM_CFFEA2A1')
GO
INSERT [dbo].[RecipeImage] ([Id], [RecipeId], [ImageUrl], [AltText], [IsPrimary], [SortOrder], [Uuid], [Code]) VALUES (3, 3, N'https://example.com/images/recipes/tartine-avocat-1.jpg', N'Vue principale de la tartine avocat oeuf', 1, 1, N'feb03346-13cb-f111-b4f1-c858b3a6df82', N'RIM_2CB717FE')
GO
INSERT [dbo].[RecipeImage] ([Id], [RecipeId], [ImageUrl], [AltText], [IsPrimary], [SortOrder], [Uuid], [Code]) VALUES (4, 3, N'https://example.com/images/recipes/tartine-avocat-2.jpg', N'Detail des tartines et oeufs', 0, 2, N'ffb03346-13cb-f111-b4f1-c858b3a6df82', N'RIM_23AAE7C1')
GO
INSERT [dbo].[RecipeImage] ([Id], [RecipeId], [ImageUrl], [AltText], [IsPrimary], [SortOrder], [Uuid], [Code]) VALUES (5, 4, N'https://example.com/images/recipes/smoothie-mangue-1.jpg', N'Verre principal du smoothie mangue banane', 1, 1, N'00b13346-13cb-f111-b4f1-c858b3a6df82', N'RIM_18910673')
GO
INSERT [dbo].[RecipeImage] ([Id], [RecipeId], [ImageUrl], [AltText], [IsPrimary], [SortOrder], [Uuid], [Code]) VALUES (6, 4, N'https://example.com/images/recipes/smoothie-mangue-2.jpg', N'Smoothie avec menthe et fruits', 0, 2, N'01b13346-13cb-f111-b4f1-c858b3a6df82', N'RIM_016416DC')
GO
INSERT [dbo].[RecipeImage] ([Id], [RecipeId], [ImageUrl], [AltText], [IsPrimary], [SortOrder], [Uuid], [Code]) VALUES (7, 1, N'https://example.com/images/recipes/pancakes-fraises-1.jpg', N'Pile de pancakes aux fraises', 1, 1, N'fab03346-13cb-f111-b4f1-c858b3a6df82', N'RIM_9F634549')
GO
INSERT [dbo].[RecipeImage] ([Id], [RecipeId], [ImageUrl], [AltText], [IsPrimary], [SortOrder], [Uuid], [Code]) VALUES (8, 1, N'https://example.com/images/recipes/pancakes-fraises-2.jpg', N'Pancakes servis avec fraises fraiches', 0, 2, N'fbb03346-13cb-f111-b4f1-c858b3a6df82', N'RIM_7F635988')
GO
SET IDENTITY_INSERT [dbo].[RecipeImage] OFF
GO
INSERT [dbo].[RecipeCategory] ([RecipeId], [FoodCategoryId]) VALUES (1, 1)
GO
INSERT [dbo].[RecipeCategory] ([RecipeId], [FoodCategoryId]) VALUES (1, 7)
GO
INSERT [dbo].[RecipeCategory] ([RecipeId], [FoodCategoryId]) VALUES (2, 2)
GO
INSERT [dbo].[RecipeCategory] ([RecipeId], [FoodCategoryId]) VALUES (2, 4)
GO
INSERT [dbo].[RecipeCategory] ([RecipeId], [FoodCategoryId]) VALUES (3, 1)
GO
INSERT [dbo].[RecipeCategory] ([RecipeId], [FoodCategoryId]) VALUES (3, 9)
GO
INSERT [dbo].[RecipeCategory] ([RecipeId], [FoodCategoryId]) VALUES (4, 1)
GO
INSERT [dbo].[RecipeCategory] ([RecipeId], [FoodCategoryId]) VALUES (4, 8)
GO
SET ANSI_PADDING ON

GO
ALTER TABLE [dbo].[IngredientType] ADD  CONSTRAINT [UQ_IngredientType_Code] UNIQUE NONCLUSTERED
(
	[Code] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
ALTER TABLE [dbo].[IngredientType] ADD  CONSTRAINT [UQ_IngredientType_Uuid] UNIQUE NONCLUSTERED
(
	[Uuid] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
SET ANSI_PADDING ON

GO
ALTER TABLE [dbo].[FoodCategory] ADD  CONSTRAINT [UQ_FoodCategory_Code] UNIQUE NONCLUSTERED
(
	[Code] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
SET ANSI_PADDING ON

GO
ALTER TABLE [dbo].[FoodCategory] ADD  CONSTRAINT [UQ_FoodCategory_Name] UNIQUE NONCLUSTERED
(
	[Name] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
ALTER TABLE [dbo].[FoodCategory] ADD  CONSTRAINT [UQ_FoodCategory_Uuid] UNIQUE NONCLUSTERED
(
	[Uuid] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
SET ANSI_PADDING ON

GO
ALTER TABLE [dbo].[Difficulty] ADD  CONSTRAINT [UQ_Difficulty_Code] UNIQUE NONCLUSTERED
(
	[Code] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
SET ANSI_PADDING ON

GO
ALTER TABLE [dbo].[Difficulty] ADD  CONSTRAINT [UQ_Difficulty_Name] UNIQUE NONCLUSTERED
(
	[Name] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
ALTER TABLE [dbo].[Difficulty] ADD  CONSTRAINT [UQ_Difficulty_Uuid] UNIQUE NONCLUSTERED
(
	[Uuid] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
SET ANSI_PADDING ON

GO
ALTER TABLE [dbo].[DailyRecipeStats] ADD  CONSTRAINT [UQ_DailyRecipeStats_Code] UNIQUE NONCLUSTERED
(
	[Code] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
ALTER TABLE [dbo].[DailyRecipeStats] ADD  CONSTRAINT [UQ_DailyRecipeStats_StatsDate] UNIQUE NONCLUSTERED
(
	[StatsDate] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
ALTER TABLE [dbo].[DailyRecipeStats] ADD  CONSTRAINT [UQ_DailyRecipeStats_Uuid] UNIQUE NONCLUSTERED
(
	[Uuid] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
CREATE NONCLUSTERED INDEX [IX_DailyRecipeStats_StatsDate] ON [dbo].[DailyRecipeStats]
(
	[StatsDate] DESC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
SET ANSI_PADDING ON

GO
ALTER TABLE [dbo].[Cuisine] ADD  CONSTRAINT [UQ_Cuisine_Code] UNIQUE NONCLUSTERED
(
	[Code] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
SET ANSI_PADDING ON

GO
ALTER TABLE [dbo].[Cuisine] ADD  CONSTRAINT [UQ_Cuisine_Name] UNIQUE NONCLUSTERED
(
	[Name] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
ALTER TABLE [dbo].[Cuisine] ADD  CONSTRAINT [UQ_Cuisine_Uuid] UNIQUE NONCLUSTERED
(
	[Uuid] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
SET ANSI_PADDING ON

GO
ALTER TABLE [dbo].[PresetAvatarImage] ADD  CONSTRAINT [UQ_PresetAvatarImage_Code] UNIQUE NONCLUSTERED
(
	[Code] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
ALTER TABLE [dbo].[PresetAvatarImage] ADD  CONSTRAINT [UQ_PresetAvatarImage_Uuid] UNIQUE NONCLUSTERED
(
	[Uuid] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
SET ANSI_PADDING ON

GO
ALTER TABLE [dbo].[MoroccanRegion] ADD  CONSTRAINT [UQ_MoroccanRegion_Code] UNIQUE NONCLUSTERED
(
	[Code] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
SET ANSI_PADDING ON

GO
ALTER TABLE [dbo].[MoroccanRegion] ADD  CONSTRAINT [UQ_MoroccanRegion_Name] UNIQUE NONCLUSTERED
(
	[Name] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
ALTER TABLE [dbo].[MoroccanRegion] ADD  CONSTRAINT [UQ_MoroccanRegion_Uuid] UNIQUE NONCLUSTERED
(
	[Uuid] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
SET ANSI_PADDING ON

GO
ALTER TABLE [dbo].[Unit] ADD  CONSTRAINT [UQ_Unit_Code] UNIQUE NONCLUSTERED
(
	[Code] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
ALTER TABLE [dbo].[Unit] ADD  CONSTRAINT [UQ_Unit_Uuid] UNIQUE NONCLUSTERED
(
	[Uuid] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
SET ANSI_PADDING ON

GO
ALTER TABLE [dbo].[Ingredient] ADD  CONSTRAINT [UQ_Ingredient_Code] UNIQUE NONCLUSTERED
(
	[Code] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
SET ANSI_PADDING ON

GO
ALTER TABLE [dbo].[Ingredient] ADD  CONSTRAINT [UQ_Ingredient_Name] UNIQUE NONCLUSTERED
(
	[Name] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
ALTER TABLE [dbo].[Ingredient] ADD  CONSTRAINT [UQ_Ingredient_Uuid] UNIQUE NONCLUSTERED
(
	[Uuid] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
SET ANSI_PADDING ON

GO
ALTER TABLE [dbo].[MoroccanCity] ADD  CONSTRAINT [UQ_MoroccanCity_Code] UNIQUE NONCLUSTERED
(
	[Code] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
SET ANSI_PADDING ON

GO
ALTER TABLE [dbo].[MoroccanCity] ADD  CONSTRAINT [UQ_MoroccanCity_Name] UNIQUE NONCLUSTERED
(
	[Name] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
SET ANSI_PADDING ON

GO
ALTER TABLE [dbo].[MoroccanCity] ADD  CONSTRAINT [UQ_MoroccanCity_Slug] UNIQUE NONCLUSTERED
(
	[Slug] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
ALTER TABLE [dbo].[MoroccanCity] ADD  CONSTRAINT [UQ_MoroccanCity_Uuid] UNIQUE NONCLUSTERED
(
	[Uuid] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
CREATE NONCLUSTERED INDEX [IX_MoroccanCity_RegionId] ON [dbo].[MoroccanCity]
(
	[RegionId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
SET ANSI_PADDING ON

GO
ALTER TABLE [dbo].[AppUser] ADD  CONSTRAINT [UQ_AppUser_Code] UNIQUE NONCLUSTERED
(
	[Code] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
SET ANSI_PADDING ON

GO
ALTER TABLE [dbo].[AppUser] ADD  CONSTRAINT [UQ_AppUser_Email] UNIQUE NONCLUSTERED
(
	[Email] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
SET ANSI_PADDING ON

GO
ALTER TABLE [dbo].[AppUser] ADD  CONSTRAINT [UQ_AppUser_UserName] UNIQUE NONCLUSTERED
(
	[UserName] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
ALTER TABLE [dbo].[AppUser] ADD  CONSTRAINT [UQ_AppUser_Uuid] UNIQUE NONCLUSTERED
(
	[Uuid] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
CREATE NONCLUSTERED INDEX [IX_AppUser_DeletedAt] ON [dbo].[AppUser]
(
	[DeletedAt] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
CREATE NONCLUSTERED INDEX [IX_AppUser_PresetAvatarImageId] ON [dbo].[AppUser]
(
	[PresetAvatarImageId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
SET ANSI_PADDING ON

GO
CREATE NONCLUSTERED INDEX [IX_AppUser_Role] ON [dbo].[AppUser]
(
	[Role] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
SET ANSI_PADDING ON

GO
ALTER TABLE [dbo].[ChefProfile] ADD  CONSTRAINT [UQ_ChefProfile_Code] UNIQUE NONCLUSTERED
(
	[Code] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
ALTER TABLE [dbo].[ChefProfile] ADD  CONSTRAINT [UQ_ChefProfile_UserId] UNIQUE NONCLUSTERED
(
	[UserId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
ALTER TABLE [dbo].[ChefProfile] ADD  CONSTRAINT [UQ_ChefProfile_Uuid] UNIQUE NONCLUSTERED
(
	[Uuid] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
CREATE NONCLUSTERED INDEX [IX_ChefProfile_DeletedAt] ON [dbo].[ChefProfile]
(
	[DeletedAt] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
SET ANSI_PADDING ON

GO
ALTER TABLE [dbo].[AppUserRefreshToken] ADD  CONSTRAINT [UQ_AppUserRefreshToken_Code] UNIQUE NONCLUSTERED
(
	[Code] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
ALTER TABLE [dbo].[AppUserRefreshToken] ADD  CONSTRAINT [UQ_AppUserRefreshToken_Uuid] UNIQUE NONCLUSTERED
(
	[Uuid] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
CREATE NONCLUSTERED INDEX [IX_AppUserRefreshToken_AppUserId] ON [dbo].[AppUserRefreshToken]
(
	[AppUserId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
SET ANSI_PADDING ON

GO
CREATE NONCLUSTERED INDEX [IX_AppUserRefreshToken_TokenHash] ON [dbo].[AppUserRefreshToken]
(
	[TokenHash] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
SET ANSI_PADDING ON

GO
ALTER TABLE [dbo].[AppUserExternalLogin] ADD  CONSTRAINT [UQ_AppUserExternalLogin_Code] UNIQUE NONCLUSTERED
(
	[Code] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
ALTER TABLE [dbo].[AppUserExternalLogin] ADD  CONSTRAINT [UQ_AppUserExternalLogin_Uuid] UNIQUE NONCLUSTERED
(
	[Uuid] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
CREATE NONCLUSTERED INDEX [IX_AppUserExternalLogin_AppUserId] ON [dbo].[AppUserExternalLogin]
(
	[AppUserId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
SET ANSI_PADDING ON

GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_AppUserExternalLogin_AppUser_Provider] ON [dbo].[AppUserExternalLogin]
(
	[AppUserId] ASC,
	[Provider] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
SET ANSI_PADDING ON

GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_AppUserExternalLogin_Provider_Subject] ON [dbo].[AppUserExternalLogin]
(
	[Provider] ASC,
	[ProviderSubject] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
SET ANSI_PADDING ON

GO
ALTER TABLE [dbo].[IngredientImage] ADD  CONSTRAINT [UQ_IngredientImage_Code] UNIQUE NONCLUSTERED
(
	[Code] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
ALTER TABLE [dbo].[IngredientImage] ADD  CONSTRAINT [UQ_IngredientImage_Uuid] UNIQUE NONCLUSTERED
(
	[Uuid] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
CREATE NONCLUSTERED INDEX [IX_IngredientImage_IngredientId] ON [dbo].[IngredientImage]
(
	[IngredientId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
SET ANSI_PADDING ON

GO
ALTER TABLE [dbo].[Recipe] ADD  CONSTRAINT [UQ_Recipe_Code] UNIQUE NONCLUSTERED
(
	[Code] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
SET ANSI_PADDING ON

GO
ALTER TABLE [dbo].[Recipe] ADD  CONSTRAINT [UQ_Recipe_Slug] UNIQUE NONCLUSTERED
(
	[Slug] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
ALTER TABLE [dbo].[Recipe] ADD  CONSTRAINT [UQ_Recipe_Uuid] UNIQUE NONCLUSTERED
(
	[Uuid] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
CREATE NONCLUSTERED INDEX [IX_Recipe_ChefId] ON [dbo].[Recipe]
(
	[ChefId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
CREATE NONCLUSTERED INDEX [IX_Recipe_CityId] ON [dbo].[Recipe]
(
	[CityId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
CREATE NONCLUSTERED INDEX [IX_Recipe_CuisineId] ON [dbo].[Recipe]
(
	[CuisineId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
CREATE NONCLUSTERED INDEX [IX_Recipe_DeletedAt] ON [dbo].[Recipe]
(
	[DeletedAt] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
CREATE NONCLUSTERED INDEX [IX_Recipe_DifficultyId] ON [dbo].[Recipe]
(
	[DifficultyId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
SET ANSI_PADDING ON

GO
ALTER TABLE [dbo].[RecipeStep] ADD  CONSTRAINT [UQ_RecipeStep_Code] UNIQUE NONCLUSTERED
(
	[Code] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
ALTER TABLE [dbo].[RecipeStep] ADD  CONSTRAINT [UQ_RecipeStep_Recipe_StepNumber] UNIQUE NONCLUSTERED
(
	[RecipeId] ASC,
	[StepNumber] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
ALTER TABLE [dbo].[RecipeStep] ADD  CONSTRAINT [UQ_RecipeStep_Uuid] UNIQUE NONCLUSTERED
(
	[Uuid] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
SET ANSI_PADDING ON

GO
ALTER TABLE [dbo].[RecipeShare] ADD  CONSTRAINT [UQ_RecipeShare_Code] UNIQUE NONCLUSTERED
(
	[Code] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
ALTER TABLE [dbo].[RecipeShare] ADD  CONSTRAINT [UQ_RecipeShare_Uuid] UNIQUE NONCLUSTERED
(
	[Uuid] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
CREATE NONCLUSTERED INDEX [IX_RecipeShare_SharedByUserId] ON [dbo].[RecipeShare]
(
	[SharedByUserId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
ALTER TABLE [dbo].[RecipeNutrition] ADD  CONSTRAINT [UQ_RecipeNutrition_Uuid] UNIQUE NONCLUSTERED
(
	[Uuid] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
CREATE NONCLUSTERED INDEX [IX_RecipeLike_UserId] ON [dbo].[RecipeLike]
(
	[UserId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
SET ANSI_PADDING ON

GO
ALTER TABLE [dbo].[RecipeIngredient] ADD  CONSTRAINT [UQ_RecipeIngredient_Code] UNIQUE NONCLUSTERED
(
	[Code] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
ALTER TABLE [dbo].[RecipeIngredient] ADD  CONSTRAINT [UQ_RecipeIngredient_Uuid] UNIQUE NONCLUSTERED
(
	[Uuid] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
CREATE NONCLUSTERED INDEX [IX_RecipeIngredient_IngredientId] ON [dbo].[RecipeIngredient]
(
	[IngredientId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
SET ANSI_PADDING ON

GO
ALTER TABLE [dbo].[RecipeImage] ADD  CONSTRAINT [UQ_RecipeImage_Code] UNIQUE NONCLUSTERED
(
	[Code] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
ALTER TABLE [dbo].[RecipeImage] ADD  CONSTRAINT [UQ_RecipeImage_Uuid] UNIQUE NONCLUSTERED
(
	[Uuid] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
CREATE NONCLUSTERED INDEX [IX_RecipeImage_RecipeId] ON [dbo].[RecipeImage]
(
	[RecipeId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
ALTER TABLE [dbo].[IngredientType] ADD  CONSTRAINT [DF_IngredientType_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[IngredientType] ADD  CONSTRAINT [DF_IngredientType_SortOrder]  DEFAULT ((0)) FOR [SortOrder]
GO
ALTER TABLE [dbo].[IngredientType] ADD  CONSTRAINT [DF_IngredientType_IconUrl]  DEFAULT ('https://images.unsplash.com/photo-1506976785307-8732e854ad03?w=400&fit=crop') FOR [IconUrl]
GO
ALTER TABLE [dbo].[IngredientType] ADD  CONSTRAINT [DF_IngredientType_DateInsert]  DEFAULT (getdate()) FOR [DateInsert]
GO
ALTER TABLE [dbo].[IngredientType] ADD  CONSTRAINT [DF_IngredientType_Uuid]  DEFAULT (newsequentialid()) FOR [Uuid]
GO
ALTER TABLE [dbo].[FoodCategory] ADD  CONSTRAINT [DF_FoodCategory_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[FoodCategory] ADD  CONSTRAINT [DF_FoodCategory_SortOrder]  DEFAULT ((0)) FOR [SortOrder]
GO
ALTER TABLE [dbo].[FoodCategory] ADD  CONSTRAINT [DF_FoodCategory_IconUrl]  DEFAULT ('https://images.unsplash.com/photo-1476224203421-9ac39bcb3327?w=400&fit=crop') FOR [IconUrl]
GO
ALTER TABLE [dbo].[FoodCategory] ADD  CONSTRAINT [DF_FoodCategory_DateInsert]  DEFAULT (getdate()) FOR [DateInsert]
GO
ALTER TABLE [dbo].[FoodCategory] ADD  CONSTRAINT [DF_FoodCategory_Uuid]  DEFAULT (newsequentialid()) FOR [Uuid]
GO
ALTER TABLE [dbo].[Difficulty] ADD  CONSTRAINT [DF_Difficulty_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Difficulty] ADD  CONSTRAINT [DF_Difficulty_IconUrl]  DEFAULT ('https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?w=400&fit=crop') FOR [IconUrl]
GO
ALTER TABLE [dbo].[Difficulty] ADD  CONSTRAINT [DF_Difficulty_DateInsert]  DEFAULT (getdate()) FOR [DateInsert]
GO
ALTER TABLE [dbo].[Difficulty] ADD  CONSTRAINT [DF_Difficulty_Uuid]  DEFAULT (newsequentialid()) FOR [Uuid]
GO
ALTER TABLE [dbo].[DailyRecipeStats] ADD  CONSTRAINT [DF_DailyRecipeStats_GeneratedAtUtc]  DEFAULT (sysutcdatetime()) FOR [GeneratedAtUtc]
GO
ALTER TABLE [dbo].[DailyRecipeStats] ADD  CONSTRAINT [DF_DailyRecipeStats_Uuid]  DEFAULT (newsequentialid()) FOR [Uuid]
GO
ALTER TABLE [dbo].[DailyRecipeStats] ADD  CONSTRAINT [DF_DailyRecipeStats_ImageUrl]  DEFAULT ('https://images.unsplash.com/photo-1551288049-bebda4e38f71?w=400&fit=crop') FOR [ImageUrl]
GO
ALTER TABLE [dbo].[Cuisine] ADD  CONSTRAINT [DF_Cuisine_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Cuisine] ADD  CONSTRAINT [DF_Cuisine_SortOrder]  DEFAULT ((0)) FOR [SortOrder]
GO
ALTER TABLE [dbo].[Cuisine] ADD  CONSTRAINT [DF_Cuisine_IconUrl]  DEFAULT ('https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=400&fit=crop') FOR [IconUrl]
GO
ALTER TABLE [dbo].[Cuisine] ADD  CONSTRAINT [DF_Cuisine_DateInsert]  DEFAULT (getdate()) FOR [DateInsert]
GO
ALTER TABLE [dbo].[Cuisine] ADD  CONSTRAINT [DF_Cuisine_Uuid]  DEFAULT (newsequentialid()) FOR [Uuid]
GO
ALTER TABLE [dbo].[PresetAvatarImage] ADD  CONSTRAINT [DF_PresetAvatarImage_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[PresetAvatarImage] ADD  CONSTRAINT [DF_PresetAvatarImage_Uuid]  DEFAULT (newsequentialid()) FOR [Uuid]
GO
ALTER TABLE [dbo].[MoroccanRegion] ADD  CONSTRAINT [DF_MoroccanRegion_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[MoroccanRegion] ADD  CONSTRAINT [DF_MoroccanRegion_SortOrder]  DEFAULT ((0)) FOR [SortOrder]
GO
ALTER TABLE [dbo].[MoroccanRegion] ADD  CONSTRAINT [DF_MoroccanRegion_DateInsert]  DEFAULT (getdate()) FOR [DateInsert]
GO
ALTER TABLE [dbo].[MoroccanRegion] ADD  CONSTRAINT [DF_MoroccanRegion_Uuid]  DEFAULT (newsequentialid()) FOR [Uuid]
GO
ALTER TABLE [dbo].[MoroccanRegion] ADD  CONSTRAINT [DF_MoroccanRegion_ImageUrl]  DEFAULT ('https://images.unsplash.com/photo-1548017544-2e7a30ae42ca?w=400&fit=crop') FOR [ImageUrl]
GO
ALTER TABLE [dbo].[Unit] ADD  CONSTRAINT [DF_Unit_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Unit] ADD  CONSTRAINT [DF_Unit_SortOrder]  DEFAULT ((0)) FOR [SortOrder]
GO
ALTER TABLE [dbo].[Unit] ADD  CONSTRAINT [DF_Unit_DateInsert]  DEFAULT (getdate()) FOR [DateInsert]
GO
ALTER TABLE [dbo].[Unit] ADD  CONSTRAINT [DF_Unit_Uuid]  DEFAULT (newsequentialid()) FOR [Uuid]
GO
ALTER TABLE [dbo].[Unit] ADD  CONSTRAINT [DF_Unit_ImageUrl]  DEFAULT ('https://images.unsplash.com/photo-1466637574441-749b8f19452f?w=400&fit=crop') FOR [ImageUrl]
GO
ALTER TABLE [dbo].[Ingredient] ADD  CONSTRAINT [DF_Ingredient_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Ingredient] ADD  CONSTRAINT [DF_Ingredient_Uuid]  DEFAULT (newsequentialid()) FOR [Uuid]
GO
ALTER TABLE [dbo].[Ingredient] ADD  CONSTRAINT [DF_Ingredient_ImageUrl]  DEFAULT ('https://images.unsplash.com/photo-1506976785307-8732e854ad03?w=400&fit=crop') FOR [ImageUrl]
GO
ALTER TABLE [dbo].[MoroccanCity] ADD  CONSTRAINT [DF_MoroccanCity_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[MoroccanCity] ADD  CONSTRAINT [DF_MoroccanCity_SortOrder]  DEFAULT ((0)) FOR [SortOrder]
GO
ALTER TABLE [dbo].[MoroccanCity] ADD  CONSTRAINT [DF_MoroccanCity_DateInsert]  DEFAULT (getdate()) FOR [DateInsert]
GO
ALTER TABLE [dbo].[MoroccanCity] ADD  CONSTRAINT [DF_MoroccanCity_Uuid]  DEFAULT (newsequentialid()) FOR [Uuid]
GO
ALTER TABLE [dbo].[MoroccanCity] ADD  CONSTRAINT [DF_MoroccanCity_ImageUrl]  DEFAULT ('https://images.unsplash.com/photo-1569383746724-6f1b882b8f46?w=400&fit=crop') FOR [ImageUrl]
GO
ALTER TABLE [dbo].[AppUser] ADD  CONSTRAINT [DF_AppUser_ProfileImageUrl]  DEFAULT ('https://images.unsplash.com/photo-1511367461989-f85a21fda167?w=400&fit=crop') FOR [ProfileImageUrl]
GO
ALTER TABLE [dbo].[AppUser] ADD  CONSTRAINT [DF_AppUser_DateInsert]  DEFAULT (getdate()) FOR [DateInsert]
GO
ALTER TABLE [dbo].[AppUser] ADD  CONSTRAINT [DF_AppUser_DeleteReason]  DEFAULT ('') FOR [DeleteReason]
GO
ALTER TABLE [dbo].[AppUser] ADD  CONSTRAINT [DF_AppUser_Role]  DEFAULT ('USER') FOR [Role]
GO
ALTER TABLE [dbo].[AppUser] ADD  CONSTRAINT [DF_AppUser_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[AppUser] ADD  CONSTRAINT [DF_AppUser_Uuid]  DEFAULT (newsequentialid()) FOR [Uuid]
GO
ALTER TABLE [dbo].[ChefProfile] ADD  CONSTRAINT [DF_ChefProfile_IsVerified]  DEFAULT ((0)) FOR [IsVerified]
GO
ALTER TABLE [dbo].[ChefProfile] ADD  CONSTRAINT [DF_ChefProfile_DateInsert]  DEFAULT (getdate()) FOR [DateInsert]
GO
ALTER TABLE [dbo].[ChefProfile] ADD  CONSTRAINT [DF_ChefProfile_DeleteReason]  DEFAULT ('') FOR [DeleteReason]
GO
ALTER TABLE [dbo].[ChefProfile] ADD  CONSTRAINT [DF_ChefProfile_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[ChefProfile] ADD  CONSTRAINT [DF_ChefProfile_Uuid]  DEFAULT (newsequentialid()) FOR [Uuid]
GO
ALTER TABLE [dbo].[ChefProfile] ADD  CONSTRAINT [DF_ChefProfile_ImageUrl]  DEFAULT ('https://images.unsplash.com/photo-1577219491135-ce391730fb2c?w=400&fit=crop') FOR [ImageUrl]
GO
ALTER TABLE [dbo].[AppUserRefreshToken] ADD  DEFAULT (sysutcdatetime()) FOR [CreatedAtUtc]
GO
ALTER TABLE [dbo].[AppUserRefreshToken] ADD  CONSTRAINT [DF_AppUserRefreshToken_Uuid]  DEFAULT (newsequentialid()) FOR [Uuid]
GO
ALTER TABLE [dbo].[AppUserExternalLogin] ADD  CONSTRAINT [DF_AppUserExternalLogin_DateInsert]  DEFAULT (getdate()) FOR [DateInsert]
GO
ALTER TABLE [dbo].[AppUserExternalLogin] ADD  CONSTRAINT [DF_AppUserExternalLogin_LastSignInAt]  DEFAULT (getdate()) FOR [LastSignInAt]
GO
ALTER TABLE [dbo].[AppUserExternalLogin] ADD  CONSTRAINT [DF_AppUserExternalLogin_Uuid]  DEFAULT (newsequentialid()) FOR [Uuid]
GO
ALTER TABLE [dbo].[IngredientImage] ADD  CONSTRAINT [DF_IngredientImage_IsPrimary]  DEFAULT ((0)) FOR [IsPrimary]
GO
ALTER TABLE [dbo].[IngredientImage] ADD  CONSTRAINT [DF_IngredientImage_Uuid]  DEFAULT (newsequentialid()) FOR [Uuid]
GO
ALTER TABLE [dbo].[Recipe] ADD  CONSTRAINT [DF_Recipe_CoverImageUrl]  DEFAULT ('https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=400&fit=crop') FOR [CoverImageUrl]
GO
ALTER TABLE [dbo].[Recipe] ADD  CONSTRAINT [DF_Recipe_Status]  DEFAULT ('DRAFT') FOR [Status]
GO
ALTER TABLE [dbo].[Recipe] ADD  CONSTRAINT [DF_Recipe_Visibility]  DEFAULT ('PUBLIC') FOR [Visibility]
GO
ALTER TABLE [dbo].[Recipe] ADD  CONSTRAINT [DF_Recipe_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Recipe] ADD  CONSTRAINT [DF_Recipe_DateInsert]  DEFAULT (getdate()) FOR [DateInsert]
GO
ALTER TABLE [dbo].[Recipe] ADD  CONSTRAINT [DF_Recipe_DeleteReason]  DEFAULT ('') FOR [DeleteReason]
GO
ALTER TABLE [dbo].[Recipe] ADD  CONSTRAINT [DF_Recipe_Uuid]  DEFAULT (newsequentialid()) FOR [Uuid]
GO
ALTER TABLE [dbo].[RecipeStep] ADD  CONSTRAINT [DF_RecipeStep_Uuid]  DEFAULT (newsequentialid()) FOR [Uuid]
GO
ALTER TABLE [dbo].[RecipeStep] ADD  CONSTRAINT [DF_RecipeStep_ImageUrl]  DEFAULT ('https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?w=400&fit=crop') FOR [ImageUrl]
GO
ALTER TABLE [dbo].[RecipeShare] ADD  CONSTRAINT [DF_RecipeShare_SharedAt]  DEFAULT (getdate()) FOR [SharedAt]
GO
ALTER TABLE [dbo].[RecipeShare] ADD  CONSTRAINT [DF_RecipeShare_Uuid]  DEFAULT (newsequentialid()) FOR [Uuid]
GO
ALTER TABLE [dbo].[RecipeShare] ADD  CONSTRAINT [DF_RecipeShare_ImageUrl]  DEFAULT ('https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=400&fit=crop') FOR [ImageUrl]
GO
ALTER TABLE [dbo].[RecipeNutrition] ADD  CONSTRAINT [DF_RecipeNutrition_Uuid]  DEFAULT (newsequentialid()) FOR [Uuid]
GO
ALTER TABLE [dbo].[RecipeNutrition] ADD  CONSTRAINT [DF_RecipeNutrition_ImageUrl]  DEFAULT ('https://images.unsplash.com/photo-1490645935967-10de6ba17061?w=400&fit=crop') FOR [ImageUrl]
GO
ALTER TABLE [dbo].[RecipeLike] ADD  CONSTRAINT [DF_RecipeLike_LikedAt]  DEFAULT (getdate()) FOR [LikedAt]
GO
ALTER TABLE [dbo].[RecipeIngredient] ADD  CONSTRAINT [DF_RecipeIngredient_IsOptional]  DEFAULT ((0)) FOR [IsOptional]
GO
ALTER TABLE [dbo].[RecipeIngredient] ADD  CONSTRAINT [DF_RecipeIngredient_Uuid]  DEFAULT (newsequentialid()) FOR [Uuid]
GO
ALTER TABLE [dbo].[RecipeIngredient] ADD  CONSTRAINT [DF_RecipeIngredient_ImageUrl]  DEFAULT ('https://images.unsplash.com/photo-1506976785307-8732e854ad03?w=400&fit=crop') FOR [ImageUrl]
GO
ALTER TABLE [dbo].[RecipeImage] ADD  CONSTRAINT [DF_RecipeImage_IsPrimary]  DEFAULT ((0)) FOR [IsPrimary]
GO
ALTER TABLE [dbo].[RecipeImage] ADD  CONSTRAINT [DF_RecipeImage_Uuid]  DEFAULT (newsequentialid()) FOR [Uuid]
GO
ALTER TABLE [dbo].[Ingredient]  WITH CHECK ADD  CONSTRAINT [FK_Ingredient_IngredientType] FOREIGN KEY([IngredientTypeId])
REFERENCES [dbo].[IngredientType] ([Id])
GO
ALTER TABLE [dbo].[Ingredient] CHECK CONSTRAINT [FK_Ingredient_IngredientType]
GO
ALTER TABLE [dbo].[Ingredient]  WITH CHECK ADD  CONSTRAINT [FK_Ingredient_Unit] FOREIGN KEY([DefaultUnitId])
REFERENCES [dbo].[Unit] ([Id])
GO
ALTER TABLE [dbo].[Ingredient] CHECK CONSTRAINT [FK_Ingredient_Unit]
GO
ALTER TABLE [dbo].[MoroccanCity]  WITH CHECK ADD  CONSTRAINT [FK_MoroccanCity_MoroccanRegion] FOREIGN KEY([RegionId])
REFERENCES [dbo].[MoroccanRegion] ([Id])
GO
ALTER TABLE [dbo].[MoroccanCity] CHECK CONSTRAINT [FK_MoroccanCity_MoroccanRegion]
GO
ALTER TABLE [dbo].[AppUser]  WITH CHECK ADD  CONSTRAINT [FK_AppUser_DeletedByUser] FOREIGN KEY([DeletedByUserId])
REFERENCES [dbo].[AppUser] ([Id])
GO
ALTER TABLE [dbo].[AppUser] CHECK CONSTRAINT [FK_AppUser_DeletedByUser]
GO
ALTER TABLE [dbo].[AppUser]  WITH CHECK ADD  CONSTRAINT [FK_AppUser_PresetAvatarImage] FOREIGN KEY([PresetAvatarImageId])
REFERENCES [dbo].[PresetAvatarImage] ([Id])
GO
ALTER TABLE [dbo].[AppUser] CHECK CONSTRAINT [FK_AppUser_PresetAvatarImage]
GO
ALTER TABLE [dbo].[ChefProfile]  WITH CHECK ADD  CONSTRAINT [FK_ChefProfile_AppUser] FOREIGN KEY([UserId])
REFERENCES [dbo].[AppUser] ([Id])
GO
ALTER TABLE [dbo].[ChefProfile] CHECK CONSTRAINT [FK_ChefProfile_AppUser]
GO
ALTER TABLE [dbo].[ChefProfile]  WITH CHECK ADD  CONSTRAINT [FK_ChefProfile_DeletedByUser] FOREIGN KEY([DeletedByUserId])
REFERENCES [dbo].[AppUser] ([Id])
GO
ALTER TABLE [dbo].[ChefProfile] CHECK CONSTRAINT [FK_ChefProfile_DeletedByUser]
GO
ALTER TABLE [dbo].[AppUserRefreshToken]  WITH CHECK ADD  CONSTRAINT [FK_AppUserRefreshToken_AppUser] FOREIGN KEY([AppUserId])
REFERENCES [dbo].[AppUser] ([Id])
GO
ALTER TABLE [dbo].[AppUserRefreshToken] CHECK CONSTRAINT [FK_AppUserRefreshToken_AppUser]
GO
ALTER TABLE [dbo].[AppUserExternalLogin]  WITH CHECK ADD  CONSTRAINT [FK_AppUserExternalLogin_AppUser] FOREIGN KEY([AppUserId])
REFERENCES [dbo].[AppUser] ([Id])
GO
ALTER TABLE [dbo].[AppUserExternalLogin] CHECK CONSTRAINT [FK_AppUserExternalLogin_AppUser]
GO
ALTER TABLE [dbo].[IngredientImage]  WITH CHECK ADD  CONSTRAINT [FK_IngredientImage_Ingredient] FOREIGN KEY([IngredientId])
REFERENCES [dbo].[Ingredient] ([Id])
GO
ALTER TABLE [dbo].[IngredientImage] CHECK CONSTRAINT [FK_IngredientImage_Ingredient]
GO
ALTER TABLE [dbo].[Recipe]  WITH CHECK ADD  CONSTRAINT [FK_Recipe_ChefProfile] FOREIGN KEY([ChefId])
REFERENCES [dbo].[ChefProfile] ([Id])
GO
ALTER TABLE [dbo].[Recipe] CHECK CONSTRAINT [FK_Recipe_ChefProfile]
GO
ALTER TABLE [dbo].[Recipe]  WITH CHECK ADD  CONSTRAINT [FK_Recipe_Cuisine] FOREIGN KEY([CuisineId])
REFERENCES [dbo].[Cuisine] ([Id])
GO
ALTER TABLE [dbo].[Recipe] CHECK CONSTRAINT [FK_Recipe_Cuisine]
GO
ALTER TABLE [dbo].[Recipe]  WITH CHECK ADD  CONSTRAINT [FK_Recipe_DeletedByUser] FOREIGN KEY([DeletedByUserId])
REFERENCES [dbo].[AppUser] ([Id])
GO
ALTER TABLE [dbo].[Recipe] CHECK CONSTRAINT [FK_Recipe_DeletedByUser]
GO
ALTER TABLE [dbo].[Recipe]  WITH CHECK ADD  CONSTRAINT [FK_Recipe_Difficulty] FOREIGN KEY([DifficultyId])
REFERENCES [dbo].[Difficulty] ([Id])
GO
ALTER TABLE [dbo].[Recipe] CHECK CONSTRAINT [FK_Recipe_Difficulty]
GO
ALTER TABLE [dbo].[Recipe]  WITH CHECK ADD  CONSTRAINT [FK_Recipe_MoroccanCity] FOREIGN KEY([CityId])
REFERENCES [dbo].[MoroccanCity] ([Id])
GO
ALTER TABLE [dbo].[Recipe] CHECK CONSTRAINT [FK_Recipe_MoroccanCity]
GO
ALTER TABLE [dbo].[RecipeStep]  WITH CHECK ADD  CONSTRAINT [FK_RecipeStep_Recipe] FOREIGN KEY([RecipeId])
REFERENCES [dbo].[Recipe] ([Id])
GO
ALTER TABLE [dbo].[RecipeStep] CHECK CONSTRAINT [FK_RecipeStep_Recipe]
GO
ALTER TABLE [dbo].[RecipeShare]  WITH CHECK ADD  CONSTRAINT [FK_RecipeShare_Recipe] FOREIGN KEY([RecipeId])
REFERENCES [dbo].[Recipe] ([Id])
GO
ALTER TABLE [dbo].[RecipeShare] CHECK CONSTRAINT [FK_RecipeShare_Recipe]
GO
ALTER TABLE [dbo].[RecipeShare]  WITH CHECK ADD  CONSTRAINT [FK_RecipeShare_SharedByUser] FOREIGN KEY([SharedByUserId])
REFERENCES [dbo].[AppUser] ([Id])
GO
ALTER TABLE [dbo].[RecipeShare] CHECK CONSTRAINT [FK_RecipeShare_SharedByUser]
GO
ALTER TABLE [dbo].[RecipeShare]  WITH CHECK ADD  CONSTRAINT [FK_RecipeShare_SharedWithUser] FOREIGN KEY([SharedWithUserId])
REFERENCES [dbo].[AppUser] ([Id])
GO
ALTER TABLE [dbo].[RecipeShare] CHECK CONSTRAINT [FK_RecipeShare_SharedWithUser]
GO
ALTER TABLE [dbo].[RecipeNutrition]  WITH CHECK ADD  CONSTRAINT [FK_RecipeNutrition_Recipe] FOREIGN KEY([RecipeId])
REFERENCES [dbo].[Recipe] ([Id])
GO
ALTER TABLE [dbo].[RecipeNutrition] CHECK CONSTRAINT [FK_RecipeNutrition_Recipe]
GO
ALTER TABLE [dbo].[RecipeLike]  WITH CHECK ADD  CONSTRAINT [FK_RecipeLike_AppUser] FOREIGN KEY([UserId])
REFERENCES [dbo].[AppUser] ([Id])
GO
ALTER TABLE [dbo].[RecipeLike] CHECK CONSTRAINT [FK_RecipeLike_AppUser]
GO
ALTER TABLE [dbo].[RecipeLike]  WITH CHECK ADD  CONSTRAINT [FK_RecipeLike_Recipe] FOREIGN KEY([RecipeId])
REFERENCES [dbo].[Recipe] ([Id])
GO
ALTER TABLE [dbo].[RecipeLike] CHECK CONSTRAINT [FK_RecipeLike_Recipe]
GO
ALTER TABLE [dbo].[RecipeIngredient]  WITH CHECK ADD  CONSTRAINT [FK_RecipeIngredient_Ingredient] FOREIGN KEY([IngredientId])
REFERENCES [dbo].[Ingredient] ([Id])
GO
ALTER TABLE [dbo].[RecipeIngredient] CHECK CONSTRAINT [FK_RecipeIngredient_Ingredient]
GO
ALTER TABLE [dbo].[RecipeIngredient]  WITH CHECK ADD  CONSTRAINT [FK_RecipeIngredient_Recipe] FOREIGN KEY([RecipeId])
REFERENCES [dbo].[Recipe] ([Id])
GO
ALTER TABLE [dbo].[RecipeIngredient] CHECK CONSTRAINT [FK_RecipeIngredient_Recipe]
GO
ALTER TABLE [dbo].[RecipeIngredient]  WITH CHECK ADD  CONSTRAINT [FK_RecipeIngredient_Unit] FOREIGN KEY([UnitId])
REFERENCES [dbo].[Unit] ([Id])
GO
ALTER TABLE [dbo].[RecipeIngredient] CHECK CONSTRAINT [FK_RecipeIngredient_Unit]
GO
ALTER TABLE [dbo].[RecipeImage]  WITH CHECK ADD  CONSTRAINT [FK_RecipeImage_Recipe] FOREIGN KEY([RecipeId])
REFERENCES [dbo].[Recipe] ([Id])
GO
ALTER TABLE [dbo].[RecipeImage] CHECK CONSTRAINT [FK_RecipeImage_Recipe]
GO
ALTER TABLE [dbo].[RecipeCategory]  WITH CHECK ADD  CONSTRAINT [FK_RecipeCategory_FoodCategory] FOREIGN KEY([FoodCategoryId])
REFERENCES [dbo].[FoodCategory] ([Id])
GO
ALTER TABLE [dbo].[RecipeCategory] CHECK CONSTRAINT [FK_RecipeCategory_FoodCategory]
GO
ALTER TABLE [dbo].[RecipeCategory]  WITH CHECK ADD  CONSTRAINT [FK_RecipeCategory_Recipe] FOREIGN KEY([RecipeId])
REFERENCES [dbo].[Recipe] ([Id])
GO
ALTER TABLE [dbo].[RecipeCategory] CHECK CONSTRAINT [FK_RecipeCategory_Recipe]
GO
ALTER TABLE [dbo].[Recipe]  WITH CHECK ADD  CONSTRAINT [CK_Recipe_Status] CHECK  (([Status]='ARCHIVED' OR [Status]='DRAFT' OR [Status]='PUBLISHED'))
GO
ALTER TABLE [dbo].[Recipe] CHECK CONSTRAINT [CK_Recipe_Status]
GO
ALTER TABLE [dbo].[Recipe]  WITH CHECK ADD  CONSTRAINT [CK_Recipe_Visibility] CHECK  (([Visibility]='UNLISTED' OR [Visibility]='PRIVATE' OR [Visibility]='PUBLIC'))
GO
ALTER TABLE [dbo].[Recipe] CHECK CONSTRAINT [CK_Recipe_Visibility]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

    CREATE TRIGGER [dbo].[TR_DailyRecipeStats_GenerateCode]
    ON [dbo].[DailyRecipeStats] AFTER INSERT AS
    BEGIN
        SET NOCOUNT ON;
        UPDATE t SET t.[Code] = 'DRS_' + CONVERT(VARCHAR(8), t.[StatsDate], 112)
        FROM [dbo].[DailyRecipeStats] t
        INNER JOIN inserted i ON t.Id = i.Id
        WHERE t.[Code] IS NULL OR t.[Code] = '';
    END;

GO
ALTER TABLE [dbo].[DailyRecipeStats] ENABLE TRIGGER [TR_DailyRecipeStats_GenerateCode]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
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
ALTER TABLE [dbo].[Ingredient] ENABLE TRIGGER [TR_Ingredient_GenerateCode]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
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
ALTER TABLE [dbo].[MoroccanCity] ENABLE TRIGGER [TR_MoroccanCity_GenerateCode]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
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
ALTER TABLE [dbo].[AppUser] ENABLE TRIGGER [TR_AppUser_GenerateCode]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
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
ALTER TABLE [dbo].[ChefProfile] ENABLE TRIGGER [TR_ChefProfile_GenerateCode]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

    CREATE TRIGGER [dbo].[TR_AppUserRefreshToken_GenerateCode]
    ON [dbo].[AppUserRefreshToken] AFTER INSERT AS
    BEGIN
        SET NOCOUNT ON;
        UPDATE t SET t.[Code] = 'RTK_' + UPPER(LEFT(CONVERT(VARCHAR(36), NEWID()), 8))
        FROM [dbo].[AppUserRefreshToken] t
        INNER JOIN inserted i ON t.Id = i.Id
        WHERE t.[Code] IS NULL OR t.[Code] = '';
    END;

GO
ALTER TABLE [dbo].[AppUserRefreshToken] ENABLE TRIGGER [TR_AppUserRefreshToken_GenerateCode]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

    CREATE TRIGGER [dbo].[TR_AppUserExternalLogin_GenerateCode]
    ON [dbo].[AppUserExternalLogin] AFTER INSERT AS
    BEGIN
        SET NOCOUNT ON;
        UPDATE t SET t.[Code] = 'EXL_' + UPPER(LEFT(CONVERT(VARCHAR(36), NEWID()), 8))
        FROM [dbo].[AppUserExternalLogin] t
        INNER JOIN inserted i ON t.Id = i.Id
        WHERE t.[Code] IS NULL OR t.[Code] = '';
    END;

GO
ALTER TABLE [dbo].[AppUserExternalLogin] ENABLE TRIGGER [TR_AppUserExternalLogin_GenerateCode]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
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
ALTER TABLE [dbo].[IngredientImage] ENABLE TRIGGER [TR_IngredientImage_GenerateCode]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
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
ALTER TABLE [dbo].[Recipe] ENABLE TRIGGER [TR_Recipe_GenerateCode]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
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
ALTER TABLE [dbo].[RecipeStep] ENABLE TRIGGER [TR_RecipeStep_GenerateCode]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
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
ALTER TABLE [dbo].[RecipeShare] ENABLE TRIGGER [TR_RecipeShare_GenerateCode]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
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
ALTER TABLE [dbo].[RecipeIngredient] ENABLE TRIGGER [TR_RecipeIngredient_GenerateCode]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
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
ALTER TABLE [dbo].[RecipeImage] ENABLE TRIGGER [TR_RecipeImage_GenerateCode]
GO
COMMIT TRANSACTION;
GO
SET NOEXEC OFF;
GO
