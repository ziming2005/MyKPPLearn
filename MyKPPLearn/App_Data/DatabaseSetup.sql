-- ============================================================================
-- MyKPP Learn - DatabaseSetup.sql
--
-- Reference copy of the schema/seed created automatically by
-- Data/DatabaseInitializer.cs on application start. Provided for manual
-- setup, documentation, and academic review. Running the application is
-- sufficient; running this script by hand is optional.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users
    (
        UserId          INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        FullName        NVARCHAR(150)      NOT NULL,
        Email           NVARCHAR(254)      NOT NULL,
        PasswordHash    NVARCHAR(256)      NOT NULL,
        Role            NVARCHAR(20)       NOT NULL,
        CreatedAt       DATETIME2          NOT NULL,
        UpdatedAt       DATETIME2          NOT NULL,
        CONSTRAINT UQ_Users_Email UNIQUE (Email)
    );
END
GO

-- Development Administrator seed (idempotent).
-- PasswordHash below is the PBKDF2-SHA256 encoding of the project owner's
-- development Administrator password ( PBKDF2-SHA256$<iterations>$<saltBase64>$<hashBase64> ).
-- The plaintext password itself is never stored anywhere in the application or
-- in this file.
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'admin@gmail.com')
BEGIN
    INSERT INTO Users (FullName, Email, PasswordHash, Role, CreatedAt, UpdatedAt)
    VALUES (
        'Administrator',
        'admin@gmail.com',
        'PBKDF2-SHA256$100000$L5CrWM0iDmfTRb6PmhilbA==$2bc4As/yNJ8uTqtFJmmIMUMoNLL7Z9szzlc3k+ZzFos=',
        'Administrator',
        SYSUTCDATETIME(),
        SYSUTCDATETIME()
    );
END
GO
