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
        ProfileImagePath NVARCHAR(500)     NULL,
        CreatedAt       DATETIME2          NOT NULL,
        UpdatedAt       DATETIME2          NOT NULL,
        CONSTRAINT UQ_Users_Email UNIQUE (Email)
    );
END
GO

-- Profile picture path (upgrade of an existing Users table; no-op on a fresh database). Existing users get NULL.
IF COL_LENGTH('Users', 'ProfileImagePath') IS NULL
BEGIN
    ALTER TABLE Users ADD ProfileImagePath NVARCHAR(500) NULL;
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

-- ============================================================================
-- Phase 4 - Learning module (idempotent; no sample content is seeded).
-- Creation order respects foreign keys: LearningChapters -> Lessons -> LearningProgress.
-- LearningChapters.DisplayOrder is the learner-visible Chapter number.
-- Lessons.DisplayOrder is the integer position inside its Chapter (shown as X.Y).
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'LearningChapters')
BEGIN
    CREATE TABLE LearningChapters
    (
        ChapterId       INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Title           NVARCHAR(150)      NOT NULL,
        Description     NVARCHAR(1000)     NOT NULL,
        DisplayOrder    INT                NOT NULL,
        IsActive        BIT                NOT NULL CONSTRAINT DF_LearningChapters_IsActive DEFAULT 1,
        CreatedAt       DATETIME2          NOT NULL,
        UpdatedAt       DATETIME2          NOT NULL,
        CONSTRAINT UQ_LearningChapters_DisplayOrder UNIQUE (DisplayOrder)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Lessons')
BEGIN
    CREATE TABLE Lessons
    (
        LessonId        INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ChapterId       INT                NOT NULL,
        Title           NVARCHAR(200)      NOT NULL,
        Content         NVARCHAR(MAX)      NOT NULL,
        ImagePath       NVARCHAR(500)      NULL,
        DisplayOrder    INT                NOT NULL,
        IsActive        BIT                NOT NULL CONSTRAINT DF_Lessons_IsActive DEFAULT 1,
        CreatedAt       DATETIME2          NOT NULL,
        UpdatedAt       DATETIME2          NOT NULL,
        CONSTRAINT FK_Lessons_LearningChapters FOREIGN KEY (ChapterId) REFERENCES LearningChapters (ChapterId),
        CONSTRAINT UQ_Lessons_Chapter_DisplayOrder UNIQUE (ChapterId, DisplayOrder)
    );
END
GO

-- Lesson image position (Above/Below content). Idempotent upgrade for databases created before
-- this column existed; the DEFAULT constraint backfills existing rows as 'Above', so an existing
-- Lesson's image keeps appearing where it already did.
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Lessons') AND name = 'ImagePosition')
BEGIN
    ALTER TABLE Lessons ADD ImagePosition NVARCHAR(20) NOT NULL CONSTRAINT DF_Lessons_ImagePosition DEFAULT 'Above';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'LearningProgress')
BEGIN
    CREATE TABLE LearningProgress
    (
        LearningProgressId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserId          INT                NOT NULL,
        LessonId        INT                NOT NULL,
        CompletedAt     DATETIME2          NOT NULL,
        CONSTRAINT FK_LearningProgress_Users FOREIGN KEY (UserId) REFERENCES Users (UserId),
        CONSTRAINT FK_LearningProgress_Lessons FOREIGN KEY (LessonId) REFERENCES Lessons (LessonId),
        CONSTRAINT UQ_LearningProgress_User_Lesson UNIQUE (UserId, LessonId)
    );
END
GO

