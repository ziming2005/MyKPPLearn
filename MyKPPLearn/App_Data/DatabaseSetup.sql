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
        Status          NVARCHAR(20)       NOT NULL CONSTRAINT DF_Users_Status DEFAULT 'Active',
        CreatedAt       DATETIME2          NOT NULL,
        UpdatedAt       DATETIME2          NOT NULL,
        CONSTRAINT UQ_Users_Email UNIQUE (Email),
        CONSTRAINT CK_Users_Status CHECK (Status IN ('Active', 'Disabled'))
    );
END
GO

-- Profile picture path (upgrade of an existing Users table; no-op on a fresh database). Existing users get NULL.
IF COL_LENGTH('Users', 'ProfileImagePath') IS NULL
BEGIN
    ALTER TABLE Users ADD ProfileImagePath NVARCHAR(500) NULL;
END
GO

-- Account status (Active / Disabled). Schema only: nothing reads or enforces it yet (login is unchanged).
-- Upgrade of an existing Users table; no-op on a fresh database. Existing users receive 'Active' from the default.
-- The CHECK is added only if every existing value is valid, so existing rows are never changed.
IF COL_LENGTH('Users', 'Status') IS NULL
BEGIN
    ALTER TABLE Users ADD Status NVARCHAR(20) NOT NULL CONSTRAINT DF_Users_Status DEFAULT 'Active';
END
GO

