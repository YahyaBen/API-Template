-- Expand the business profile email contract to match ASP.NET Core Identity.
-- Apply before enabling Identity event consumption or the new API authentication.
SET XACT_ABORT ON;

IF OBJECT_ID(N'dbo.AppUser', N'U') IS NULL
    THROW 50001, 'Apply the Foodiya database schema before expanding profile emails.', 1;

IF EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = N'dbo' AND TABLE_NAME = N'AppUser' AND COLUMN_NAME = N'Email'
        AND (CHARACTER_MAXIMUM_LENGTH BETWEEN 1 AND 255
            OR CHARACTER_MAXIMUM_LENGTH = 256 AND DATA_TYPE = N'varchar')
)
BEGIN
    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @isConstraint bit = CASE WHEN EXISTS (
            SELECT 1 FROM sys.key_constraints
            WHERE parent_object_id = OBJECT_ID(N'dbo.AppUser') AND name = N'UQ_AppUser_Email'
        ) THEN 1 ELSE 0 END;

        IF @isConstraint = 1
            ALTER TABLE dbo.AppUser DROP CONSTRAINT UQ_AppUser_Email;
        ELSE IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.AppUser') AND name = N'UQ_AppUser_Email')
            DROP INDEX UQ_AppUser_Email ON dbo.AppUser;

        ALTER TABLE dbo.AppUser ALTER COLUMN Email nvarchar(256) NOT NULL;

        IF @isConstraint = 1
            ALTER TABLE dbo.AppUser ADD CONSTRAINT UQ_AppUser_Email UNIQUE NONCLUSTERED (Email);
        ELSE
            CREATE UNIQUE NONCLUSTERED INDEX UQ_AppUser_Email ON dbo.AppUser (Email);
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