-- ====================================================================
-- Hidden Road Sign Game (real database-backed module)
-- Idempotent: every statement is guarded, nothing is dropped, altered or seeded.
-- Creation order respects foreign keys:
--   Assets -> Practices (Sets) -> Items -> Choices -> Attempts -> AttemptAnswers
-- HiddenRoadSignAttempts / HiddenRoadSignAttemptAnswers hold COMPLETED attempts only;
-- a learner attempt is written in one transaction when the whole Game is finished.
-- ====================================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'HiddenRoadSignAssets')
BEGIN
    CREATE TABLE HiddenRoadSignAssets
    (
        RoadSignAssetId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Name            NVARCHAR(150)      NOT NULL,
        ImagePath       NVARCHAR(500)      NOT NULL,
        IsActive        BIT                NOT NULL CONSTRAINT DF_HiddenRoadSignAssets_IsActive DEFAULT 1,
        CreatedAt       DATETIME2          NOT NULL,
        UpdatedAt       DATETIME2          NOT NULL
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'HiddenRoadSignPractices')
BEGIN
    CREATE TABLE HiddenRoadSignPractices
    (
        HiddenRoadSignPracticeId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        SetNumber       INT                NOT NULL,
        SceneImagePath  NVARCHAR(500)      NOT NULL,
        IsActive        BIT                NOT NULL CONSTRAINT DF_HiddenRoadSignPractices_IsActive DEFAULT 1,
        CreatedAt       DATETIME2          NOT NULL,
        UpdatedAt       DATETIME2          NOT NULL,
        CONSTRAINT UQ_HiddenRoadSignPractices_SetNumber UNIQUE (SetNumber),
        CONSTRAINT CK_HiddenRoadSignPractices_SetNumber CHECK (SetNumber > 0)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'HiddenRoadSignItems')
BEGIN
    CREATE TABLE HiddenRoadSignItems
    (
        HiddenRoadSignItemId     INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        HiddenRoadSignPracticeId INT                NOT NULL,
        RoadSignAssetId          INT                NOT NULL,
        QuestionText             NVARCHAR(1000)     NULL,      -- NULL while the Road Sign is an incomplete Draft
        Explanation              NVARCHAR(MAX)      NULL,
        HotspotXPercent          DECIMAL(5,2)       NOT NULL,
        HotspotYPercent          DECIMAL(5,2)       NOT NULL,
        HotspotWidthPercent      DECIMAL(5,2)       NOT NULL,
        HotspotHeightPercent     DECIMAL(5,2)       NOT NULL,
        IsConfigured             BIT                NOT NULL CONSTRAINT DF_HiddenRoadSignItems_IsConfigured DEFAULT 0,
        CreatedAt                DATETIME2          NOT NULL,
        UpdatedAt                DATETIME2          NOT NULL,
        CONSTRAINT FK_HiddenRoadSignItems_Practices FOREIGN KEY (HiddenRoadSignPracticeId) REFERENCES HiddenRoadSignPractices (HiddenRoadSignPracticeId),
        CONSTRAINT FK_HiddenRoadSignItems_Assets FOREIGN KEY (RoadSignAssetId) REFERENCES HiddenRoadSignAssets (RoadSignAssetId),
        CONSTRAINT CK_HiddenRoadSignItems_Configured CHECK (IsConfigured = 0 OR (QuestionText IS NOT NULL AND LEN(LTRIM(RTRIM(QuestionText))) > 0)),
        CONSTRAINT CK_HiddenRoadSignItems_Hotspot CHECK
        (
            HotspotXPercent >= 0 AND HotspotYPercent >= 0
            AND HotspotWidthPercent > 0 AND HotspotHeightPercent > 0
            AND HotspotXPercent + HotspotWidthPercent <= 100
            AND HotspotYPercent + HotspotHeightPercent <= 100
        )
    );
END
GO

