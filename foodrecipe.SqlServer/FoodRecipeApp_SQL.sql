-- Script compatible avec Azure SQL Database.
-- A executer directement sur la base cible. Aucun USE n'est necessaire.

IF OBJECT_ID(N'[dbo].[PresetAvatarImage]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[PresetAvatarImage](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Code] [varchar](50) NOT NULL,
        [Label] [varchar](100) NOT NULL,
        [ImageUrl] [varchar](500) NOT NULL,
        [BackgroundColor] [varchar](20) NULL,
        [SortOrder] [int] NOT NULL,
        [IsActive] [bit] NOT NULL CONSTRAINT [DF_PresetAvatarImage_IsActive] DEFAULT ((1)),
        CONSTRAINT [PK_PresetAvatarImage] PRIMARY KEY ([Id]),
        CONSTRAINT [UQ_PresetAvatarImage_Code] UNIQUE ([Code])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[AppUser]', N'U') IS NULL
BEGIN
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
        [DateInsert] [smalldatetime] NOT NULL CONSTRAINT [DF_AppUser_DateInsert] DEFAULT (GETDATE()),
        [DateModif] [smalldatetime] NULL,
        CONSTRAINT [PK_AppUser] PRIMARY KEY ([Id]),
        CONSTRAINT [UQ_AppUser_UserName] UNIQUE ([UserName]),
        CONSTRAINT [UQ_AppUser_Email] UNIQUE ([Email]),
        CONSTRAINT [FK_AppUser_PresetAvatarImage] FOREIGN KEY ([PresetAvatarImageId]) REFERENCES [dbo].[PresetAvatarImage]([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[ChefProfile]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ChefProfile](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [int] NOT NULL,
        [DisplayName] [varchar](120) NOT NULL,
        [Bio] [varchar](1000) NULL,
        [Specialty] [varchar](150) NULL,
        [YearsOfExperience] [int] NULL,
        [IsVerified] [bit] NOT NULL CONSTRAINT [DF_ChefProfile_IsVerified] DEFAULT ((0)),
        [DateInsert] [smalldatetime] NOT NULL CONSTRAINT [DF_ChefProfile_DateInsert] DEFAULT (GETDATE()),
        CONSTRAINT [PK_ChefProfile] PRIMARY KEY ([Id]),
        CONSTRAINT [UQ_ChefProfile_UserId] UNIQUE ([UserId]),
        CONSTRAINT [FK_ChefProfile_AppUser] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AppUser]([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Difficulty]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Difficulty](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Name] [varchar](50) NOT NULL,
        [SortOrder] [int] NOT NULL,
        CONSTRAINT [PK_Difficulty] PRIMARY KEY ([Id]),
        CONSTRAINT [UQ_Difficulty_Name] UNIQUE ([Name])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[FoodCategory]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[FoodCategory](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Name] [varchar](100) NOT NULL,
        [Description] [varchar](250) NULL,
        CONSTRAINT [PK_FoodCategory] PRIMARY KEY ([Id]),
        CONSTRAINT [UQ_FoodCategory_Name] UNIQUE ([Name])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Cuisine]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Cuisine](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Name] [varchar](100) NOT NULL,
        CONSTRAINT [PK_Cuisine] PRIMARY KEY ([Id]),
        CONSTRAINT [UQ_Cuisine_Name] UNIQUE ([Name])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[MoroccanRegion]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[MoroccanRegion](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Code] [varchar](20) NOT NULL,
        [Name] [varchar](150) NOT NULL,
        CONSTRAINT [PK_MoroccanRegion] PRIMARY KEY ([Id]),
        CONSTRAINT [UQ_MoroccanRegion_Code] UNIQUE ([Code]),
        CONSTRAINT [UQ_MoroccanRegion_Name] UNIQUE ([Name])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[MoroccanCity]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[MoroccanCity](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [RegionId] [int] NOT NULL,
        [Name] [varchar](150) NOT NULL,
        [Slug] [varchar](170) NOT NULL,
        CONSTRAINT [PK_MoroccanCity] PRIMARY KEY ([Id]),
        CONSTRAINT [UQ_MoroccanCity_Name] UNIQUE ([Name]),
        CONSTRAINT [UQ_MoroccanCity_Slug] UNIQUE ([Slug]),
        CONSTRAINT [FK_MoroccanCity_MoroccanRegion] FOREIGN KEY ([RegionId]) REFERENCES [dbo].[MoroccanRegion]([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Unit]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Unit](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Code] [varchar](20) NOT NULL,
        [Label] [varchar](50) NOT NULL,
        CONSTRAINT [PK_Unit] PRIMARY KEY ([Id]),
        CONSTRAINT [UQ_Unit_Code] UNIQUE ([Code])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[IngredientType]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[IngredientType](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Code] [varchar](30) NOT NULL,
        [Label] [varchar](80) NOT NULL,
        CONSTRAINT [PK_IngredientType] PRIMARY KEY ([Id]),
        CONSTRAINT [UQ_IngredientType_Code] UNIQUE ([Code])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Ingredient]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Ingredient](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Name] [varchar](150) NOT NULL,
        [IngredientTypeId] [int] NOT NULL,
        [DefaultUnitId] [int] NULL,
        [CaloriesPer100g] [decimal](10,2) NULL,
        [IsActive] [bit] NOT NULL CONSTRAINT [DF_Ingredient_IsActive] DEFAULT ((1)),
        CONSTRAINT [PK_Ingredient] PRIMARY KEY ([Id]),
        CONSTRAINT [UQ_Ingredient_Name] UNIQUE ([Name]),
        CONSTRAINT [FK_Ingredient_IngredientType] FOREIGN KEY ([IngredientTypeId]) REFERENCES [dbo].[IngredientType]([Id]),
        CONSTRAINT [FK_Ingredient_Unit] FOREIGN KEY ([DefaultUnitId]) REFERENCES [dbo].[Unit]([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[IngredientImage]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[IngredientImage](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [IngredientId] [int] NOT NULL,
        [ImageUrl] [varchar](500) NOT NULL,
        [AltText] [varchar](250) NULL,
        [IsPrimary] [bit] NOT NULL CONSTRAINT [DF_IngredientImage_IsPrimary] DEFAULT ((0)),
        [SortOrder] [int] NOT NULL,
        CONSTRAINT [PK_IngredientImage] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_IngredientImage_Ingredient] FOREIGN KEY ([IngredientId]) REFERENCES [dbo].[Ingredient]([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Recipe]', N'U') IS NULL
BEGIN
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
        [Status] [varchar](20) NOT NULL CONSTRAINT [DF_Recipe_Status] DEFAULT ('DRAFT'),
        [Visibility] [varchar](20) NOT NULL CONSTRAINT [DF_Recipe_Visibility] DEFAULT ('Publique'),
        [IsActive] [bit] NOT NULL CONSTRAINT [DF_Recipe_IsActive] DEFAULT ((1)),
        [DateInsert] [smalldatetime] NOT NULL CONSTRAINT [DF_Recipe_DateInsert] DEFAULT (GETDATE()),
        [DateModif] [smalldatetime] NULL,
        [PublishedAt] [smalldatetime] NULL,
        CONSTRAINT [PK_Recipe] PRIMARY KEY ([Id]),
        CONSTRAINT [UQ_Recipe_Slug] UNIQUE ([Slug]),
        CONSTRAINT [CK_Recipe_Status] CHECK ([Status] IN ('PUBLISH', 'DRAFT', 'ARCHIVE')),
        CONSTRAINT [FK_Recipe_ChefProfile] FOREIGN KEY ([ChefId]) REFERENCES [dbo].[ChefProfile]([Id]),
        CONSTRAINT [FK_Recipe_Difficulty] FOREIGN KEY ([DifficultyId]) REFERENCES [dbo].[Difficulty]([Id]),
        CONSTRAINT [FK_Recipe_Cuisine] FOREIGN KEY ([CuisineId]) REFERENCES [dbo].[Cuisine]([Id]),
        CONSTRAINT [FK_Recipe_MoroccanCity] FOREIGN KEY ([CityId]) REFERENCES [dbo].[MoroccanCity]([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[RecipeImage]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[RecipeImage](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [RecipeId] [int] NOT NULL,
        [ImageUrl] [varchar](500) NOT NULL,
        [AltText] [varchar](250) NULL,
        [IsPrimary] [bit] NOT NULL CONSTRAINT [DF_RecipeImage_IsPrimary] DEFAULT ((0)),
        [SortOrder] [int] NOT NULL,
        CONSTRAINT [PK_RecipeImage] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RecipeImage_Recipe] FOREIGN KEY ([RecipeId]) REFERENCES [dbo].[Recipe]([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[RecipeCategory]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[RecipeCategory](
        [RecipeId] [int] NOT NULL,
        [FoodCategoryId] [int] NOT NULL,
        CONSTRAINT [PK_RecipeCategory] PRIMARY KEY ([RecipeId], [FoodCategoryId]),
        CONSTRAINT [FK_RecipeCategory_Recipe] FOREIGN KEY ([RecipeId]) REFERENCES [dbo].[Recipe]([Id]),
        CONSTRAINT [FK_RecipeCategory_FoodCategory] FOREIGN KEY ([FoodCategoryId]) REFERENCES [dbo].[FoodCategory]([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[RecipeIngredient]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[RecipeIngredient](
        [RecipeId] [int] NOT NULL,
        [IngredientId] [int] NOT NULL,
        [Quantity] [decimal](10,2) NOT NULL,
        [UnitId] [int] NULL,
        [IsOptional] [bit] NOT NULL CONSTRAINT [DF_RecipeIngredient_IsOptional] DEFAULT ((0)),
        [Notes] [varchar](250) NULL,
        [SortOrder] [int] NOT NULL,
        CONSTRAINT [PK_RecipeIngredient] PRIMARY KEY ([RecipeId], [IngredientId]),
        CONSTRAINT [FK_RecipeIngredient_Recipe] FOREIGN KEY ([RecipeId]) REFERENCES [dbo].[Recipe]([Id]),
        CONSTRAINT [FK_RecipeIngredient_Ingredient] FOREIGN KEY ([IngredientId]) REFERENCES [dbo].[Ingredient]([Id]),
        CONSTRAINT [FK_RecipeIngredient_Unit] FOREIGN KEY ([UnitId]) REFERENCES [dbo].[Unit]([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[RecipeStep]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[RecipeStep](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [RecipeId] [int] NOT NULL,
        [StepNumber] [int] NOT NULL,
        [Title] [varchar](150) NULL,
        [Instruction] [varchar](2000) NOT NULL,
        [DurationMinutes] [int] NULL,
        CONSTRAINT [PK_RecipeStep] PRIMARY KEY ([Id]),
        CONSTRAINT [UQ_RecipeStep_Recipe_StepNumber] UNIQUE ([RecipeId], [StepNumber]),
        CONSTRAINT [FK_RecipeStep_Recipe] FOREIGN KEY ([RecipeId]) REFERENCES [dbo].[Recipe]([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[RecipeNutrition]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[RecipeNutrition](
        [RecipeId] [int] NOT NULL,
        [CaloriesPerServing] [decimal](10,2) NOT NULL,
        [ProteinGrams] [decimal](10,2) NULL,
        [CarbsGrams] [decimal](10,2) NULL,
        [FatGrams] [decimal](10,2) NULL,
        CONSTRAINT [PK_RecipeNutrition] PRIMARY KEY ([RecipeId]),
        CONSTRAINT [FK_RecipeNutrition_Recipe] FOREIGN KEY ([RecipeId]) REFERENCES [dbo].[Recipe]([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[RecipeLike]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[RecipeLike](
        [RecipeId] [int] NOT NULL,
        [UserId] [int] NOT NULL,
        [LikedAt] [smalldatetime] NOT NULL CONSTRAINT [DF_RecipeLike_LikedAt] DEFAULT (GETDATE()),
        CONSTRAINT [PK_RecipeLike] PRIMARY KEY ([RecipeId], [UserId]),
        CONSTRAINT [FK_RecipeLike_Recipe] FOREIGN KEY ([RecipeId]) REFERENCES [dbo].[Recipe]([Id]),
        CONSTRAINT [FK_RecipeLike_AppUser] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AppUser]([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[RecipeShare]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[RecipeShare](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [RecipeId] [int] NOT NULL,
        [SharedByUserId] [int] NOT NULL,
        [SharedWithUserId] [int] NULL,
        [ShareChannel] [varchar](30) NOT NULL,
        [ShareMessage] [varchar](500) NULL,
        [SharedAt] [smalldatetime] NOT NULL CONSTRAINT [DF_RecipeShare_SharedAt] DEFAULT (GETDATE()),
        CONSTRAINT [PK_RecipeShare] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RecipeShare_Recipe] FOREIGN KEY ([RecipeId]) REFERENCES [dbo].[Recipe]([Id]),
        CONSTRAINT [FK_RecipeShare_SharedByUser] FOREIGN KEY ([SharedByUserId]) REFERENCES [dbo].[AppUser]([Id]),
        CONSTRAINT [FK_RecipeShare_SharedWithUser] FOREIGN KEY ([SharedWithUserId]) REFERENCES [dbo].[AppUser]([Id])
    );
END;
GO

IF COL_LENGTH(N'[dbo].[AppUser]', N'PresetAvatarImageId') IS NULL
BEGIN
    ALTER TABLE [dbo].[AppUser] ADD [PresetAvatarImageId] [int] NULL;
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM [sys].[foreign_keys]
    WHERE [name] = 'FK_AppUser_PresetAvatarImage'
      AND [parent_object_id] = OBJECT_ID(N'[dbo].[AppUser]')
)
BEGIN
    ALTER TABLE [dbo].[AppUser]
    WITH CHECK
    ADD CONSTRAINT [FK_AppUser_PresetAvatarImage]
    FOREIGN KEY ([PresetAvatarImageId]) REFERENCES [dbo].[PresetAvatarImage]([Id]);
END;
GO

IF COL_LENGTH(N'[dbo].[Recipe]', N'CityId') IS NULL
BEGIN
    ALTER TABLE [dbo].[Recipe] ADD [CityId] [int] NULL;
END;
GO

IF COL_LENGTH(N'[dbo].[Recipe]', N'IsActive') IS NULL
BEGIN
    ALTER TABLE [dbo].[Recipe]
    ADD [IsActive] [bit] NOT NULL
        CONSTRAINT [DF_Recipe_IsActive] DEFAULT ((1)) WITH VALUES;
END;
GO

UPDATE [dbo].[Recipe]
SET [Status] = CASE UPPER(LTRIM(RTRIM([Status])))
    WHEN 'BROUILLON' THEN 'DRAFT'
    WHEN 'DRAFT' THEN 'DRAFT'
    WHEN 'PUBLIE' THEN 'PUBLISH'
    WHEN 'PUBLISH' THEN 'PUBLISH'
    WHEN 'ARCHIVE' THEN 'ARCHIVE'
    ELSE [Status]
END
WHERE [Status] IS NOT NULL;
GO

IF EXISTS (
    SELECT 1
    FROM [sys].[default_constraints]
    WHERE [name] = 'DF_Recipe_Status'
      AND [parent_object_id] = OBJECT_ID(N'[dbo].[Recipe]')
)
BEGIN
    ALTER TABLE [dbo].[Recipe] DROP CONSTRAINT [DF_Recipe_Status];
END;
GO

ALTER TABLE [dbo].[Recipe]
ADD CONSTRAINT [DF_Recipe_Status] DEFAULT ('DRAFT') FOR [Status];
GO

IF NOT EXISTS (
    SELECT 1
    FROM [sys].[check_constraints]
    WHERE [name] = 'CK_Recipe_Status'
      AND [parent_object_id] = OBJECT_ID(N'[dbo].[Recipe]')
)
BEGIN
    ALTER TABLE [dbo].[Recipe]
    WITH CHECK
    ADD CONSTRAINT [CK_Recipe_Status]
    CHECK ([Status] IN ('PUBLISH', 'DRAFT', 'ARCHIVE'));
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM [sys].[foreign_keys]
    WHERE [name] = 'FK_Recipe_MoroccanCity'
      AND [parent_object_id] = OBJECT_ID(N'[dbo].[Recipe]')
)
BEGIN
    ALTER TABLE [dbo].[Recipe]
    WITH CHECK
    ADD CONSTRAINT [FK_Recipe_MoroccanCity]
    FOREIGN KEY ([CityId]) REFERENCES [dbo].[MoroccanCity]([Id]);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM [sys].[indexes]
    WHERE [name] = 'IX_AppUser_PresetAvatarImageId'
      AND [object_id] = OBJECT_ID(N'[dbo].[AppUser]')
)
BEGIN
    CREATE INDEX [IX_AppUser_PresetAvatarImageId] ON [dbo].[AppUser]([PresetAvatarImageId]);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM [sys].[indexes]
    WHERE [name] = 'IX_MoroccanCity_RegionId'
      AND [object_id] = OBJECT_ID(N'[dbo].[MoroccanCity]')
)
BEGIN
    CREATE INDEX [IX_MoroccanCity_RegionId] ON [dbo].[MoroccanCity]([RegionId]);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM [sys].[indexes]
    WHERE [name] = 'IX_Recipe_ChefId'
      AND [object_id] = OBJECT_ID(N'[dbo].[Recipe]')
)
BEGIN
    CREATE INDEX [IX_Recipe_ChefId] ON [dbo].[Recipe]([ChefId]);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM [sys].[indexes]
    WHERE [name] = 'IX_Recipe_DifficultyId'
      AND [object_id] = OBJECT_ID(N'[dbo].[Recipe]')
)
BEGIN
    CREATE INDEX [IX_Recipe_DifficultyId] ON [dbo].[Recipe]([DifficultyId]);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM [sys].[indexes]
    WHERE [name] = 'IX_Recipe_CuisineId'
      AND [object_id] = OBJECT_ID(N'[dbo].[Recipe]')
)
BEGIN
    CREATE INDEX [IX_Recipe_CuisineId] ON [dbo].[Recipe]([CuisineId]);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM [sys].[indexes]
    WHERE [name] = 'IX_Recipe_CityId'
      AND [object_id] = OBJECT_ID(N'[dbo].[Recipe]')
)
BEGIN
    CREATE INDEX [IX_Recipe_CityId] ON [dbo].[Recipe]([CityId]);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM [sys].[indexes]
    WHERE [name] = 'IX_RecipeIngredient_IngredientId'
      AND [object_id] = OBJECT_ID(N'[dbo].[RecipeIngredient]')
)
BEGIN
    CREATE INDEX [IX_RecipeIngredient_IngredientId] ON [dbo].[RecipeIngredient]([IngredientId]);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM [sys].[indexes]
    WHERE [name] = 'IX_IngredientImage_IngredientId'
      AND [object_id] = OBJECT_ID(N'[dbo].[IngredientImage]')
)
BEGIN
    CREATE INDEX [IX_IngredientImage_IngredientId] ON [dbo].[IngredientImage]([IngredientId]);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM [sys].[indexes]
    WHERE [name] = 'IX_RecipeImage_RecipeId'
      AND [object_id] = OBJECT_ID(N'[dbo].[RecipeImage]')
)
BEGIN
    CREATE INDEX [IX_RecipeImage_RecipeId] ON [dbo].[RecipeImage]([RecipeId]);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM [sys].[indexes]
    WHERE [name] = 'IX_RecipeLike_UserId'
      AND [object_id] = OBJECT_ID(N'[dbo].[RecipeLike]')
)
BEGIN
    CREATE INDEX [IX_RecipeLike_UserId] ON [dbo].[RecipeLike]([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM [sys].[indexes]
    WHERE [name] = 'IX_RecipeShare_SharedByUserId'
      AND [object_id] = OBJECT_ID(N'[dbo].[RecipeShare]')
)
BEGIN
    CREATE INDEX [IX_RecipeShare_SharedByUserId] ON [dbo].[RecipeShare]([SharedByUserId]);
END;
GO

MERGE [dbo].[Difficulty] AS [target]
USING (VALUES
    ('Facile', 1),
    ('Moyen', 2),
    ('Difficile', 3),
    ('Tres difficile', 4),
    ('Exotic', 5)
) AS [source]([Name], [SortOrder])
ON [target].[Name] = [source].[Name]
WHEN MATCHED THEN
    UPDATE SET [SortOrder] = [source].[SortOrder]
WHEN NOT MATCHED THEN
    INSERT ([Name], [SortOrder])
    VALUES ([source].[Name], [source].[SortOrder]);
GO

INSERT INTO [dbo].[FoodCategory] ([Name], [Description])
SELECT [v].[Name], [v].[Description]
FROM (VALUES
    ('Petit-dejeuner', 'Repas du matin'),
    ('Dejeuner', 'Repas du midi'),
    ('Diner', 'Repas du soir'),
    ('Salade', 'Salades froides ou chaudes'),
    ('Plat principal', 'Plats principaux'),
    ('Soupe', 'Soupes et bouillons'),
    ('Dessert', 'Preparations sucrees'),
    ('Boisson', 'Jus, smoothies, cocktails et boissons chaudes'),
    ('Collation', 'Petite faim entre les repas')
) AS [v]([Name], [Description])
WHERE NOT EXISTS (
    SELECT 1
    FROM [dbo].[FoodCategory] [fc]
    WHERE [fc].[Name] = [v].[Name]
);
GO

INSERT INTO [dbo].[Cuisine] ([Name])
SELECT [v].[Name]
FROM (VALUES
    ('Internationale'),
    ('Italienne'),
    ('Francaise'),
    ('Marocaine'),
    ('Mexicaine'),
    ('Indienne'),
    ('Japonaise')
) AS [v]([Name])
WHERE NOT EXISTS (
    SELECT 1
    FROM [dbo].[Cuisine] [c]
    WHERE [c].[Name] = [v].[Name]
);
GO

INSERT INTO [dbo].[PresetAvatarImage] ([Code], [Label], [ImageUrl], [BackgroundColor], [SortOrder])
SELECT [v].[Code], [v].[Label], [v].[ImageUrl], [v].[BackgroundColor], [v].[SortOrder]
FROM (VALUES
    ('avatar-chef-rouge', 'Chef Rouge', 'https://example.com/avatars/avatar-chef-rouge.png', '#FDE2E4', 1),
    ('avatar-chef-bleu', 'Chef Bleu', 'https://example.com/avatars/avatar-chef-bleu.png', '#DCEBFF', 2),
    ('avatar-chef-vert', 'Chef Vert', 'https://example.com/avatars/avatar-chef-vert.png', '#DDF3E4', 3),
    ('avatar-chef-jaune', 'Chef Jaune', 'https://example.com/avatars/avatar-chef-jaune.png', '#FFF0C9', 4),
    ('avatar-chef-rose', 'Chef Rose', 'https://example.com/avatars/avatar-chef-rose.png', '#F9D9E8', 5),
    ('avatar-chef-noir', 'Chef Noir', 'https://example.com/avatars/avatar-chef-noir.png', '#E3E3E3', 6)
) AS [v]([Code], [Label], [ImageUrl], [BackgroundColor], [SortOrder])
WHERE NOT EXISTS (
    SELECT 1
    FROM [dbo].[PresetAvatarImage] [pai]
    WHERE [pai].[Code] = [v].[Code]
);
GO

INSERT INTO [dbo].[MoroccanRegion] ([Code], [Name])
SELECT [v].[Code], [v].[Name]
FROM (VALUES
    ('TTA', 'Tanger-Tetouan-Al Hoceima'),
    ('ORI', 'Oriental'),
    ('FM', 'Fes-Meknes'),
    ('RSK', 'Rabat-Sale-Kenitra'),
    ('BMK', 'Beni Mellal-Khenifra'),
    ('CS', 'Casablanca-Settat'),
    ('MS', 'Marrakech-Safi'),
    ('DT', 'Draa-Tafilalet'),
    ('SM', 'Souss-Massa'),
    ('GON', 'Guelmim-Oued Noun'),
    ('LSH', 'Laayoune-Sakia El Hamra'),
    ('DOE', 'Dakhla-Oued Ed-Dahab')
) AS [v]([Code], [Name])
WHERE NOT EXISTS (
    SELECT 1
    FROM [dbo].[MoroccanRegion] [mr]
    WHERE [mr].[Code] = [v].[Code]
);
GO

INSERT INTO [dbo].[MoroccanCity] ([RegionId], [Name], [Slug])
SELECT [mr].[Id], [v].[Name], [v].[Slug]
FROM (VALUES
    ('TTA', 'Tanger', 'tanger'),
    ('TTA', 'Tetouan', 'tetouan'),
    ('TTA', 'Al Hoceima', 'al-hoceima'),
    ('TTA', 'Larache', 'larache'),
    ('TTA', 'Ksar El Kebir', 'ksar-el-kebir'),
    ('TTA', 'Chefchaouen', 'chefchaouen'),
    ('TTA', 'Asilah', 'asilah'),
    ('TTA', 'Martil', 'martil'),
    ('ORI', 'Oujda', 'oujda'),
    ('ORI', 'Nador', 'nador'),
    ('ORI', 'Berkane', 'berkane'),
    ('ORI', 'Taourirt', 'taourirt'),
    ('ORI', 'Jerada', 'jerada'),
    ('ORI', 'Saidia', 'saidia'),
    ('FM', 'Fes', 'fes'),
    ('FM', 'Meknes', 'meknes'),
    ('FM', 'Taza', 'taza'),
    ('FM', 'Sefrou', 'sefrou'),
    ('FM', 'Ifrane', 'ifrane'),
    ('FM', 'Azrou', 'azrou'),
    ('RSK', 'Rabat', 'rabat'),
    ('RSK', 'Sale', 'sale'),
    ('RSK', 'Kenitra', 'kenitra'),
    ('RSK', 'Temara', 'temara'),
    ('RSK', 'Skhirat', 'skhirat'),
    ('RSK', 'Tiflet', 'tiflet'),
    ('RSK', 'Sidi Kacem', 'sidi-kacem'),
    ('BMK', 'Beni Mellal', 'beni-mellal'),
    ('BMK', 'Khenifra', 'khenifra'),
    ('BMK', 'Khouribga', 'khouribga'),
    ('BMK', 'Fkih Ben Salah', 'fkih-ben-salah'),
    ('BMK', 'Azilal', 'azilal'),
    ('BMK', 'Oued Zem', 'oued-zem'),
    ('CS', 'Casablanca', 'casablanca'),
    ('CS', 'Mohammedia', 'mohammedia'),
    ('CS', 'Settat', 'settat'),
    ('CS', 'El Jadida', 'el-jadida'),
    ('CS', 'Berrechid', 'berrechid'),
    ('CS', 'Benslimane', 'benslimane'),
    ('CS', 'Dar Bouazza', 'dar-bouazza'),
    ('MS', 'Marrakech', 'marrakech'),
    ('MS', 'Safi', 'safi'),
    ('MS', 'Essaouira', 'essaouira'),
    ('MS', 'Ben Guerir', 'ben-guerir'),
    ('MS', 'Youssoufia', 'youssoufia'),
    ('MS', 'Chichaoua', 'chichaoua'),
    ('DT', 'Ouarzazate', 'ouarzazate'),
    ('DT', 'Errachidia', 'errachidia'),
    ('DT', 'Midelt', 'midelt'),
    ('DT', 'Tinghir', 'tinghir'),
    ('DT', 'Zagora', 'zagora'),
    ('DT', 'Rissani', 'rissani'),
    ('SM', 'Agadir', 'agadir'),
    ('SM', 'Inezgane', 'inezgane'),
    ('SM', 'Ait Melloul', 'ait-melloul'),
    ('SM', 'Taroudant', 'taroudant'),
    ('SM', 'Tiznit', 'tiznit'),
    ('SM', 'Tata', 'tata'),
    ('GON', 'Guelmim', 'guelmim'),
    ('GON', 'Tan-Tan', 'tan-tan'),
    ('GON', 'Sidi Ifni', 'sidi-ifni'),
    ('GON', 'Assa', 'assa'),
    ('LSH', 'Laayoune', 'laayoune'),
    ('LSH', 'Boujdour', 'boujdour'),
    ('LSH', 'Tarfaya', 'tarfaya'),
    ('LSH', 'Es-Smara', 'es-smara'),
    ('DOE', 'Dakhla', 'dakhla'),
    ('DOE', 'Aousserd', 'aousserd'),
    ('DOE', 'Bir Gandouz', 'bir-gandouz')
) AS [v]([RegionCode], [Name], [Slug])
INNER JOIN [dbo].[MoroccanRegion] [mr] ON [mr].[Code] = [v].[RegionCode]
WHERE NOT EXISTS (
    SELECT 1
    FROM [dbo].[MoroccanCity] [mc]
    WHERE [mc].[Slug] = [v].[Slug]
);
GO

INSERT INTO [dbo].[Unit] ([Code], [Label])
SELECT [v].[Code], [v].[Label]
FROM (VALUES
    ('g', 'Gramme'),
    ('kg', 'Kilogramme'),
    ('ml', 'Millilitre'),
    ('l', 'Litre'),
    ('cc', 'Cuillere a cafe'),
    ('cs', 'Cuillere a soupe'),
    ('tasse', 'Tasse'),
    ('piece', 'Piece')
) AS [v]([Code], [Label])
WHERE NOT EXISTS (
    SELECT 1
    FROM [dbo].[Unit] [u]
    WHERE [u].[Code] = [v].[Code]
);
GO

INSERT INTO [dbo].[IngredientType] ([Code], [Label])
SELECT [v].[Code], [v].[Label]
FROM (VALUES
    ('PRINCIPAL', 'Ingredient principal'),
    ('ASSAISONNEMENT', 'Assaisonnement'),
    ('SAUCE', 'Sauce'),
    ('GARNITURE', 'Garniture')
) AS [v]([Code], [Label])
WHERE NOT EXISTS (
    SELECT 1
    FROM [dbo].[IngredientType] [it]
    WHERE [it].[Code] = [v].[Code]
);
GO

INSERT INTO [dbo].[AppUser] ([UserName], [FirstName], [LastName], [Email], [PasswordHash], [PasswordSalt], [ProfileImageUrl], [PresetAvatarImageId])
SELECT [v].[UserName], [v].[FirstName], [v].[LastName], [v].[Email], [v].[PasswordHash], [v].[PasswordSalt], [v].[ProfileImageUrl], [pai].[Id]
FROM (VALUES
    ('chef_samia', 'Samia', 'Bennani', 'samia@example.com', 0xD668DB854A70F8BD3DAF405020793D4473DEDE6F73A800847B706B556FBDB2BE, 0x62649150219B91828157808840623BAD, NULL, 'avatar-chef-rose'),
    ('chef_LOL', 'LOL', 'Alaoui', 'LOL@example.com', 0x2CBB346C16B552B4E1F1FD8625BB69513BAA4624179A4555F00C8A80C1737039, 0xD30ECBF67A0A320A094852F7749AA8FB, NULL, 'avatar-chef-bleu'),
    ('nora_gourmande', 'Nora', 'Karim', 'nora@example.com', 0x8FD9DE2B79768097B5FD85D79E835372F0B41675DB0711E9D5F0C7623755C154, 0x013A8D07CC91A531097A7491991F922C, NULL, 'avatar-chef-jaune'),
    ('adam_cuisine', 'Adam', 'Lahlou', 'adam@example.com', 0x310571D0A48835F7FB88041DE5EA30E3CA9EF5300C64F13FD0B87542A02FC2CD, 0x592A7D6B2BF28F9C5CF88A6F7592DEF4, NULL, 'avatar-chef-vert'),
    ('sara_saveurs', 'Sara', 'El Idrissi', 'sara@example.com', 0xDC36DD400A8463B5EBD7630614FC0F77F4C5D70A2C25FE6EA759E25D36CC015F, 0x947E9956DBFF3EEB694461B3BBE84152, NULL, 'avatar-chef-rouge')
) AS [v]([UserName], [FirstName], [LastName], [Email], [PasswordHash], [PasswordSalt], [ProfileImageUrl], [PresetAvatarCode])
LEFT JOIN [dbo].[PresetAvatarImage] [pai] ON [pai].[Code] = [v].[PresetAvatarCode]
WHERE NOT EXISTS (
    SELECT 1
    FROM [dbo].[AppUser] [u]
    WHERE [u].[UserName] = [v].[UserName]
       OR [u].[Email] = [v].[Email]
);
GO

UPDATE [u]
SET [u].[PresetAvatarImageId] = [pai].[Id]
FROM [dbo].[AppUser] [u]
INNER JOIN (VALUES
    ('chef_samia', 'avatar-chef-rose'),
    ('chef_LOL', 'avatar-chef-bleu'),
    ('nora_gourmande', 'avatar-chef-jaune'),
    ('adam_cuisine', 'avatar-chef-vert'),
    ('sara_saveurs', 'avatar-chef-rouge')
) AS [v]([UserName], [PresetAvatarCode]) ON [u].[UserName] = [v].[UserName]
INNER JOIN [dbo].[PresetAvatarImage] [pai] ON [pai].[Code] = [v].[PresetAvatarCode]
WHERE [u].[PresetAvatarImageId] IS NULL;
GO

INSERT INTO [dbo].[ChefProfile] ([UserId], [DisplayName], [Bio], [Specialty], [YearsOfExperience], [IsVerified])
SELECT [u].[Id], [v].[DisplayName], [v].[Bio], [v].[Specialty], [v].[YearsOfExperience], [v].[IsVerified]
FROM (VALUES
    ('chef_samia', 'Chef Samia', 'Cheffe maison orientee repas frais et equilibres.', 'Salades et boissons', 6, 1),
    ('chef_LOL', 'Chef LOL', 'Createur de brunchs et de repas rapides.', 'Petit-dejeuner', 8, 1),
    ('sara_saveurs', 'Chef Sara', 'Passionnee de desserts simples et modernes.', 'Desserts et collations', 5, 0)
) AS [v]([UserName], [DisplayName], [Bio], [Specialty], [YearsOfExperience], [IsVerified])
INNER JOIN [dbo].[AppUser] [u] ON [u].[UserName] = [v].[UserName]
WHERE NOT EXISTS (
    SELECT 1
    FROM [dbo].[ChefProfile] [cp]
    WHERE [cp].[UserId] = [u].[Id]
);
GO

INSERT INTO [dbo].[Ingredient] ([Name], [IngredientTypeId], [DefaultUnitId], [CaloriesPer100g])
SELECT [v].[Name], [it].[Id], [u].[Id], [v].[CaloriesPer100g]
FROM (VALUES
    ('Tomate', 'PRINCIPAL', 'g', 18.00),
    ('Concombre', 'PRINCIPAL', 'g', 15.00),
    ('Laitue', 'PRINCIPAL', 'g', 15.00),
    ('Fromage feta', 'PRINCIPAL', 'g', 264.00),
    ('Huile d olive', 'SAUCE', 'ml', 884.00),
    ('Jus de citron', 'SAUCE', 'ml', 22.00),
    ('Sel', 'ASSAISONNEMENT', 'cc', 0.00),
    ('Poivre noir', 'ASSAISONNEMENT', 'cc', 251.00),
    ('Avocat', 'PRINCIPAL', 'piece', 160.00),
    ('Pain', 'PRINCIPAL', 'piece', 265.00),
    ('Oeuf', 'PRINCIPAL', 'piece', 155.00),
    ('Lait', 'PRINCIPAL', 'ml', 42.00),
    ('Banane', 'PRINCIPAL', 'piece', 89.00),
    ('Mangue', 'PRINCIPAL', 'piece', 60.00),
    ('Yaourt', 'PRINCIPAL', 'g', 59.00),
    ('Miel', 'SAUCE', 'cs', 304.00),
    ('Menthe', 'GARNITURE', 'g', 44.00),
    ('Flocons d avoine', 'PRINCIPAL', 'g', 389.00),
    ('Farine', 'PRINCIPAL', 'g', 364.00),
    ('Sucre', 'ASSAISONNEMENT', 'g', 387.00),
    ('Beurre', 'PRINCIPAL', 'g', 717.00),
    ('Levure chimique', 'ASSAISONNEMENT', 'g', 53.00),
    ('Fraises', 'PRINCIPAL', 'g', 32.00)
) AS [v]([Name], [IngredientTypeCode], [UnitCode], [CaloriesPer100g])
INNER JOIN [dbo].[IngredientType] [it] ON [it].[Code] = [v].[IngredientTypeCode]
LEFT JOIN [dbo].[Unit] [u] ON [u].[Code] = [v].[UnitCode]
WHERE NOT EXISTS (
    SELECT 1
    FROM [dbo].[Ingredient] [i]
    WHERE [i].[Name] = [v].[Name]
);
GO

INSERT INTO [dbo].[IngredientImage] ([IngredientId], [ImageUrl], [AltText], [IsPrimary], [SortOrder])
SELECT [i].[Id], [v].[ImageUrl], [v].[AltText], [v].[IsPrimary], [v].[SortOrder]
FROM (VALUES
    ('Tomate', 'https://example.com/images/ingredients/tomate.jpg', 'Tomate fraiche', 1, 1),
    ('Concombre', 'https://example.com/images/ingredients/concombre.jpg', 'Concombre frais', 1, 1),
    ('Laitue', 'https://example.com/images/ingredients/laitue.jpg', 'Laitue verte', 1, 1),
    ('Fromage feta', 'https://example.com/images/ingredients/feta.jpg', 'Fromage feta emiette', 1, 1),
    ('Huile d olive', 'https://example.com/images/ingredients/huile-olive.jpg', 'Huile d olive', 1, 1),
    ('Jus de citron', 'https://example.com/images/ingredients/jus-citron.jpg', 'Jus de citron frais', 1, 1),
    ('Sel', 'https://example.com/images/ingredients/sel.jpg', 'Sel fin', 1, 1),
    ('Poivre noir', 'https://example.com/images/ingredients/poivre-noir.jpg', 'Poivre noir moulu', 1, 1),
    ('Avocat', 'https://example.com/images/ingredients/avocat.jpg', 'Avocat mur', 1, 1),
    ('Pain', 'https://example.com/images/ingredients/pain.jpg', 'Tranches de pain', 1, 1),
    ('Oeuf', 'https://example.com/images/ingredients/oeuf.jpg', 'Oeuf frais', 1, 1),
    ('Lait', 'https://example.com/images/ingredients/lait.jpg', 'Verre de lait', 1, 1),
    ('Banane', 'https://example.com/images/ingredients/banane.jpg', 'Banane jaune', 1, 1),
    ('Mangue', 'https://example.com/images/ingredients/mangue.jpg', 'Mangue coupee', 1, 1),
    ('Yaourt', 'https://example.com/images/ingredients/yaourt.jpg', 'Yaourt nature', 1, 1),
    ('Miel', 'https://example.com/images/ingredients/miel.jpg', 'Miel liquide', 1, 1),
    ('Menthe', 'https://example.com/images/ingredients/menthe.jpg', 'Feuilles de menthe', 1, 1),
    ('Flocons d avoine', 'https://example.com/images/ingredients/flocons-avoine.jpg', 'Flocons d avoine', 1, 1),
    ('Farine', 'https://example.com/images/ingredients/farine.jpg', 'Farine blanche', 1, 1),
    ('Sucre', 'https://example.com/images/ingredients/sucre.jpg', 'Sucre en poudre', 1, 1),
    ('Beurre', 'https://example.com/images/ingredients/beurre.jpg', 'Beurre doux', 1, 1),
    ('Levure chimique', 'https://example.com/images/ingredients/levure.jpg', 'Levure chimique', 1, 1),
    ('Fraises', 'https://example.com/images/ingredients/fraises.jpg', 'Fraises fraiches', 1, 1)
) AS [v]([IngredientName], [ImageUrl], [AltText], [IsPrimary], [SortOrder])
INNER JOIN [dbo].[Ingredient] [i] ON [i].[Name] = [v].[IngredientName]
WHERE NOT EXISTS (
    SELECT 1
    FROM [dbo].[IngredientImage] [ii]
    WHERE [ii].[IngredientId] = [i].[Id]
      AND [ii].[ImageUrl] = [v].[ImageUrl]
);
GO

INSERT INTO [dbo].[Recipe] (
    [ChefId],
    [DifficultyId],
    [CuisineId],
    [CityId],
    [Title],
    [Slug],
    [Summary],
    [PrepTimeMinutes],
    [CookTimeMinutes],
    [Servings],
    [CoverImageUrl],
    [Status],
    [Visibility],
    [PublishedAt]
)
SELECT
    [cp].[Id],
    [d].[Id],
    [c].[Id],
    [mc].[Id],
    [v].[Title],
    [v].[Slug],
    [v].[Summary],
    [v].[PrepTimeMinutes],
    [v].[CookTimeMinutes],
    [v].[Servings],
    [v].[CoverImageUrl],
    [v].[Status],
    [v].[Visibility],
    GETDATE()
FROM (VALUES
    ('Chef Samia', 'Facile', 'Internationale', 'Casablanca', 'Salade grecque fraiche', 'salade-grecque-fraiche', 'Salade fraiche avec legumes croquants, feta et sauce citronnee.', 15, 0, 2, 'https://example.com/images/recipes/salade-grecque-1.jpg', 'PUBLISH', 'Publique'),
    ('Chef LOL', 'Facile', 'Internationale', 'Rabat', 'Tartine avocat oeuf', 'tartine-avocat-oeuf', 'Petit-dejeuner rapide avec avocat cremeux et oeufs.', 10, 8, 2, 'https://example.com/images/recipes/tartine-avocat-1.jpg', 'PUBLISH', 'Publique'),
    ('Chef Samia', 'Facile', 'Internationale', 'Agadir', 'Smoothie mangue banane', 'smoothie-mangue-banane', 'Smoothie froid pour le matin ou la collation.', 10, 0, 2, 'https://example.com/images/recipes/smoothie-mangue-1.jpg', 'PUBLISH', 'Publique'),
    ('Chef Sara', 'Moyen', 'Francaise', 'Tetouan', 'Pancakes aux fraises', 'pancakes-aux-fraises', 'Pancakes moelleux servis avec fraises fraiches.', 15, 10, 4, 'https://example.com/images/recipes/pancakes-fraises-1.jpg', 'PUBLISH', 'Publique')
) AS [v](
    [ChefDisplayName],
    [DifficultyName],
    [CuisineName],
    [CityName],
    [Title],
    [Slug],
    [Summary],
    [PrepTimeMinutes],
    [CookTimeMinutes],
    [Servings],
    [CoverImageUrl],
    [Status],
    [Visibility]
)
INNER JOIN [dbo].[ChefProfile] [cp] ON [cp].[DisplayName] = [v].[ChefDisplayName]
INNER JOIN [dbo].[Difficulty] [d] ON [d].[Name] = [v].[DifficultyName]
LEFT JOIN [dbo].[Cuisine] [c] ON [c].[Name] = [v].[CuisineName]
LEFT JOIN [dbo].[MoroccanCity] [mc] ON [mc].[Name] = [v].[CityName]
WHERE NOT EXISTS (
    SELECT 1
    FROM [dbo].[Recipe] [r]
    WHERE [r].[Slug] = [v].[Slug]
);
GO

UPDATE [r]
SET
    [r].[CityId] = ISNULL([r].[CityId], [mc].[Id]),
    [r].[CoverImageUrl] = ISNULL([r].[CoverImageUrl], [v].[CoverImageUrl])
FROM [dbo].[Recipe] [r]
INNER JOIN (VALUES
    ('salade-grecque-fraiche', 'Casablanca', 'https://example.com/images/recipes/salade-grecque-1.jpg'),
    ('tartine-avocat-oeuf', 'Rabat', 'https://example.com/images/recipes/tartine-avocat-1.jpg'),
    ('smoothie-mangue-banane', 'Agadir', 'https://example.com/images/recipes/smoothie-mangue-1.jpg'),
    ('pancakes-aux-fraises', 'Tetouan', 'https://example.com/images/recipes/pancakes-fraises-1.jpg')
) AS [v]([Slug], [CityName], [CoverImageUrl]) ON [r].[Slug] = [v].[Slug]
LEFT JOIN [dbo].[MoroccanCity] [mc] ON [mc].[Name] = [v].[CityName]
WHERE [r].[CityId] IS NULL
   OR [r].[CoverImageUrl] IS NULL;
GO

INSERT INTO [dbo].[RecipeImage] ([RecipeId], [ImageUrl], [AltText], [IsPrimary], [SortOrder])
SELECT [r].[Id], [v].[ImageUrl], [v].[AltText], [v].[IsPrimary], [v].[SortOrder]
FROM (VALUES
    ('salade-grecque-fraiche', 'https://example.com/images/recipes/salade-grecque-1.jpg', 'Vue principale de la salade grecque fraiche', 1, 1),
    ('salade-grecque-fraiche', 'https://example.com/images/recipes/salade-grecque-2.jpg', 'Detail de la feta et des legumes', 0, 2),
    ('tartine-avocat-oeuf', 'https://example.com/images/recipes/tartine-avocat-1.jpg', 'Vue principale de la tartine avocat oeuf', 1, 1),
    ('tartine-avocat-oeuf', 'https://example.com/images/recipes/tartine-avocat-2.jpg', 'Detail des tartines et oeufs', 0, 2),
    ('smoothie-mangue-banane', 'https://example.com/images/recipes/smoothie-mangue-1.jpg', 'Verre principal du smoothie mangue banane', 1, 1),
    ('smoothie-mangue-banane', 'https://example.com/images/recipes/smoothie-mangue-2.jpg', 'Smoothie avec menthe et fruits', 0, 2),
    ('pancakes-aux-fraises', 'https://example.com/images/recipes/pancakes-fraises-1.jpg', 'Pile de pancakes aux fraises', 1, 1),
    ('pancakes-aux-fraises', 'https://example.com/images/recipes/pancakes-fraises-2.jpg', 'Pancakes servis avec fraises fraiches', 0, 2)
) AS [v]([RecipeSlug], [ImageUrl], [AltText], [IsPrimary], [SortOrder])
INNER JOIN [dbo].[Recipe] [r] ON [r].[Slug] = [v].[RecipeSlug]
WHERE NOT EXISTS (
    SELECT 1
    FROM [dbo].[RecipeImage] [ri]
    WHERE [ri].[RecipeId] = [r].[Id]
      AND [ri].[ImageUrl] = [v].[ImageUrl]
);
GO

INSERT INTO [dbo].[RecipeCategory] ([RecipeId], [FoodCategoryId])
SELECT [r].[Id], [fc].[Id]
FROM (VALUES
    ('salade-grecque-fraiche', 'Dejeuner'),
    ('salade-grecque-fraiche', 'Salade'),
    ('tartine-avocat-oeuf', 'Petit-dejeuner'),
    ('tartine-avocat-oeuf', 'Collation'),
    ('smoothie-mangue-banane', 'Petit-dejeuner'),
    ('smoothie-mangue-banane', 'Boisson'),
    ('pancakes-aux-fraises', 'Petit-dejeuner'),
    ('pancakes-aux-fraises', 'Dessert')
) AS [v]([RecipeSlug], [CategoryName])
INNER JOIN [dbo].[Recipe] [r] ON [r].[Slug] = [v].[RecipeSlug]
INNER JOIN [dbo].[FoodCategory] [fc] ON [fc].[Name] = [v].[CategoryName]
WHERE NOT EXISTS (
    SELECT 1
    FROM [dbo].[RecipeCategory] [rc]
    WHERE [rc].[RecipeId] = [r].[Id]
      AND [rc].[FoodCategoryId] = [fc].[Id]
);
GO

INSERT INTO [dbo].[RecipeIngredient] ([RecipeId], [IngredientId], [Quantity], [UnitId], [IsOptional], [Notes], [SortOrder])
SELECT
    [r].[Id],
    [i].[Id],
    [v].[Quantity],
    [u].[Id],
    [v].[IsOptional],
    [v].[Notes],
    [v].[SortOrder]
FROM (VALUES
    ('salade-grecque-fraiche', 'Tomate', 200.00, 'g', 0, 'Coupee en des', 1),
    ('salade-grecque-fraiche', 'Concombre', 150.00, 'g', 0, 'Coupe en rondelles', 2),
    ('salade-grecque-fraiche', 'Laitue', 100.00, 'g', 0, 'Emincee', 3),
    ('salade-grecque-fraiche', 'Fromage feta', 80.00, 'g', 0, 'Emiettee', 4),
    ('salade-grecque-fraiche', 'Huile d olive', 30.00, 'ml', 0, 'Pour la sauce', 5),
    ('salade-grecque-fraiche', 'Jus de citron', 15.00, 'ml', 0, 'Presse minute', 6),
    ('salade-grecque-fraiche', 'Sel', 1.00, 'cc', 0, NULL, 7),
    ('salade-grecque-fraiche', 'Poivre noir', 0.50, 'cc', 0, NULL, 8),
    ('tartine-avocat-oeuf', 'Avocat', 1.00, 'piece', 0, 'Bien mur', 1),
    ('tartine-avocat-oeuf', 'Pain', 2.00, 'piece', 0, 'Tranches de pain', 2),
    ('tartine-avocat-oeuf', 'Oeuf', 2.00, 'piece', 0, 'Bouilli ou poele', 3),
    ('tartine-avocat-oeuf', 'Huile d olive', 10.00, 'ml', 1, 'Filet facultatif', 4),
    ('tartine-avocat-oeuf', 'Sel', 0.50, 'cc', 0, NULL, 5),
    ('tartine-avocat-oeuf', 'Poivre noir', 0.25, 'cc', 0, NULL, 6),
    ('smoothie-mangue-banane', 'Mangue', 1.00, 'piece', 0, 'Epluchee et coupee', 1),
    ('smoothie-mangue-banane', 'Banane', 1.00, 'piece', 0, 'Coupee en rondelles', 2),
    ('smoothie-mangue-banane', 'Lait', 250.00, 'ml', 0, NULL, 3),
    ('smoothie-mangue-banane', 'Yaourt', 120.00, 'g', 0, 'Nature', 4),
    ('smoothie-mangue-banane', 'Miel', 1.00, 'cs', 1, 'Pour sucrer davantage', 5),
    ('smoothie-mangue-banane', 'Menthe', 5.00, 'g', 1, 'Pour decorer', 6),
    ('smoothie-mangue-banane', 'Flocons d avoine', 20.00, 'g', 1, 'Pour une texture plus epaisse', 7),
    ('pancakes-aux-fraises', 'Farine', 200.00, 'g', 0, NULL, 1),
    ('pancakes-aux-fraises', 'Lait', 250.00, 'ml', 0, NULL, 2),
    ('pancakes-aux-fraises', 'Oeuf', 2.00, 'piece', 0, NULL, 3),
    ('pancakes-aux-fraises', 'Sucre', 30.00, 'g', 0, NULL, 4),
    ('pancakes-aux-fraises', 'Beurre', 25.00, 'g', 0, 'Fondu', 5),
    ('pancakes-aux-fraises', 'Levure chimique', 10.00, 'g', 0, NULL, 6),
    ('pancakes-aux-fraises', 'Fraises', 120.00, 'g', 1, 'Pour le service', 7)
) AS [v]([RecipeSlug], [IngredientName], [Quantity], [UnitCode], [IsOptional], [Notes], [SortOrder])
INNER JOIN [dbo].[Recipe] [r] ON [r].[Slug] = [v].[RecipeSlug]
INNER JOIN [dbo].[Ingredient] [i] ON [i].[Name] = [v].[IngredientName]
LEFT JOIN [dbo].[Unit] [u] ON [u].[Code] = [v].[UnitCode]
WHERE NOT EXISTS (
    SELECT 1
    FROM [dbo].[RecipeIngredient] [ri]
    WHERE [ri].[RecipeId] = [r].[Id]
      AND [ri].[IngredientId] = [i].[Id]
);
GO

INSERT INTO [dbo].[RecipeNutrition] ([RecipeId], [CaloriesPerServing], [ProteinGrams], [CarbsGrams], [FatGrams])
SELECT [r].[Id], [v].[CaloriesPerServing], [v].[ProteinGrams], [v].[CarbsGrams], [v].[FatGrams]
FROM (VALUES
    ('salade-grecque-fraiche', 220.00, 7.00, 12.00, 16.00),
    ('tartine-avocat-oeuf', 310.00, 13.00, 24.00, 18.00),
    ('smoothie-mangue-banane', 205.00, 6.00, 34.00, 5.00),
    ('pancakes-aux-fraises', 340.00, 9.00, 46.00, 12.00)
) AS [v]([RecipeSlug], [CaloriesPerServing], [ProteinGrams], [CarbsGrams], [FatGrams])
INNER JOIN [dbo].[Recipe] [r] ON [r].[Slug] = [v].[RecipeSlug]
WHERE NOT EXISTS (
    SELECT 1
    FROM [dbo].[RecipeNutrition] [rn]
    WHERE [rn].[RecipeId] = [r].[Id]
);
GO

INSERT INTO [dbo].[RecipeStep] ([RecipeId], [StepNumber], [Title], [Instruction], [DurationMinutes])
SELECT [r].[Id], [v].[StepNumber], [v].[Title], [v].[Instruction], [v].[DurationMinutes]
FROM (VALUES
    ('salade-grecque-fraiche', 1, 'Preparer les legumes', 'Laver la tomate, le concombre et la laitue. Couper la tomate en des, le concombre en rondelles et emincer la laitue.', 5),
    ('salade-grecque-fraiche', 2, 'Preparer la sauce', 'Dans un petit bol, melanger l huile d olive, le jus de citron, le sel et le poivre noir.', 2),
    ('salade-grecque-fraiche', 3, 'Assembler la salade', 'Disposer les legumes dans un saladier, ajouter la feta emiettee puis verser la sauce.', 3),
    ('salade-grecque-fraiche', 4, 'Servir', 'Melanger delicatement et servir immediatement.', 1),
    ('tartine-avocat-oeuf', 1, 'Griller le pain', 'Faire griller les tranches de pain jusqu a obtenir une belle couleur doree.', 3),
    ('tartine-avocat-oeuf', 2, 'Ecraser l avocat', 'Ecraser l avocat avec le sel et le poivre noir jusqu a obtention d une texture cremeuse.', 2),
    ('tartine-avocat-oeuf', 3, 'Cuire les oeufs', 'Cuire les oeufs selon votre preference, a l eau ou a la poele.', 5),
    ('tartine-avocat-oeuf', 4, 'Monter les tartines', 'Etaler l avocat sur le pain, ajouter les oeufs puis un filet d huile d olive si desire.', 2),
    ('smoothie-mangue-banane', 1, 'Preparer les fruits', 'Eplucher la mangue et la banane puis les couper en morceaux.', 3),
    ('smoothie-mangue-banane', 2, 'Mixer', 'Mettre la mangue, la banane, le lait, le yaourt et le miel dans un blender puis mixer.', 3),
    ('smoothie-mangue-banane', 3, 'Ajuster la texture', 'Ajouter les flocons d avoine si vous voulez un smoothie plus epais puis mixer de nouveau.', 1),
    ('smoothie-mangue-banane', 4, 'Servir frais', 'Verser dans des verres et decorer avec la menthe avant de servir.', 1),
    ('pancakes-aux-fraises', 1, 'Melanger les ingredients secs', 'Dans un saladier, melanger la farine, le sucre et la levure chimique.', 3),
    ('pancakes-aux-fraises', 2, 'Ajouter les liquides', 'Ajouter le lait, les oeufs et le beurre fondu puis fouetter jusqu a obtenir une pate lisse.', 4),
    ('pancakes-aux-fraises', 3, 'Cuire les pancakes', 'Verser une petite louche de pate dans une poele chaude et cuire chaque face jusqu a coloration doree.', 8),
    ('pancakes-aux-fraises', 4, 'Dresser', 'Servir les pancakes avec les fraises fraiches sur le dessus.', 2)
) AS [v]([RecipeSlug], [StepNumber], [Title], [Instruction], [DurationMinutes])
INNER JOIN [dbo].[Recipe] [r] ON [r].[Slug] = [v].[RecipeSlug]
WHERE NOT EXISTS (
    SELECT 1
    FROM [dbo].[RecipeStep] [rs]
    WHERE [rs].[RecipeId] = [r].[Id]
      AND [rs].[StepNumber] = [v].[StepNumber]
);
GO

INSERT INTO [dbo].[RecipeLike] ([RecipeId], [UserId])
SELECT [r].[Id], [u].[Id]
FROM (VALUES
    ('salade-grecque-fraiche', 'nora_gourmande'),
    ('tartine-avocat-oeuf', 'chef_samia'),
    ('smoothie-mangue-banane', 'nora_gourmande'),
    ('pancakes-aux-fraises', 'adam_cuisine'),
    ('pancakes-aux-fraises', 'nora_gourmande')
) AS [v]([RecipeSlug], [UserName])
INNER JOIN [dbo].[Recipe] [r] ON [r].[Slug] = [v].[RecipeSlug]
INNER JOIN [dbo].[AppUser] [u] ON [u].[UserName] = [v].[UserName]
WHERE NOT EXISTS (
    SELECT 1
    FROM [dbo].[RecipeLike] [rl]
    WHERE [rl].[RecipeId] = [r].[Id]
      AND [rl].[UserId] = [u].[Id]
);
GO

INSERT INTO [dbo].[RecipeShare] ([RecipeId], [SharedByUserId], [SharedWithUserId], [ShareChannel], [ShareMessage])
SELECT
    [r].[Id],
    [sharedBy].[Id],
    [sharedWith].[Id],
    [v].[ShareChannel],
    [v].[ShareMessage]
FROM (VALUES
    ('salade-grecque-fraiche', 'chef_samia', 'nora_gourmande', 'Application', 'Teste cette salade fraiche pour le dejeuner.'),
    ('smoothie-mangue-banane', 'nora_gourmande', NULL, 'Lien', 'Je partage mon smoothie prefere.'),
    ('pancakes-aux-fraises', 'sara_saveurs', 'adam_cuisine', 'Application', 'Recette ideale pour un brunch du week-end.')
) AS [v]([RecipeSlug], [SharedByUserName], [SharedWithUserName], [ShareChannel], [ShareMessage])
INNER JOIN [dbo].[Recipe] [r] ON [r].[Slug] = [v].[RecipeSlug]
INNER JOIN [dbo].[AppUser] [sharedBy] ON [sharedBy].[UserName] = [v].[SharedByUserName]
LEFT JOIN [dbo].[AppUser] [sharedWith] ON [sharedWith].[UserName] = [v].[SharedWithUserName]
WHERE NOT EXISTS (
    SELECT 1
    FROM [dbo].[RecipeShare] [rs]
    WHERE [rs].[RecipeId] = [r].[Id]
      AND [rs].[SharedByUserId] = [sharedBy].[Id]
      AND ISNULL([rs].[SharedWithUserId], 0) = ISNULL([sharedWith].[Id], 0)
      AND [rs].[ShareChannel] = [v].[ShareChannel]
      AND ISNULL([rs].[ShareMessage], '') = ISNULL([v].[ShareMessage], '')
);
GO
