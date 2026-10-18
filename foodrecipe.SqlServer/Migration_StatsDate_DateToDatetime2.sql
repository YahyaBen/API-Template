-- ============================================================
-- Migration: Change DailyRecipeStats.StatsDate from DATE to DATETIME2
-- Purpose  : Store ISO 8601 timestamps (e.g. 2026-04-03T19:16:00.0000000)
-- Date     : 2026-04-08
-- ============================================================

-- STEP 1 — Drop the existing index on StatsDate
IF EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE [name] = N'IX_DailyRecipeStats_StatsDate'
      AND object_id = OBJECT_ID(N'[dbo].[DailyRecipeStats]')
)
BEGIN
    DROP INDEX [IX_DailyRecipeStats_StatsDate] ON [dbo].[DailyRecipeStats];
    PRINT '✅ Dropped index IX_DailyRecipeStats_StatsDate';
END;

-- STEP 2 — Drop the UNIQUE constraint on StatsDate
IF EXISTS (
    SELECT 1
    FROM sys.key_constraints
    WHERE [name] = N'UQ_DailyRecipeStats_StatsDate'
      AND parent_object_id = OBJECT_ID(N'[dbo].[DailyRecipeStats]')
)
BEGIN
    ALTER TABLE [dbo].[DailyRecipeStats]
        DROP CONSTRAINT [UQ_DailyRecipeStats_StatsDate];
    PRINT '✅ Dropped constraint UQ_DailyRecipeStats_StatsDate';
END;

-- STEP 3 — Alter column from DATE to DATETIME2
ALTER TABLE [dbo].[DailyRecipeStats]
    ALTER COLUMN [StatsDate] DATETIME2 NOT NULL;
PRINT '✅ Changed StatsDate from DATE to DATETIME2';

-- STEP 4 — Recreate the UNIQUE constraint
ALTER TABLE [dbo].[DailyRecipeStats]
    ADD CONSTRAINT [UQ_DailyRecipeStats_StatsDate] UNIQUE ([StatsDate]);
PRINT '✅ Recreated constraint UQ_DailyRecipeStats_StatsDate';

-- STEP 5 — Recreate the index
CREATE NONCLUSTERED INDEX [IX_DailyRecipeStats_StatsDate]
    ON [dbo].[DailyRecipeStats] ([StatsDate] DESC);
PRINT '✅ Recreated index IX_DailyRecipeStats_StatsDate';