-- In-place upgrade of an EXISTING HiddenRoadSignItems table (no-ops on a fresh database).
-- Mirrors DatabaseInitializer.UpgradeHiddenRoadSignItems. Never recreates the table or deletes rows.
-- 1. IsConfigured: complete existing Road Signs become 1, malformed ones 0, new rows default to 0 (Draft).
IF COL_LENGTH('HiddenRoadSignItems', 'IsConfigured') IS NULL
BEGIN
    ALTER TABLE HiddenRoadSignItems ADD IsConfigured BIT NULL;

    EXEC('UPDATE i SET IsConfigured = CASE WHEN LEN(LTRIM(RTRIM(i.QuestionText))) > 0
        AND (SELECT COUNT(*) FROM HiddenRoadSignChoices c WHERE c.HiddenRoadSignItemId = i.HiddenRoadSignItemId) = 3
        AND (SELECT COUNT(*) FROM HiddenRoadSignChoices c WHERE c.HiddenRoadSignItemId = i.HiddenRoadSignItemId AND c.IsCorrect = 1) = 1
        AND NOT EXISTS (SELECT 1 FROM HiddenRoadSignChoices c WHERE c.HiddenRoadSignItemId = i.HiddenRoadSignItemId AND LEN(LTRIM(RTRIM(c.ChoiceText))) = 0)
        THEN 1 ELSE 0 END
        FROM HiddenRoadSignItems i');

    EXEC('ALTER TABLE HiddenRoadSignItems ALTER COLUMN IsConfigured BIT NOT NULL');
    EXEC('ALTER TABLE HiddenRoadSignItems ADD CONSTRAINT DF_HiddenRoadSignItems_IsConfigured DEFAULT 0 FOR IsConfigured');
END
GO

-- 2. QuestionText becomes nullable (a Draft has no question yet).
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('HiddenRoadSignItems') AND name = 'QuestionText' AND is_nullable = 0)
BEGIN
    ALTER TABLE HiddenRoadSignItems ALTER COLUMN QuestionText NVARCHAR(1000) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_HiddenRoadSignItems_Configured' AND parent_object_id = OBJECT_ID('HiddenRoadSignItems'))
BEGIN
    EXEC('ALTER TABLE HiddenRoadSignItems ADD CONSTRAINT CK_HiddenRoadSignItems_Configured
          CHECK (IsConfigured = 0 OR (QuestionText IS NOT NULL AND LEN(LTRIM(RTRIM(QuestionText))) > 0))');
END
GO

-- 3. Drop HiddenRoadSignItems.DisplayOrder and everything that involves it
--    (UQ_HiddenRoadSignItems_Practice_DisplayOrder, CK_HiddenRoadSignItems_DisplayOrder, ...).
--    HiddenRoadSignChoices.DisplayOrder (A/B/C) is a different column and is not touched.
IF COL_LENGTH('HiddenRoadSignItems', 'DisplayOrder') IS NOT NULL
BEGIN
    DECLARE @tbl INT = OBJECT_ID('HiddenRoadSignItems');
    DECLARE @col INT = COLUMNPROPERTY(@tbl, 'DisplayOrder', 'ColumnId');
    DECLARE @sql NVARCHAR(MAX);

    WHILE 1 = 1
    BEGIN
        SET @sql = NULL;
        SELECT TOP 1 @sql = CASE WHEN i.is_unique_constraint = 1
                                 THEN N'ALTER TABLE HiddenRoadSignItems DROP CONSTRAINT ' + QUOTENAME(i.name)
                                 ELSE N'DROP INDEX ' + QUOTENAME(i.name) + N' ON HiddenRoadSignItems' END
        FROM sys.indexes i
        INNER JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
        WHERE i.object_id = @tbl AND ic.column_id = @col AND i.is_primary_key = 0;
        IF @sql IS NULL BREAK;
        EXEC(@sql);
    END

    WHILE 1 = 1
    BEGIN
        SET @sql = NULL;
        SELECT TOP 1 @sql = N'ALTER TABLE HiddenRoadSignItems DROP CONSTRAINT ' + QUOTENAME(name)
        FROM sys.check_constraints
        WHERE parent_object_id = @tbl AND (parent_column_id = @col OR definition LIKE N'%[[]DisplayOrder]%');
        IF @sql IS NULL BREAK;
        EXEC(@sql);
    END

    WHILE 1 = 1
    BEGIN
        SET @sql = NULL;
        SELECT TOP 1 @sql = N'ALTER TABLE HiddenRoadSignItems DROP CONSTRAINT ' + QUOTENAME(name)
        FROM sys.default_constraints
        WHERE parent_object_id = @tbl AND parent_column_id = @col;
        IF @sql IS NULL BREAK;
        EXEC(@sql);
    END

    ALTER TABLE HiddenRoadSignItems DROP COLUMN DisplayOrder;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'HiddenRoadSignChoices')