IF COL_LENGTH('Users', 'Status') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Users_Status' AND parent_object_id = OBJECT_ID('Users'))
BEGIN
    EXEC('IF NOT EXISTS (SELECT 1 FROM Users WHERE Status NOT IN (''Active'', ''Disabled''))
              ALTER TABLE Users ADD CONSTRAINT CK_Users_Status CHECK (Status IN (''Active'', ''Disabled''))');
END
GO

-- Legacy cleanup: some development databases carry a Users.IsActive column that was never part of this schema.
-- Users.Status (Active / Disabled) is the single account-status column; login authorization and Learner Records must use
-- Status and never reference Users.IsActive. No-op when the column is absent (fresh and already-cleaned databases).
-- Transactional and guarded: it refuses (and changes nothing) if any user has IsActive = 0 or an invalid Status, or if
-- anything other than the column's own default constraint depends on it. Mirrors DatabaseInitializer.EnsureLegacyUserIsActiveColumnRemoved.
IF COL_LENGTH('Users', 'IsActive') IS NOT NULL
BEGIN
    IF COL_LENGTH('Users', 'Status') IS NULL
        THROW 50001, 'Users.IsActive was not removed: Users.Status does not exist.', 1;

    DECLARE @usersId INT = OBJECT_ID('Users');
    DECLARE @colId INT = COLUMNPROPERTY(@usersId, 'IsActive', 'ColumnId');
    DECLARE @conflicts INT;
    DECLARE @blockers INT;
    DECLARE @defaultCount INT;
    DECLARE @defaultName SYSNAME;

    -- Status is the single account-status column, so nothing may be lost: refuse if any user is switched off through
    -- IsActive, or has a Status outside Active / Disabled. (IsActive = 1 with Status = 'Disabled' loses nothing.)
    EXEC sp_executesql
        N'SELECT @n = COUNT(*) FROM Users WHERE IsActive = 0 OR Status NOT IN (''Active'', ''Disabled'')',
        N'@n INT OUTPUT', @n = @conflicts OUTPUT;

    IF @conflicts > 0
        THROW 50002, 'Users.IsActive was not removed: some users have IsActive = 0 or an invalid Status. Resolve their account status first.', 1;

    -- The column's own default constraint is the only object allowed to depend on it.
    SELECT @blockers =
          (SELECT COUNT(*) FROM sys.check_constraints WHERE parent_object_id = @usersId AND (parent_column_id = @colId OR definition LIKE N'%[[]IsActive]%'))
        + (SELECT COUNT(*) FROM sys.index_columns WHERE object_id = @usersId AND column_id = @colId)
        + (SELECT COUNT(*) FROM sys.indexes WHERE object_id = @usersId AND filter_definition LIKE N'%[[]IsActive]%')
        + (SELECT COUNT(*) FROM sys.computed_columns WHERE object_id = @usersId AND definition LIKE N'%[[]IsActive]%')
        + (SELECT COUNT(*) FROM sys.foreign_key_columns WHERE (parent_object_id = @usersId AND parent_column_id = @colId) OR (referenced_object_id = @usersId AND referenced_column_id = @colId))
        + (SELECT COUNT(*) FROM sys.triggers WHERE parent_id = @usersId)
        + (SELECT COUNT(*) FROM sys.sql_expression_dependencies WHERE referenced_id = @usersId AND referenced_minor_id = @colId);

    IF @blockers > 0
        THROW 50003, 'Users.IsActive was not removed: other database objects depend on it. Remove those dependencies first.', 1;

    -- The default constraint is looked up by its real name (never assumed).
    SELECT @defaultCount = COUNT(*), @defaultName = MAX(name)
    FROM sys.default_constraints
    WHERE parent_object_id = @usersId AND parent_column_id = @colId;

    IF @defaultCount > 1
        THROW 50004, 'Users.IsActive was not removed: the column has more than one default constraint.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF @defaultName IS NOT NULL
            EXEC(N'ALTER TABLE Users DROP CONSTRAINT ' + N'[' + @defaultName + N']');

        EXEC(N'ALTER TABLE Users DROP COLUMN IsActive');

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
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

-- ============================================================================
-- Shared Question Bank, Standard Practice and Mock Exam (SCHEMA ONLY).
-- No repositories or pages use these tables yet. Mirrors DatabaseInitializer.EnsureQuestionBankAndExamTablesExist.
-- Idempotent: every statement is guarded; nothing is dropped or altered; the only rows inserted are the four fixed
-- QuestionCategories (system configuration, not demo content). No sample questions, sets, attempts or users.
-- Creation order respects foreign keys:
--   QuestionCategories -> PracticeSets -> Questions -> QuestionOptions
--   -> PracticeAttempts -> PracticeAttemptQuestions
--   -> MockExamSets -> MockExamSetQuestions -> MockExamAttempts -> MockExamAttemptQuestions
-- There are no cascading deletes: attempt history can never be removed by deleting a question, option or set.
-- Attempt-question rows carry snapshots of the question as the learner saw it, so history stays accurate after edits.
-- ============================================================================

-- The four shared categories. The Mock Exam section label "Colour Blind Test" uses colour-recognition.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'QuestionCategories')
BEGIN
    CREATE TABLE QuestionCategories
    (
        QuestionCategoryId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Code            NVARCHAR(40)       NOT NULL,
        Title           NVARCHAR(100)      NOT NULL,
        Description     NVARCHAR(300)      NULL,
        DisplayOrder    INT                NOT NULL,
        IsActive        BIT                NOT NULL CONSTRAINT DF_QuestionCategories_IsActive DEFAULT 1,
        CreatedAt       DATETIME2          NOT NULL,
        UpdatedAt       DATETIME2          NOT NULL,
        CONSTRAINT UQ_QuestionCategories_Code UNIQUE (Code),
        CONSTRAINT UQ_QuestionCategories_DisplayOrder UNIQUE (DisplayOrder),
        CONSTRAINT CK_QuestionCategories_DisplayOrder CHECK (DisplayOrder > 0)
    );
END
GO

