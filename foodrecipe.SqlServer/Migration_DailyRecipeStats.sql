-- ============================================================================
-- Migration: DailyRecipeStats — Historical Daily Analytics
-- Date:      2026-04-07
-- Purpose:   Create a DailyRecipeStats table to store daily platform-wide
--            recipe analytics produced by the DailyRecipeStatsFunction.
--            Scalar metrics are stored as columns; top-lists and breakdowns
--            are stored as NVARCHAR(MAX) JSON blobs.
-- ============================================================================

-- ─────────────────────────────────────────────────────────────────────────────
-- STEP 1 — Create the DailyRecipeStats table
-- ─────────────────────────────────────────────────────────────────────────────

IF OBJECT_ID(N'[dbo].[DailyRecipeStats]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[DailyRecipeStats]
    (
        [Id]                    INT             IDENTITY(1,1)   NOT NULL,
        [StatsDate]             DATE            NOT NULL,
        [GeneratedAtUtc]        DATETIME2(0)    NOT NULL,

        -- Scalar counters
        [TotalRecipes]          INT             NOT NULL,
        [TotalPublishedRecipes] INT             NOT NULL,
        [NewRecipesToday]       INT             NOT NULL,
        [TotalLikesToday]       INT             NOT NULL,

        -- JSON snapshots
        [TopLikedRecipesJson]   NVARCHAR(MAX)   NULL,
        [RecentRecipesJson]     NVARCHAR(MAX)   NULL,
        [CuisineBreakdownJson]  NVARCHAR(MAX)   NULL,
        [DifficultyBreakdownJson] NVARCHAR(MAX) NULL,

        CONSTRAINT [PK_DailyRecipeStats]            PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [UQ_DailyRecipeStats_StatsDate]  UNIQUE ([StatsDate])
    );
END;
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- STEP 2 — Add default for GeneratedAtUtc
-- ─────────────────────────────────────────────────────────────────────────────

IF NOT EXISTS (
    SELECT 1 FROM sys.default_constraints
    WHERE [name] = N'DF_DailyRecipeStats_GeneratedAtUtc'
)
BEGIN
    ALTER TABLE [dbo].[DailyRecipeStats]
    ADD CONSTRAINT [DF_DailyRecipeStats_GeneratedAtUtc] DEFAULT (SYSUTCDATETIME()) FOR [GeneratedAtUtc];
END;
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- STEP 3 — Index on StatsDate for fast date-range queries
-- ─────────────────────────────────────────────────────────────────────────────

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE [name] = N'IX_DailyRecipeStats_StatsDate' AND object_id = OBJECT_ID(N'[dbo].[DailyRecipeStats]')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_DailyRecipeStats_StatsDate]
        ON [dbo].[DailyRecipeStats] ([StatsDate] DESC);
END;
GO

PRINT '✅ Migration_DailyRecipeStats applied successfully.';
GO