BEGIN
    CREATE TABLE HiddenRoadSignChoices
    (
        HiddenRoadSignChoiceId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        HiddenRoadSignItemId   INT                NOT NULL,
        ChoiceText             NVARCHAR(500)      NOT NULL,
        DisplayOrder           INT                NOT NULL,
        IsCorrect              BIT                NOT NULL,
        CONSTRAINT FK_HiddenRoadSignChoices_Items FOREIGN KEY (HiddenRoadSignItemId) REFERENCES HiddenRoadSignItems (HiddenRoadSignItemId),
        CONSTRAINT UQ_HiddenRoadSignChoices_Item_DisplayOrder UNIQUE (HiddenRoadSignItemId, DisplayOrder),
        CONSTRAINT CK_HiddenRoadSignChoices_DisplayOrder CHECK (DisplayOrder BETWEEN 1 AND 3)
    );
END
GO

-- Filtered indexes require QUOTED_IDENTIFIER ON (the ADO.NET default; sqlcmd needs it explicit).
SET QUOTED_IDENTIFIER ON;
GO

-- At most one correct choice per item. 'Exactly three choices' and 'exactly one correct'
-- are enforced by application validation inside the save transaction.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_HiddenRoadSignChoices_OneCorrect' AND object_id = OBJECT_ID('HiddenRoadSignChoices'))
BEGIN
    CREATE UNIQUE INDEX UX_HiddenRoadSignChoices_OneCorrect
        ON HiddenRoadSignChoices (HiddenRoadSignItemId) WHERE IsCorrect = 1;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'HiddenRoadSignAttempts')
BEGIN
    CREATE TABLE HiddenRoadSignAttempts
    (
        HiddenRoadSignAttemptId  INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserId                   INT                NOT NULL,
        HiddenRoadSignPracticeId INT                NOT NULL,
        StartedAt                DATETIME2          NOT NULL,
        CompletedAt              DATETIME2          NOT NULL,
        CorrectCount             INT                NOT NULL,
        TotalItems               INT                NOT NULL,
        CONSTRAINT FK_HiddenRoadSignAttempts_Users FOREIGN KEY (UserId) REFERENCES Users (UserId),
        CONSTRAINT FK_HiddenRoadSignAttempts_Practices FOREIGN KEY (HiddenRoadSignPracticeId) REFERENCES HiddenRoadSignPractices (HiddenRoadSignPracticeId),
        CONSTRAINT CK_HiddenRoadSignAttempts_Counts CHECK (TotalItems > 0 AND CorrectCount >= 0 AND CorrectCount <= TotalItems)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'HiddenRoadSignAttemptAnswers')
BEGIN
    CREATE TABLE HiddenRoadSignAttemptAnswers
    (
        HiddenRoadSignAttemptAnswerId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        HiddenRoadSignAttemptId       INT                NOT NULL,
        HiddenRoadSignItemId          INT                NOT NULL,
        SelectedChoiceId              INT                NOT NULL,
        IsCorrect                     BIT                NOT NULL,
        FoundOrder                    INT                NOT NULL,
        CONSTRAINT FK_HiddenRoadSignAttemptAnswers_Attempts FOREIGN KEY (HiddenRoadSignAttemptId) REFERENCES HiddenRoadSignAttempts (HiddenRoadSignAttemptId),
        CONSTRAINT FK_HiddenRoadSignAttemptAnswers_Items FOREIGN KEY (HiddenRoadSignItemId) REFERENCES HiddenRoadSignItems (HiddenRoadSignItemId),
        CONSTRAINT FK_HiddenRoadSignAttemptAnswers_Choices FOREIGN KEY (SelectedChoiceId) REFERENCES HiddenRoadSignChoices (HiddenRoadSignChoiceId),
        CONSTRAINT UQ_HiddenRoadSignAttemptAnswers_Attempt_Item UNIQUE (HiddenRoadSignAttemptId, HiddenRoadSignItemId),
        CONSTRAINT CK_HiddenRoadSignAttemptAnswers_FoundOrder CHECK (FoundOrder > 0)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_HiddenRoadSignAttempts_User' AND object_id = OBJECT_ID('HiddenRoadSignAttempts'))
BEGIN
    CREATE INDEX IX_HiddenRoadSignAttempts_User ON HiddenRoadSignAttempts (UserId, HiddenRoadSignPracticeId);
END
GO