-- Inserts only missing categories (matched by Code); existing rows, including edited values, are never updated.
INSERT INTO QuestionCategories (Code, Title, Description, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
SELECT v.Code, v.Title, v.Description, v.DisplayOrder, 1, SYSUTCDATETIME(), SYSUTCDATETIME()
FROM (VALUES
    (N'colour-recognition', N'Colour Recognition', N'Practice identifying the meaning of key road and signal colours.', 1),
    (N'road-signs', N'Road Signs', N'Test your knowledge of warning, regulatory and information signs.', 2),
    (N'rules-of-the-road', N'Rules of the Road', N'Review common driving scenarios and correct road behaviour.', 3),
    (N'kejara-safety', N'KEJARA & Safety', N'Practice questions on the demerit points system and road safety.', 4)
) AS v (Code, Title, Description, DisplayOrder)
WHERE NOT EXISTS (SELECT 1 FROM QuestionCategories c WHERE c.Code = v.Code);
GO

-- "Practice Set N" is generated from SetNumber (no title column). Different categories may reuse a number.
-- UQ_PracticeSets_Id_Category exists only so Questions can reference (set, category) together.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PracticeSets')
BEGIN
    CREATE TABLE PracticeSets
    (
        PracticeSetId   INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        QuestionCategoryId INT             NOT NULL,
        SetNumber       INT                NOT NULL,
        IsActive        BIT                NOT NULL CONSTRAINT DF_PracticeSets_IsActive DEFAULT 1,
        CreatedAt       DATETIME2          NOT NULL,
        UpdatedAt       DATETIME2          NOT NULL,
        CONSTRAINT FK_PracticeSets_QuestionCategories FOREIGN KEY (QuestionCategoryId) REFERENCES QuestionCategories (QuestionCategoryId),
        CONSTRAINT UQ_PracticeSets_Category_SetNumber UNIQUE (QuestionCategoryId, SetNumber),
        CONSTRAINT UQ_PracticeSets_Id_Category UNIQUE (PracticeSetId, QuestionCategoryId),
        CONSTRAINT CK_PracticeSets_SetNumber CHECK (SetNumber > 0)
    );
END
GO

-- One shared Question Bank. PracticeSetId NULL = available to Mock Exams only. The composite foreign key
-- (PracticeSetId, QuestionCategoryId) makes the database reject a question whose category differs from its Practice
-- Set's category; it is not checked while PracticeSetId is NULL.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Questions')
BEGIN
    CREATE TABLE Questions
    (
        QuestionId      INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        QuestionCategoryId INT             NOT NULL,
        PracticeSetId   INT                NULL,
        QuestionText    NVARCHAR(500)      NOT NULL,
        Explanation     NVARCHAR(500)      NOT NULL,
        ImagePath       NVARCHAR(500)      NULL,
        ImageAltText    NVARCHAR(200)      NULL,
        IsActive        BIT                NOT NULL CONSTRAINT DF_Questions_IsActive DEFAULT 1,
        CreatedAt       DATETIME2          NOT NULL,
        UpdatedAt       DATETIME2          NOT NULL,
        CONSTRAINT FK_Questions_QuestionCategories FOREIGN KEY (QuestionCategoryId) REFERENCES QuestionCategories (QuestionCategoryId),
        CONSTRAINT FK_Questions_PracticeSets FOREIGN KEY (PracticeSetId, QuestionCategoryId) REFERENCES PracticeSets (PracticeSetId, QuestionCategoryId),
        CONSTRAINT CK_Questions_Text CHECK (LEN(LTRIM(RTRIM(QuestionText))) > 0),
        CONSTRAINT CK_Questions_Explanation CHECK (LEN(LTRIM(RTRIM(Explanation))) > 0),
        CONSTRAINT CK_Questions_ImageAlt CHECK (ImagePath IS NULL OR (ImageAltText IS NOT NULL AND LEN(LTRIM(RTRIM(ImageAltText))) > 0))
    );
END
GO

-- Final design: every question has exactly A, B and C (1-3). The database prevents duplicate positions and a second
-- correct option; 'exactly three rows' and 'exactly one correct' are validated by the future Question Bank save.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'QuestionOptions')
BEGIN
    CREATE TABLE QuestionOptions
    (
        QuestionOptionId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        QuestionId      INT                NOT NULL,
        OptionText      NVARCHAR(200)      NOT NULL,
        OptionOrder     INT                NOT NULL,
        IsCorrect       BIT                NOT NULL,
        CONSTRAINT FK_QuestionOptions_Questions FOREIGN KEY (QuestionId) REFERENCES Questions (QuestionId),
        CONSTRAINT UQ_QuestionOptions_Question_OptionOrder UNIQUE (QuestionId, OptionOrder),
        CONSTRAINT CK_QuestionOptions_OptionOrder CHECK (OptionOrder BETWEEN 1 AND 3),
        CONSTRAINT CK_QuestionOptions_Text CHECK (LEN(LTRIM(RTRIM(OptionText))) > 0)
    );
