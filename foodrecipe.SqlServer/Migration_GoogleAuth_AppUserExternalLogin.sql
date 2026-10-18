IF OBJECT_ID(N'dbo.AppUserExternalLogin', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AppUserExternalLogin
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AppUserExternalLogin PRIMARY KEY,
        AppUserId INT NOT NULL,
        Provider VARCHAR(50) NOT NULL,
        ProviderSubject VARCHAR(200) NOT NULL,
        DateInsert SMALLDATETIME NOT NULL CONSTRAINT DF_AppUserExternalLogin_DateInsert DEFAULT (GETDATE()),
        LastSignInAt SMALLDATETIME NOT NULL CONSTRAINT DF_AppUserExternalLogin_LastSignInAt DEFAULT (GETDATE()),
        CONSTRAINT FK_AppUserExternalLogin_AppUser
            FOREIGN KEY (AppUserId) REFERENCES dbo.AppUser (Id)
    );

    CREATE INDEX IX_AppUserExternalLogin_AppUserId
        ON dbo.AppUserExternalLogin (AppUserId);

    CREATE UNIQUE INDEX UQ_AppUserExternalLogin_Provider_Subject
        ON dbo.AppUserExternalLogin (Provider, ProviderSubject);

    CREATE UNIQUE INDEX UQ_AppUserExternalLogin_AppUser_Provider
        ON dbo.AppUserExternalLogin (AppUserId, Provider);
END;
