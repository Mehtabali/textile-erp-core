/*=====================================================
  USER_REFRESH_TOKENS
  Minimal Refresh Token Table
  Supports:
  - Refresh token lookup
  - Logout current device
  - Logout all devices
  - Active sessions
  Re-runnable Script
=====================================================*/

IF OBJECT_ID('dbo.USER_REFRESH_TOKENS', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.USER_REFRESH_TOKENS
    (
        ID BIGINT IDENTITY(1,1) NOT NULL,
        USERID INT NOT NULL,
        TOKENHASH NVARCHAR(500) NOT NULL,
        EXPIRESAT DATETIME2 NOT NULL,
        CREATEDAT DATETIME2 NOT NULL
            CONSTRAINT DF_USER_REFRESH_TOKENS_CREATEDAT
            DEFAULT SYSUTCDATETIME(),
        REVOKEDAT DATETIME2 NULL,

        CONSTRAINT PK_USER_REFRESH_TOKENS
            PRIMARY KEY CLUSTERED (ID),

        CONSTRAINT FK_USER_REFRESH_TOKENS_USERS
            FOREIGN KEY (USERID)
            REFERENCES dbo.USERS(USERID)
    );
END
GO

/* Fast refresh-token lookup + no duplicate token hashes */
IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_USER_REFRESH_TOKENS_TOKENHASH'
      AND object_id = OBJECT_ID('dbo.USER_REFRESH_TOKENS')
)
BEGIN
    CREATE UNIQUE INDEX UX_USER_REFRESH_TOKENS_TOKENHASH
    ON dbo.USER_REFRESH_TOKENS (TOKENHASH);
END
GO

/* Fast logout-all-devices and active-sessions lookup */
IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_USER_REFRESH_TOKENS_USERID_REVOKEDAT_EXPIRESAT'
      AND object_id = OBJECT_ID('dbo.USER_REFRESH_TOKENS')
)
BEGIN
    CREATE INDEX IX_USER_REFRESH_TOKENS_USERID_REVOKEDAT_EXPIRESAT
    ON dbo.USER_REFRESH_TOKENS (USERID, REVOKEDAT, EXPIRESAT);
END
GO