END
GO

-- Standard Practice has no pass/fail. UnansweredCount is stored because the result screen shows it.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PracticeAttempts')
BEGIN
    CREATE TABLE PracticeAttempts
    (
        PracticeAttemptId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserId          INT                NOT NULL,
        PracticeSetId   INT                NOT NULL,
        StartedAt       DATETIME2          NOT NULL,
        CompletedAt     DATETIME2          NOT NULL,
        TotalQuestions  INT                NOT NULL,
        CorrectCount    INT                NOT NULL,
        UnansweredCount INT                NOT NULL,
        CONSTRAINT FK_PracticeAttempts_Users FOREIGN KEY (UserId) REFERENCES Users (UserId),
        CONSTRAINT FK_PracticeAttempts_PracticeSets FOREIGN KEY (PracticeSetId) REFERENCES PracticeSets (PracticeSetId),
        CONSTRAINT CK_PracticeAttempts_Counts CHECK (TotalQuestions > 0 AND CorrectCount >= 0 AND UnansweredCount >= 0 AND CorrectCount + UnansweredCount <= TotalQuestions),
        CONSTRAINT CK_PracticeAttempts_Times CHECK (CompletedAt >= StartedAt)
    );
END
GO

-- Snapshots keep an old review accurate after the Question Bank is edited. Unanswered = no selected option, no selected
-- snapshot, IsCorrect 0. ImagePathSnapshot is only a path: future image management must keep files history still uses.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PracticeAttemptQuestions')
BEGIN
    CREATE TABLE PracticeAttemptQuestions
    (
        PracticeAttemptQuestionId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        PracticeAttemptId INT              NOT NULL,
        QuestionId      INT                NOT NULL,
        QuestionNumber  INT                NOT NULL,
        SelectedOptionId INT               NULL,
        IsCorrect       BIT                NOT NULL,
        QuestionTextSnapshot NVARCHAR(500) NOT NULL,
        ExplanationSnapshot NVARCHAR(500)  NOT NULL,
        OptionATextSnapshot NVARCHAR(200)  NOT NULL,
        OptionBTextSnapshot NVARCHAR(200)  NOT NULL,
        OptionCTextSnapshot NVARCHAR(200)  NOT NULL,
        CorrectOptionSnapshot CHAR(1)      NOT NULL,
        SelectedOptionSnapshot CHAR(1)     NULL,
        SelectedOptionTextSnapshot NVARCHAR(200) NULL,
        ImagePathSnapshot NVARCHAR(500)    NULL,
        ImageAltTextSnapshot NVARCHAR(200) NULL,
        CONSTRAINT FK_PracticeAttemptQuestions_Attempts FOREIGN KEY (PracticeAttemptId) REFERENCES PracticeAttempts (PracticeAttemptId),
        CONSTRAINT FK_PracticeAttemptQuestions_Questions FOREIGN KEY (QuestionId) REFERENCES Questions (QuestionId),
        CONSTRAINT FK_PracticeAttemptQuestions_QuestionOptions FOREIGN KEY (SelectedOptionId) REFERENCES QuestionOptions (QuestionOptionId),
        CONSTRAINT UQ_PracticeAttemptQuestions_Attempt_Question UNIQUE (PracticeAttemptId, QuestionId),
        CONSTRAINT UQ_PracticeAttemptQuestions_Attempt_Number UNIQUE (PracticeAttemptId, QuestionNumber),
        CONSTRAINT CK_PracticeAttemptQuestions_Number CHECK (QuestionNumber > 0),
        CONSTRAINT CK_PracticeAttemptQuestions_CorrectOption CHECK (CorrectOptionSnapshot IN ('A', 'B', 'C')),
        CONSTRAINT CK_PracticeAttemptQuestions_SelectedOption CHECK (SelectedOptionSnapshot IS NULL OR SelectedOptionSnapshot IN ('A', 'B', 'C')),
        CONSTRAINT CK_PracticeAttemptQuestions_Answered CHECK
        (
            (SelectedOptionId IS NULL AND SelectedOptionSnapshot IS NULL AND SelectedOptionTextSnapshot IS NULL AND IsCorrect = 0)
            OR (SelectedOptionId IS NOT NULL AND SelectedOptionSnapshot IS NOT NULL AND SelectedOptionTextSnapshot IS NOT NULL)
        ),
        CONSTRAINT CK_PracticeAttemptQuestions_Correct CHECK
        (
            SelectedOptionSnapshot IS NULL
            OR IsCorrect = CASE WHEN SelectedOptionSnapshot = CorrectOptionSnapshot THEN 1 ELSE 0 END
        ),
        CONSTRAINT CK_PracticeAttemptQuestions_ImageAlt CHECK (ImagePathSnapshot IS NULL OR (ImageAltTextSnapshot IS NOT NULL AND LEN(LTRIM(RTRIM(ImageAltTextSnapshot))) > 0))
    );
