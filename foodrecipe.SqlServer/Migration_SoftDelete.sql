-- ============================================================================
-- Migration: Soft Delete Support
-- Date:      2026-04-04
-- Purpose:   Add DeletedAt, DeletedByUserId, DeleteReason to core business
--            tables (Recipe, AppUser, ChefProfile).
--            Idempotent — safe to re-run.
-- ============================================================================

-- ─────────────────────────────────────────────────────────────────────────────
-- Recipe
-- ─────────────────────────────────────────────────────────────────────────────

IF COL_LENGTH(N'[dbo].[Recipe]', N'DeletedAt') IS NULL
    ALTER TABLE [dbo].[Recipe] ADD [DeletedAt] [smalldatetime] NULL;
GO

IF COL_LENGTH(N'[dbo].[Recipe]', N'DeletedByUserId') IS NULL
    ALTER TABLE [dbo].[Recipe] ADD [DeletedByUserId] [int] NULL;
GO

IF COL_LENGTH(N'[dbo].[Recipe]', N'DeleteReason') IS NULL
    ALTER TABLE [dbo].[Recipe] ADD [DeleteReason] [varchar](500) NOT NULL
        CONSTRAINT [DF_Recipe_DeleteReason] DEFAULT ('');
GO

IF NOT EXISTS (SELECT 1 FROM [sys].[foreign_keys] WHERE [name] = 'FK_Recipe_DeletedByUser' AND [parent_object_id] = OBJECT_ID(N'[dbo].[Recipe]'))
BEGIN
    ALTER TABLE [dbo].[Recipe]
    WITH CHECK ADD CONSTRAINT [FK_Recipe_DeletedByUser]
    FOREIGN KEY ([DeletedByUserId]) REFERENCES [dbo].[AppUser]([Id]);
END;
GO

IF NOT EXISTS (SELECT 1 FROM [sys].[indexes] WHERE [name] = 'IX_Recipe_DeletedAt' AND [object_id] = OBJECT_ID(N'[dbo].[Recipe]'))
BEGIN
    CREATE INDEX [IX_Recipe_DeletedAt] ON [dbo].[Recipe]([DeletedAt]);
END;
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- AppUser
-- ─────────────────────────────────────────────────────────────────────────────

IF COL_LENGTH(N'[dbo].[AppUser]', N'DeletedAt') IS NULL
    ALTER TABLE [dbo].[AppUser] ADD [DeletedAt] [smalldatetime] NULL;
GO

IF COL_LENGTH(N'[dbo].[AppUser]', N'DeletedByUserId') IS NULL
    ALTER TABLE [dbo].[AppUser] ADD [DeletedByUserId] [int] NULL;
GO

IF COL_LENGTH(N'[dbo].[AppUser]', N'DeleteReason') IS NULL
    ALTER TABLE [dbo].[AppUser] ADD [DeleteReason] [varchar](500) NOT NULL
        CONSTRAINT [DF_AppUser_DeleteReason] DEFAULT ('');
GO

IF NOT EXISTS (SELECT 1 FROM [sys].[foreign_keys] WHERE [name] = 'FK_AppUser_DeletedByUser' AND [parent_object_id] = OBJECT_ID(N'[dbo].[AppUser]'))
BEGIN
    ALTER TABLE [dbo].[AppUser]
    WITH CHECK ADD CONSTRAINT [FK_AppUser_DeletedByUser]
    FOREIGN KEY ([DeletedByUserId]) REFERENCES [dbo].[AppUser]([Id]);
END;
GO

IF NOT EXISTS (SELECT 1 FROM [sys].[indexes] WHERE [name] = 'IX_AppUser_DeletedAt' AND [object_id] = OBJECT_ID(N'[dbo].[AppUser]'))
BEGIN
    CREATE INDEX [IX_AppUser_DeletedAt] ON [dbo].[AppUser]([DeletedAt]);
END;
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- ChefProfile
-- ─────────────────────────────────────────────────────────────────────────────

IF COL_LENGTH(N'[dbo].[ChefProfile]', N'DeletedAt') IS NULL
    ALTER TABLE [dbo].[ChefProfile] ADD [DeletedAt] [smalldatetime] NULL;
GO

IF COL_LENGTH(N'[dbo].[ChefProfile]', N'DeletedByUserId') IS NULL
    ALTER TABLE [dbo].[ChefProfile] ADD [DeletedByUserId] [int] NULL;
GO

IF COL_LENGTH(N'[dbo].[ChefProfile]', N'DeleteReason') IS NULL
    ALTER TABLE [dbo].[ChefProfile] ADD [DeleteReason] [varchar](500) NOT NULL
        CONSTRAINT [DF_ChefProfile_DeleteReason] DEFAULT ('');
GO

IF NOT EXISTS (SELECT 1 FROM [sys].[foreign_keys] WHERE [name] = 'FK_ChefProfile_DeletedByUser' AND [parent_object_id] = OBJECT_ID(N'[dbo].[ChefProfile]'))
BEGIN
    ALTER TABLE [dbo].[ChefProfile]
    WITH CHECK ADD CONSTRAINT [FK_ChefProfile_DeletedByUser]
    FOREIGN KEY ([DeletedByUserId]) REFERENCES [dbo].[AppUser]([Id]);
END;
GO

IF NOT EXISTS (SELECT 1 FROM [sys].[indexes] WHERE [name] = 'IX_ChefProfile_DeletedAt' AND [object_id] = OBJECT_ID(N'[dbo].[ChefProfile]'))
BEGIN
    CREATE INDEX [IX_ChefProfile_DeletedAt] ON [dbo].[ChefProfile]([DeletedAt]);
END;
GO

PRINT 'Soft delete migration completed successfully.';
GO
