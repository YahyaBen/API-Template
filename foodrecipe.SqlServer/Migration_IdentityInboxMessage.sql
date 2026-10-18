-- Apply before enabling IdentityEvents:Enabled in Foodiya.API.
-- The inbox event and the AppUser change are committed in one transaction.
IF OBJECT_ID(N'dbo.IdentityInboxMessage', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IdentityInboxMessage
    (
        EventId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_IdentityInboxMessage PRIMARY KEY,
        EventType VARCHAR(100) NOT NULL,
        ProcessedAtUtc DATETIME2 NOT NULL
    );
END;