END
GO

-- The two fixed exam types are application rules (Scripts/mock-exam-demo.js), so there is no ExamTypes table.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MockExamSets')
BEGIN
    CREATE TABLE MockExamSets
    (
        MockExamSetId   INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ExamTypeCode    NVARCHAR(30)       NOT NULL,
        SetNumber       INT                NOT NULL,
        IsActive        BIT                NOT NULL CONSTRAINT DF_MockExamSets_IsActive DEFAULT 1,
        CreatedAt       DATETIME2          NOT NULL,
        UpdatedAt       DATETIME2          NOT NULL,
        CONSTRAINT UQ_MockExamSets_ExamType_SetNumber UNIQUE (ExamTypeCode, SetNumber),
        CONSTRAINT CK_MockExamSets_ExamType CHECK (ExamTypeCode IN ('combined', 'kpp-standard')),
        CONSTRAINT CK_MockExamSets_SetNumber CHECK (SetNumber > 0)
    );
END
GO

-- DisplayOrder is the fixed position of the question within the exam. The per-section question counts of each exam type
-- are application rules validated by the future Mock Exam save.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MockExamSetQuestions')
BEGIN
    CREATE TABLE MockExamSetQuestions
    (
        MockExamSetQuestionId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        MockExamSetId   INT                NOT NULL,
        QuestionId      INT                NOT NULL,
        DisplayOrder    INT                NOT NULL,
        CONSTRAINT FK_MockExamSetQuestions_Sets FOREIGN KEY (MockExamSetId) REFERENCES MockExamSets (MockExamSetId),
        CONSTRAINT FK_MockExamSetQuestions_Questions FOREIGN KEY (QuestionId) REFERENCES Questions (QuestionId),
        CONSTRAINT UQ_MockExamSetQuestions_Set_Question UNIQUE (MockExamSetId, QuestionId),
        CONSTRAINT UQ_MockExamSetQuestions_Set_DisplayOrder UNIQUE (MockExamSetId, DisplayOrder),
        CONSTRAINT CK_MockExamSetQuestions_DisplayOrder CHECK (DisplayOrder > 0)
    );
END
GO

-- IsPassed is decided by the server from the exam type's fixed pass marks when the attempt is saved.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MockExamAttempts')
BEGIN
    CREATE TABLE MockExamAttempts
    (
        MockExamAttemptId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserId          INT                NOT NULL,
        MockExamSetId   INT                NOT NULL,
        StartedAt       DATETIME2          NOT NULL,
        CompletedAt     DATETIME2          NOT NULL,
        ColourBlindCorrect INT             NOT NULL,
        ColourBlindTotal INT               NOT NULL,
        TheoryCorrect   INT                NOT NULL,
        TheoryTotal     INT                NOT NULL,
        IsPassed        BIT                NOT NULL,
        TimeTakenSeconds INT               NOT NULL,
        AutoSubmitted   BIT                NOT NULL CONSTRAINT DF_MockExamAttempts_AutoSubmitted DEFAULT 0,
        CONSTRAINT FK_MockExamAttempts_Users FOREIGN KEY (UserId) REFERENCES Users (UserId),
        CONSTRAINT FK_MockExamAttempts_Sets FOREIGN KEY (MockExamSetId) REFERENCES MockExamSets (MockExamSetId),
        CONSTRAINT CK_MockExamAttempts_ColourBlind CHECK (ColourBlindTotal > 0 AND ColourBlindCorrect >= 0 AND ColourBlindCorrect <= ColourBlindTotal),
        CONSTRAINT CK_MockExamAttempts_Theory CHECK (TheoryTotal > 0 AND TheoryCorrect >= 0 AND TheoryCorrect <= TheoryTotal),
        CONSTRAINT CK_MockExamAttempts_Time CHECK (TimeTakenSeconds >= 0),
        CONSTRAINT CK_MockExamAttempts_Times CHECK (CompletedAt >= StartedAt)
    );
