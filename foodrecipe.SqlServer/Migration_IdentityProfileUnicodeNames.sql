-- Preserve Identity's Unicode given/family names in the business profile.
SET XACT_ABORT ON;
IF OBJECT_ID(N'dbo.AppUser', N'U') IS NULL
    THROW 50001, 'Apply the Foodiya database schema before expanding profile names.', 1;

BEGIN TRY
    BEGIN TRANSACTION;
    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = N'dbo'
        AND TABLE_NAME = N'AppUser' AND COLUMN_NAME = N'FirstName' AND DATA_TYPE = N'varchar')
        ALTER TABLE dbo.AppUser ALTER COLUMN FirstName nvarchar(100) NOT NULL;
    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = N'dbo'
        AND TABLE_NAME = N'AppUser' AND COLUMN_NAME = N'LastName' AND DATA_TYPE = N'varchar')
        ALTER TABLE dbo.AppUser ALTER COLUMN LastName nvarchar(100) NOT NULL;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
