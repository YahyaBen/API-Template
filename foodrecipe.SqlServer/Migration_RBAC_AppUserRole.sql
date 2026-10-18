-- ============================================================================
-- Migration: Role-Based Access Control (RBAC)
-- Date:      2026-04-05
-- Purpose:   Add Role column to AppUser table with default 'USER'.
--            Seed existing chef users with 'CHEF' role.
--            Idempotent — safe to re-run.
-- ============================================================================

-- ─────────────────────────────────────────────────────────────────────────────
-- STEP 1 — Add Role column (defaults to 'USER')
-- ─────────────────────────────────────────────────────────────────────────────

IF COL_LENGTH(N'[dbo].[AppUser]', N'Role') IS NULL
BEGIN
    ALTER TABLE [dbo].[AppUser]
    ADD [Role] [varchar](20) NOT NULL
        CONSTRAINT [DF_AppUser_Role] DEFAULT ('USER');
END;
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- STEP 2 — Promote existing users who have a ChefProfile to 'CHEF'
-- ─────────────────────────────────────────────────────────────────────────────

UPDATE [u]
SET [u].[Role] = 'CHEF'
FROM [dbo].[AppUser] [u]
INNER JOIN [dbo].[ChefProfile] [cp] ON [cp].[UserId] = [u].[Id]
WHERE [u].[Role] = 'USER'
  AND [cp].[DeletedAt] IS NULL;
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- STEP 3 — Create the first SuperAdmin (chef_samia for dev/demo purposes)
-- ─────────────────────────────────────────────────────────────────────────────

UPDATE [dbo].[AppUser]
SET [Role] = 'SUPER_ADMIN'
WHERE [UserName] = 'chef_samia'
  AND [Role] <> 'SUPER_ADMIN';
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- STEP 4 — Index for fast role-based queries
-- ─────────────────────────────────────────────────────────────────────────────

IF NOT EXISTS (
    SELECT 1
    FROM [sys].[indexes]
    WHERE [name] = 'IX_AppUser_Role'
      AND [object_id] = OBJECT_ID(N'[dbo].[AppUser]')
)
BEGIN
    CREATE INDEX [IX_AppUser_Role] ON [dbo].[AppUser]([Role]);
END;
GO