END
GO

-- Same snapshot approach as PracticeAttemptQuestions, plus the category code (for section grouping in a review) and the
-- flag, which is kept whether or not the question was answered.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MockExamAttemptQuestions')
BEGIN
    CREATE TABLE MockExamAttemptQuestions
    (
        MockExamAttemptQuestionId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        MockExamAttemptId INT              NOT NULL,
        QuestionId      INT                NOT NULL,
        QuestionNumber  INT                NOT NULL,
        SelectedOptionId INT               NULL,
        IsCorrect       BIT                NOT NULL,
        IsFlagged       BIT                NOT NULL CONSTRAINT DF_MockExamAttemptQuestions_IsFlagged DEFAULT 0,
        QuestionTextSnapshot NVARCHAR(500) NOT NULL,
        ExplanationSnapshot NVARCHAR(500)  NOT NULL,
        OptionATextSnapshot NVARCHAR(200)  NOT NULL,
        OptionBTextSnapshot NVARCHAR(200)  NOT NULL,
        OptionCTextSnapshot NVARCHAR(200)  NOT NULL,
        CorrectOptionSnapshot CHAR(1)      NOT NULL,
        SelectedOptionSnapshot CHAR(1)     NULL,
        SelectedOptionTextSnapshot NVARCHAR(200) NULL,
        ImagePathSnapshot NVARCHAR(500)    NULL,
        ImageAltTextSnapshot NVARCHAR(200) NULL,
        QuestionCategoryCodeSnapshot NVARCHAR(40) NOT NULL,
        CONSTRAINT FK_MockExamAttemptQuestions_Attempts FOREIGN KEY (MockExamAttemptId) REFERENCES MockExamAttempts (MockExamAttemptId),
        CONSTRAINT FK_MockExamAttemptQuestions_Questions FOREIGN KEY (QuestionId) REFERENCES Questions (QuestionId),
        CONSTRAINT FK_MockExamAttemptQuestions_QuestionOptions FOREIGN KEY (SelectedOptionId) REFERENCES QuestionOptions (QuestionOptionId),
        CONSTRAINT UQ_MockExamAttemptQuestions_Attempt_Question UNIQUE (MockExamAttemptId, QuestionId),
        CONSTRAINT UQ_MockExamAttemptQuestions_Attempt_Number UNIQUE (MockExamAttemptId, QuestionNumber),
        CONSTRAINT CK_MockExamAttemptQuestions_Number CHECK (QuestionNumber > 0),
        CONSTRAINT CK_MockExamAttemptQuestions_CorrectOption CHECK (CorrectOptionSnapshot IN ('A', 'B', 'C')),
        CONSTRAINT CK_MockExamAttemptQuestions_SelectedOption CHECK (SelectedOptionSnapshot IS NULL OR SelectedOptionSnapshot IN ('A', 'B', 'C')),
        CONSTRAINT CK_MockExamAttemptQuestions_Answered CHECK
        (
            (SelectedOptionId IS NULL AND SelectedOptionSnapshot IS NULL AND SelectedOptionTextSnapshot IS NULL AND IsCorrect = 0)
            OR (SelectedOptionId IS NOT NULL AND SelectedOptionSnapshot IS NOT NULL AND SelectedOptionTextSnapshot IS NOT NULL)
        ),
        CONSTRAINT CK_MockExamAttemptQuestions_Correct CHECK
        (
            SelectedOptionSnapshot IS NULL
            OR IsCorrect = CASE WHEN SelectedOptionSnapshot = CorrectOptionSnapshot THEN 1 ELSE 0 END
        ),
        CONSTRAINT CK_MockExamAttemptQuestions_ImageAlt CHECK (ImagePathSnapshot IS NULL OR (ImageAltTextSnapshot IS NOT NULL AND LEN(LTRIM(RTRIM(ImageAltTextSnapshot))) > 0)),
        CONSTRAINT CK_MockExamAttemptQuestions_CategoryCode CHECK (LEN(LTRIM(RTRIM(QuestionCategoryCodeSnapshot))) > 0)
    );
