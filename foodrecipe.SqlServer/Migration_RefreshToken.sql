-- ============================================================
-- Migration: Refresh Tokens
-- Adds AppUserRefreshToken table for JWT refresh token rotation.
-- Idempotent — safe to re-run.
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AppUserRefreshToken')
BEGIN
    CREATE TABLE dbo.AppUserRefreshToken
    (
        Id              INT IDENTITY(1,1) NOT NULL,
        AppUserId       INT               NOT NULL,
        TokenHash       VARCHAR(128)      NOT NULL,   -- SHA-256 hex of the opaque token
        ExpiresAtUtc    DATETIME2(7)      NOT NULL,
        CreatedAtUtc    DATETIME2(7)      NOT NULL DEFAULT(SYSUTCDATETIME()),
        RevokedAtUtc    DATETIME2(7)      NULL,
        ReplacedByTokenHash VARCHAR(128)  NULL,       -- points to the rotated successor

        CONSTRAINT PK_AppUserRefreshToken PRIMARY KEY (Id),
        CONSTRAINT FK_AppUserRefreshToken_AppUser
            FOREIGN KEY (AppUserId) REFERENCES dbo.AppUser(Id)
    );

    CREATE INDEX IX_AppUserRefreshToken_TokenHash
        ON dbo.AppUserRefreshToken (TokenHash);

    CREATE INDEX IX_AppUserRefreshToken_AppUserId
        ON dbo.AppUserRefreshToken (AppUserId);

    PRINT 'Created AppUserRefreshToken table.';
END
ELSE
    PRINT 'AppUserRefreshToken table already exists — skipping.';
GO