END
GO

-- Indexes. Lookups already served by a unique constraint (PracticeSets by category, QuestionOptions by question,
-- MockExamSets by type) get no extra index. The rest support history queries and the foreign-key checks that make
-- deleting a referenced question, option or set fail.
-- Filtered indexes require QUOTED_IDENTIFIER ON (the ADO.NET default; sqlcmd needs it explicit).
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Questions_QuestionCategoryId' AND object_id = OBJECT_ID('Questions'))
    CREATE INDEX IX_Questions_QuestionCategoryId ON Questions (QuestionCategoryId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Questions_PracticeSetId' AND object_id = OBJECT_ID('Questions'))
    CREATE INDEX IX_Questions_PracticeSetId ON Questions (PracticeSetId) WHERE PracticeSetId IS NOT NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_QuestionOptions_OneCorrect' AND object_id = OBJECT_ID('QuestionOptions'))
    CREATE UNIQUE INDEX UX_QuestionOptions_OneCorrect ON QuestionOptions (QuestionId) WHERE IsCorrect = 1;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PracticeAttempts_User_CompletedAt' AND object_id = OBJECT_ID('PracticeAttempts'))
    CREATE INDEX IX_PracticeAttempts_User_CompletedAt ON PracticeAttempts (UserId, CompletedAt DESC);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PracticeAttempts_PracticeSet_User' AND object_id = OBJECT_ID('PracticeAttempts'))
    CREATE INDEX IX_PracticeAttempts_PracticeSet_User ON PracticeAttempts (PracticeSetId, UserId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PracticeAttemptQuestions_QuestionId' AND object_id = OBJECT_ID('PracticeAttemptQuestions'))
    CREATE INDEX IX_PracticeAttemptQuestions_QuestionId ON PracticeAttemptQuestions (QuestionId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PracticeAttemptQuestions_SelectedOptionId' AND object_id = OBJECT_ID('PracticeAttemptQuestions'))
    CREATE INDEX IX_PracticeAttemptQuestions_SelectedOptionId ON PracticeAttemptQuestions (SelectedOptionId) WHERE SelectedOptionId IS NOT NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_MockExamSetQuestions_QuestionId' AND object_id = OBJECT_ID('MockExamSetQuestions'))
    CREATE INDEX IX_MockExamSetQuestions_QuestionId ON MockExamSetQuestions (QuestionId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_MockExamAttempts_User_CompletedAt' AND object_id = OBJECT_ID('MockExamAttempts'))
    CREATE INDEX IX_MockExamAttempts_User_CompletedAt ON MockExamAttempts (UserId, CompletedAt DESC);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_MockExamAttempts_Set_User' AND object_id = OBJECT_ID('MockExamAttempts'))
    CREATE INDEX IX_MockExamAttempts_Set_User ON MockExamAttempts (MockExamSetId, UserId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_MockExamAttemptQuestions_QuestionId' AND object_id = OBJECT_ID('MockExamAttemptQuestions'))
    CREATE INDEX IX_MockExamAttemptQuestions_QuestionId ON MockExamAttemptQuestions (QuestionId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_MockExamAttemptQuestions_SelectedOptionId' AND object_id = OBJECT_ID('MockExamAttemptQuestions'))
    CREATE INDEX IX_MockExamAttemptQuestions_SelectedOptionId ON MockExamAttemptQuestions (SelectedOptionId) WHERE SelectedOptionId IS NOT NULL;
GO
