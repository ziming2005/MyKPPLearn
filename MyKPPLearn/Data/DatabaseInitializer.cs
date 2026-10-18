using System;
using System.Data.SqlClient;
using System.IO;
using MyKPPLearn.Security;

namespace MyKPPLearn.Data
{
    /// <summary>
    /// Ensures the LocalDB database file and its tables exist. No account is created here: the Administrator
    /// row lives only in the Users table of the database. Safe to run on every application start:
    /// every step here is idempotent and never wipes existing data.
    /// </summary>
    public static class DatabaseInitializer
    {
        public static void Initialize(string appDataPhysicalPath)
        {
            EnsureDatabaseFileExists(appDataPhysicalPath);
            EnsureUsersTableExists();
            EnsureProfileImagePathColumnExists();
            EnsureUserStatusColumnExists();
            EnsureLegacyUserIsActiveColumnRemoved();
            EnsureLearningTablesExist();
            EnsureLessonImagePositionColumnExists();
            EnsureHiddenRoadSignTablesExist();
            EnsureQuestionBankAndExamTablesExist();
        }

        // Shared Question Bank, Standard Practice and Mock Exam schema (SCHEMA ONLY: no repositories or pages use these
        // tables yet). Creation order respects foreign keys:
        //   QuestionCategories -> PracticeSets -> Questions -> QuestionOptions
        //   -> PracticeAttempts -> PracticeAttemptQuestions
        //   -> MockExamSets -> MockExamSetQuestions -> MockExamAttempts -> MockExamAttemptQuestions
        // Every statement is guarded by an existence check, never drops or alters existing data, and nothing is seeded
        // except the four fixed QuestionCategories (system configuration, not demo content). A table that already exists is
        // left exactly as it is. No foreign key cascades: attempt history can never be removed by deleting a question, option
        // or set. Attempt-question rows carry snapshots of the question content so history stays accurate after later edits.
        private static void EnsureQuestionBankAndExamTablesExist()
        {
            // The four shared categories. "Colour Blind Test" (a Mock Exam section label) uses colour-recognition.
            const string categoriesSql = @"
                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'QuestionCategories')
                BEGIN
                    CREATE TABLE QuestionCategories
                    (
                        QuestionCategoryId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        Code NVARCHAR(40) NOT NULL,
                        Title NVARCHAR(100) NOT NULL,
                        Description NVARCHAR(300) NULL,
                        DisplayOrder INT NOT NULL,
                        IsActive BIT NOT NULL CONSTRAINT DF_QuestionCategories_IsActive DEFAULT 1,
                        CreatedAt DATETIME2 NOT NULL,
                        UpdatedAt DATETIME2 NOT NULL,
                        CONSTRAINT UQ_QuestionCategories_Code UNIQUE (Code),
                        CONSTRAINT UQ_QuestionCategories_DisplayOrder UNIQUE (DisplayOrder),
                        CONSTRAINT CK_QuestionCategories_DisplayOrder CHECK (DisplayOrder > 0)
                    );
                END";

            // Inserts only the categories that are missing (matched by Code). Existing rows, including any edited
            // Title/Description/IsActive, are never updated, and no other row is touched.
            const string categorySeedSql = @"
                INSERT INTO QuestionCategories (Code, Title, Description, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
                SELECT v.Code, v.Title, v.Description, v.DisplayOrder, 1, SYSUTCDATETIME(), SYSUTCDATETIME()
                FROM (VALUES
                    (N'colour-recognition', N'Colour Recognition', N'Practice identifying the meaning of key road and signal colours.', 1),
                    (N'road-signs', N'Road Signs', N'Test your knowledge of warning, regulatory and information signs.', 2),
                    (N'rules-of-the-road', N'Rules of the Road', N'Review common driving scenarios and correct road behaviour.', 3),
                    (N'kejara-safety', N'KEJARA & Safety', N'Practice questions on the demerit points system and road safety.', 4)
                ) AS v (Code, Title, Description, DisplayOrder)
                WHERE NOT EXISTS (SELECT 1 FROM QuestionCategories c WHERE c.Code = v.Code);";

            // Practice Set N is generated from SetNumber (no title column). Different categories may reuse a number.
            // UQ_PracticeSets_Id_Category exists only so Questions can reference (set, category) together.
            const string practiceSetsSql = @"
                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PracticeSets')
                BEGIN
                    CREATE TABLE PracticeSets
                    (
                        PracticeSetId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        QuestionCategoryId INT NOT NULL,
                        SetNumber INT NOT NULL,
                        IsActive BIT NOT NULL CONSTRAINT DF_PracticeSets_IsActive DEFAULT 1,
                        CreatedAt DATETIME2 NOT NULL,
                        UpdatedAt DATETIME2 NOT NULL,
                        CONSTRAINT FK_PracticeSets_QuestionCategories FOREIGN KEY (QuestionCategoryId) REFERENCES QuestionCategories (QuestionCategoryId),
                        CONSTRAINT UQ_PracticeSets_Category_SetNumber UNIQUE (QuestionCategoryId, SetNumber),
                        CONSTRAINT UQ_PracticeSets_Id_Category UNIQUE (PracticeSetId, QuestionCategoryId),
                        CONSTRAINT CK_PracticeSets_SetNumber CHECK (SetNumber > 0)
                    );
                END";

            // One shared Question Bank. PracticeSetId NULL = available to Mock Exams only. The composite foreign key
            // (PracticeSetId, QuestionCategoryId) makes the database reject a question whose category differs from its
            // Practice Set's category; it is not checked while PracticeSetId is NULL.
            const string questionsSql = @"
                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Questions')
                BEGIN
                    CREATE TABLE Questions
                    (
                        QuestionId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        QuestionCategoryId INT NOT NULL,
                        PracticeSetId INT NULL,
                        QuestionText NVARCHAR(500) NOT NULL,
                        Explanation NVARCHAR(500) NOT NULL,
                        ImagePath NVARCHAR(500) NULL,
                        ImageAltText NVARCHAR(200) NULL,
                        IsActive BIT NOT NULL CONSTRAINT DF_Questions_IsActive DEFAULT 1,
                        CreatedAt DATETIME2 NOT NULL,
                        UpdatedAt DATETIME2 NOT NULL,
                        CONSTRAINT FK_Questions_QuestionCategories FOREIGN KEY (QuestionCategoryId) REFERENCES QuestionCategories (QuestionCategoryId),
                        CONSTRAINT FK_Questions_PracticeSets FOREIGN KEY (PracticeSetId, QuestionCategoryId) REFERENCES PracticeSets (PracticeSetId, QuestionCategoryId),
                        CONSTRAINT CK_Questions_Text CHECK (LEN(LTRIM(RTRIM(QuestionText))) > 0),
                        CONSTRAINT CK_Questions_Explanation CHECK (LEN(LTRIM(RTRIM(Explanation))) > 0),
                        CONSTRAINT CK_Questions_ImageAlt CHECK (ImagePath IS NULL OR (ImageAltText IS NOT NULL AND LEN(LTRIM(RTRIM(ImageAltText))) > 0))
                    );
                END";

            // Final design: every question has exactly A, B and C (1-3). The database prevents duplicate positions and a second
            // correct option; 'exactly three rows' and 'exactly one correct' are validated by the future Question Bank save.
            const string optionsSql = @"
                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'QuestionOptions')
                BEGIN
                    CREATE TABLE QuestionOptions
                    (
                        QuestionOptionId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        QuestionId INT NOT NULL,
                        OptionText NVARCHAR(200) NOT NULL,
                        OptionOrder INT NOT NULL,
                        IsCorrect BIT NOT NULL,
                        CONSTRAINT FK_QuestionOptions_Questions FOREIGN KEY (QuestionId) REFERENCES Questions (QuestionId),
                        CONSTRAINT UQ_QuestionOptions_Question_OptionOrder UNIQUE (QuestionId, OptionOrder),
                        CONSTRAINT CK_QuestionOptions_OptionOrder CHECK (OptionOrder BETWEEN 1 AND 3),
                        CONSTRAINT CK_QuestionOptions_Text CHECK (LEN(LTRIM(RTRIM(OptionText))) > 0)
                    );
                END";

            // Standard Practice has no pass/fail. UnansweredCount is stored because the result screen shows it.
            const string practiceAttemptsSql = @"
                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PracticeAttempts')
                BEGIN
                    CREATE TABLE PracticeAttempts
                    (
                        PracticeAttemptId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        UserId INT NOT NULL,
                        PracticeSetId INT NOT NULL,
                        StartedAt DATETIME2 NOT NULL,
                        CompletedAt DATETIME2 NOT NULL,
                        TotalQuestions INT NOT NULL,
                        CorrectCount INT NOT NULL,
                        UnansweredCount INT NOT NULL,
                        CONSTRAINT FK_PracticeAttempts_Users FOREIGN KEY (UserId) REFERENCES Users (UserId),
                        CONSTRAINT FK_PracticeAttempts_PracticeSets FOREIGN KEY (PracticeSetId) REFERENCES PracticeSets (PracticeSetId),
                        CONSTRAINT CK_PracticeAttempts_Counts CHECK (TotalQuestions > 0 AND CorrectCount >= 0 AND UnansweredCount >= 0 AND CorrectCount + UnansweredCount <= TotalQuestions),
                        CONSTRAINT CK_PracticeAttempts_Times CHECK (CompletedAt >= StartedAt)
                    );
                END";

            // History rows carry snapshots of the question as the learner saw it, so a later edit to the Question Bank never
            // changes an old review. Unanswered = no selected option, no selected snapshot, IsCorrect 0.
            // ImagePathSnapshot is only a path: future image management must keep files that history still references.
            const string practiceAttemptQuestionsSql = @"
                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PracticeAttemptQuestions')
                BEGIN
                    CREATE TABLE PracticeAttemptQuestions
                    (
                        PracticeAttemptQuestionId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        PracticeAttemptId INT NOT NULL,
                        QuestionId INT NOT NULL,
                        QuestionNumber INT NOT NULL,
                        SelectedOptionId INT NULL,
                        IsCorrect BIT NOT NULL,
                        QuestionTextSnapshot NVARCHAR(500) NOT NULL,
                        ExplanationSnapshot NVARCHAR(500) NOT NULL,
                        OptionATextSnapshot NVARCHAR(200) NOT NULL,
                        OptionBTextSnapshot NVARCHAR(200) NOT NULL,
                        OptionCTextSnapshot NVARCHAR(200) NOT NULL,
                        CorrectOptionSnapshot CHAR(1) NOT NULL,
                        SelectedOptionSnapshot CHAR(1) NULL,
                        SelectedOptionTextSnapshot NVARCHAR(200) NULL,
                        ImagePathSnapshot NVARCHAR(500) NULL,
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
                END";

            // The two fixed exam types are application rules (see Scripts/mock-exam-demo.js), so there is no ExamTypes table.
            const string mockExamSetsSql = @"
                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MockExamSets')
                BEGIN
                    CREATE TABLE MockExamSets
                    (
                        MockExamSetId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        ExamTypeCode NVARCHAR(30) NOT NULL,
                        SetNumber INT NOT NULL,
                        IsActive BIT NOT NULL CONSTRAINT DF_MockExamSets_IsActive DEFAULT 1,
                        CreatedAt DATETIME2 NOT NULL,
                        UpdatedAt DATETIME2 NOT NULL,
                        CONSTRAINT UQ_MockExamSets_ExamType_SetNumber UNIQUE (ExamTypeCode, SetNumber),
                        CONSTRAINT CK_MockExamSets_ExamType CHECK (ExamTypeCode IN ('combined', 'kpp-standard')),
                        CONSTRAINT CK_MockExamSets_SetNumber CHECK (SetNumber > 0)
                    );
                END";

            // DisplayOrder is the fixed position of the question within the exam. The per-section question counts of each
            // exam type are application rules validated by the future Mock Exam save.
            const string mockExamSetQuestionsSql = @"
                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MockExamSetQuestions')
                BEGIN
                    CREATE TABLE MockExamSetQuestions
                    (
                        MockExamSetQuestionId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        MockExamSetId INT NOT NULL,
                        QuestionId INT NOT NULL,
                        DisplayOrder INT NOT NULL,
                        CONSTRAINT FK_MockExamSetQuestions_Sets FOREIGN KEY (MockExamSetId) REFERENCES MockExamSets (MockExamSetId),
                        CONSTRAINT FK_MockExamSetQuestions_Questions FOREIGN KEY (QuestionId) REFERENCES Questions (QuestionId),
                        CONSTRAINT UQ_MockExamSetQuestions_Set_Question UNIQUE (MockExamSetId, QuestionId),
                        CONSTRAINT UQ_MockExamSetQuestions_Set_DisplayOrder UNIQUE (MockExamSetId, DisplayOrder),
                        CONSTRAINT CK_MockExamSetQuestions_DisplayOrder CHECK (DisplayOrder > 0)
                    );
                END";

            // IsPassed is decided by the server from the exam type's fixed pass marks when the attempt is saved.
            const string mockExamAttemptsSql = @"
                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MockExamAttempts')
                BEGIN
                    CREATE TABLE MockExamAttempts
                    (
                        MockExamAttemptId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        UserId INT NOT NULL,
                        MockExamSetId INT NOT NULL,
                        StartedAt DATETIME2 NOT NULL,
                        CompletedAt DATETIME2 NOT NULL,
                        ColourBlindCorrect INT NOT NULL,
                        ColourBlindTotal INT NOT NULL,
                        TheoryCorrect INT NOT NULL,
                        TheoryTotal INT NOT NULL,
                        IsPassed BIT NOT NULL,
                        TimeTakenSeconds INT NOT NULL,
                        AutoSubmitted BIT NOT NULL CONSTRAINT DF_MockExamAttempts_AutoSubmitted DEFAULT 0,
                        CONSTRAINT FK_MockExamAttempts_Users FOREIGN KEY (UserId) REFERENCES Users (UserId),
                        CONSTRAINT FK_MockExamAttempts_Sets FOREIGN KEY (MockExamSetId) REFERENCES MockExamSets (MockExamSetId),
                        CONSTRAINT CK_MockExamAttempts_ColourBlind CHECK (ColourBlindTotal > 0 AND ColourBlindCorrect >= 0 AND ColourBlindCorrect <= ColourBlindTotal),
                        CONSTRAINT CK_MockExamAttempts_Theory CHECK (TheoryTotal > 0 AND TheoryCorrect >= 0 AND TheoryCorrect <= TheoryTotal),
                        CONSTRAINT CK_MockExamAttempts_Time CHECK (TimeTakenSeconds >= 0),
                        CONSTRAINT CK_MockExamAttempts_Times CHECK (CompletedAt >= StartedAt)
                    );
                END";

            // Same snapshot approach as PracticeAttemptQuestions, plus the category code (for section grouping in a review)
            // and the flag, which is kept whether or not the question was answered.
            const string mockExamAttemptQuestionsSql = @"
                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MockExamAttemptQuestions')
                BEGIN
                    CREATE TABLE MockExamAttemptQuestions
                    (
                        MockExamAttemptQuestionId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        MockExamAttemptId INT NOT NULL,
                        QuestionId INT NOT NULL,
                        QuestionNumber INT NOT NULL,
                        SelectedOptionId INT NULL,
                        IsCorrect BIT NOT NULL,
                        IsFlagged BIT NOT NULL CONSTRAINT DF_MockExamAttemptQuestions_IsFlagged DEFAULT 0,
                        QuestionTextSnapshot NVARCHAR(500) NOT NULL,
                        ExplanationSnapshot NVARCHAR(500) NOT NULL,
                        OptionATextSnapshot NVARCHAR(200) NOT NULL,
                        OptionBTextSnapshot NVARCHAR(200) NOT NULL,
                        OptionCTextSnapshot NVARCHAR(200) NOT NULL,
                        CorrectOptionSnapshot CHAR(1) NOT NULL,
                        SelectedOptionSnapshot CHAR(1) NULL,
                        SelectedOptionTextSnapshot NVARCHAR(200) NULL,
                        ImagePathSnapshot NVARCHAR(500) NULL,
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
                END";

            DatabaseHelper.ExecuteNonQuery(categoriesSql);
            DatabaseHelper.ExecuteNonQuery(categorySeedSql);
            DatabaseHelper.ExecuteNonQuery(practiceSetsSql);
            DatabaseHelper.ExecuteNonQuery(questionsSql);
            DatabaseHelper.ExecuteNonQuery(optionsSql);
            DatabaseHelper.ExecuteNonQuery(practiceAttemptsSql);
            DatabaseHelper.ExecuteNonQuery(practiceAttemptQuestionsSql);
            DatabaseHelper.ExecuteNonQuery(mockExamSetsSql);
            DatabaseHelper.ExecuteNonQuery(mockExamSetQuestionsSql);
            DatabaseHelper.ExecuteNonQuery(mockExamAttemptsSql);
            DatabaseHelper.ExecuteNonQuery(mockExamAttemptQuestionsSql);

            // Indexes. Lookups already served by a unique constraint (for example PracticeSets by category, QuestionOptions by
            // question, MockExamSets by type) get no extra index. The rest support history queries and the foreign-key
            // checks that make deleting a referenced question, option or set fail.
            EnsureIndex("Questions", "IX_Questions_QuestionCategoryId",
                "CREATE INDEX IX_Questions_QuestionCategoryId ON Questions (QuestionCategoryId);");
            EnsureIndex("Questions", "IX_Questions_PracticeSetId",
                "CREATE INDEX IX_Questions_PracticeSetId ON Questions (PracticeSetId) WHERE PracticeSetId IS NOT NULL;");
            EnsureIndex("QuestionOptions", "UX_QuestionOptions_OneCorrect",
                "CREATE UNIQUE INDEX UX_QuestionOptions_OneCorrect ON QuestionOptions (QuestionId) WHERE IsCorrect = 1;");
            EnsureIndex("PracticeAttempts", "IX_PracticeAttempts_User_CompletedAt",
                "CREATE INDEX IX_PracticeAttempts_User_CompletedAt ON PracticeAttempts (UserId, CompletedAt DESC);");
            EnsureIndex("PracticeAttempts", "IX_PracticeAttempts_PracticeSet_User",
                "CREATE INDEX IX_PracticeAttempts_PracticeSet_User ON PracticeAttempts (PracticeSetId, UserId);");
            EnsureIndex("PracticeAttemptQuestions", "IX_PracticeAttemptQuestions_QuestionId",
                "CREATE INDEX IX_PracticeAttemptQuestions_QuestionId ON PracticeAttemptQuestions (QuestionId);");
            EnsureIndex("PracticeAttemptQuestions", "IX_PracticeAttemptQuestions_SelectedOptionId",
                "CREATE INDEX IX_PracticeAttemptQuestions_SelectedOptionId ON PracticeAttemptQuestions (SelectedOptionId) WHERE SelectedOptionId IS NOT NULL;");
            EnsureIndex("MockExamSetQuestions", "IX_MockExamSetQuestions_QuestionId",
                "CREATE INDEX IX_MockExamSetQuestions_QuestionId ON MockExamSetQuestions (QuestionId);");
            EnsureIndex("MockExamAttempts", "IX_MockExamAttempts_User_CompletedAt",
                "CREATE INDEX IX_MockExamAttempts_User_CompletedAt ON MockExamAttempts (UserId, CompletedAt DESC);");
            EnsureIndex("MockExamAttempts", "IX_MockExamAttempts_Set_User",
                "CREATE INDEX IX_MockExamAttempts_Set_User ON MockExamAttempts (MockExamSetId, UserId);");
            EnsureIndex("MockExamAttemptQuestions", "IX_MockExamAttemptQuestions_QuestionId",
                "CREATE INDEX IX_MockExamAttemptQuestions_QuestionId ON MockExamAttemptQuestions (QuestionId);");
            EnsureIndex("MockExamAttemptQuestions", "IX_MockExamAttemptQuestions_SelectedOptionId",
                "CREATE INDEX IX_MockExamAttemptQuestions_SelectedOptionId ON MockExamAttemptQuestions (SelectedOptionId) WHERE SelectedOptionId IS NOT NULL;");
        }

        // Creates an index only when no index of that name exists on the table. All arguments are constants in this file.
        private static void EnsureIndex(string table, string indexName, string createIndexSql)
        {
            DatabaseHelper.ExecuteNonQuery(
                "IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = '" + indexName + "' AND object_id = OBJECT_ID('" + table + "')) " +
                "BEGIN " + createIndexSql + " END");
        }

        // Account status for a later Disable/Enable feature. SCHEMA ONLY: nothing reads or enforces it yet (login is unchanged).
        // Existing users receive 'Active' from the column default. The CHECK is added only if every existing value is valid, so
        // a column that already exists is never rewritten and no row is ever changed.
        private static void EnsureUserStatusColumnExists()
        {
            const string addColumnSql = @"
                IF COL_LENGTH('Users', 'Status') IS NULL
                BEGIN
                    ALTER TABLE Users ADD Status NVARCHAR(20) NOT NULL CONSTRAINT DF_Users_Status DEFAULT 'Active';
                END";

            const string addCheckSql = @"
                IF COL_LENGTH('Users', 'Status') IS NOT NULL
                   AND NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Users_Status' AND parent_object_id = OBJECT_ID('Users'))
                BEGIN
                    EXEC('IF NOT EXISTS (SELECT 1 FROM Users WHERE Status NOT IN (''Active'', ''Disabled''))
                              ALTER TABLE Users ADD CONSTRAINT CK_Users_Status CHECK (Status IN (''Active'', ''Disabled''))');
                END";

            DatabaseHelper.ExecuteNonQuery(addColumnSql);
            DatabaseHelper.ExecuteNonQuery(addCheckSql);
        }

        // One-time cleanup of a legacy Users.IsActive column that existed in some development databases but was never part of
        // this project's schema. Users.Status (Active / Disabled) is the single account-status column; future Learner Records
        // and login authorization must use Status and must never reference Users.IsActive.
        // Guarded and transactional: it does nothing when the column is absent (every fresh and every already-cleaned database),
        // and it refuses (with a clear error, changing nothing) if any user is switched off through IsActive, if Status holds an
        // invalid value, or if anything other than the column's own default constraint depends on it. Only that default
        // constraint (found by its real name) and the column itself are removed; the Users table and its rows are never rebuilt.
        private static void EnsureLegacyUserIsActiveColumnRemoved()
        {
            const string sql = @"
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
                END";

            DatabaseHelper.ExecuteNonQuery(sql);
        }

        // Hidden Road Sign Game. Creation order respects foreign keys:
        // Assets -> Practices (Sets) -> Items -> Choices -> Attempts -> AttemptAnswers.
        // Every statement is guarded by an existence check, never drops or alters existing
        // data, and no Game content is ever seeded. Attempts/AttemptAnswers only ever hold
        // COMPLETED learner attempts (rows are inserted in a single transaction on completion).
        private static void EnsureHiddenRoadSignTablesExist()
        {
            const string assetsSql = @"
                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'HiddenRoadSignAssets')
                BEGIN
                    CREATE TABLE HiddenRoadSignAssets
                    (
                        RoadSignAssetId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        Name NVARCHAR(150) NOT NULL,
                        ImagePath NVARCHAR(500) NOT NULL,
                        IsActive BIT NOT NULL CONSTRAINT DF_HiddenRoadSignAssets_IsActive DEFAULT 1,
                        CreatedAt DATETIME2 NOT NULL,
                        UpdatedAt DATETIME2 NOT NULL
                    );
                END";

            const string practicesSql = @"
                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'HiddenRoadSignPractices')
                BEGIN
                    CREATE TABLE HiddenRoadSignPractices
                    (
                        HiddenRoadSignPracticeId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        SetNumber INT NOT NULL,
                        SceneImagePath NVARCHAR(500) NOT NULL,
                        IsActive BIT NOT NULL CONSTRAINT DF_HiddenRoadSignPractices_IsActive DEFAULT 1,
                        CreatedAt DATETIME2 NOT NULL,
                        UpdatedAt DATETIME2 NOT NULL,
                        CONSTRAINT UQ_HiddenRoadSignPractices_SetNumber UNIQUE (SetNumber),
                        CONSTRAINT CK_HiddenRoadSignPractices_SetNumber CHECK (SetNumber > 0)
                    );
                END";

            const string itemsSql = @"
                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'HiddenRoadSignItems')
                BEGIN
                    CREATE TABLE HiddenRoadSignItems
                    (
                        HiddenRoadSignItemId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        HiddenRoadSignPracticeId INT NOT NULL,
                        RoadSignAssetId INT NOT NULL,
                        QuestionText NVARCHAR(1000) NULL,
                        Explanation NVARCHAR(MAX) NULL,
                        HotspotXPercent DECIMAL(5,2) NOT NULL,
                        HotspotYPercent DECIMAL(5,2) NOT NULL,
                        HotspotWidthPercent DECIMAL(5,2) NOT NULL,
                        HotspotHeightPercent DECIMAL(5,2) NOT NULL,
                        IsConfigured BIT NOT NULL CONSTRAINT DF_HiddenRoadSignItems_IsConfigured DEFAULT 0,
                        CreatedAt DATETIME2 NOT NULL,
                        UpdatedAt DATETIME2 NOT NULL,
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
                END";

            const string choicesSql = @"
                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'HiddenRoadSignChoices')
                BEGIN
                    CREATE TABLE HiddenRoadSignChoices
                    (
                        HiddenRoadSignChoiceId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        HiddenRoadSignItemId INT NOT NULL,
                        ChoiceText NVARCHAR(500) NOT NULL,
                        DisplayOrder INT NOT NULL,
                        IsCorrect BIT NOT NULL,
                        CONSTRAINT FK_HiddenRoadSignChoices_Items FOREIGN KEY (HiddenRoadSignItemId) REFERENCES HiddenRoadSignItems (HiddenRoadSignItemId),
                        CONSTRAINT UQ_HiddenRoadSignChoices_Item_DisplayOrder UNIQUE (HiddenRoadSignItemId, DisplayOrder),
                        CONSTRAINT CK_HiddenRoadSignChoices_DisplayOrder CHECK (DisplayOrder BETWEEN 1 AND 3)
                    );
                END";

            // At most one correct choice per item. 'Exactly three choices' and 'exactly one
            // correct' are enforced by application validation inside the save transaction.
            const string choicesCorrectIndexSql = @"
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_HiddenRoadSignChoices_OneCorrect' AND object_id = OBJECT_ID('HiddenRoadSignChoices'))
                BEGIN
                    CREATE UNIQUE INDEX UX_HiddenRoadSignChoices_OneCorrect
                        ON HiddenRoadSignChoices (HiddenRoadSignItemId) WHERE IsCorrect = 1;
                END";

            const string attemptsSql = @"
                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'HiddenRoadSignAttempts')
                BEGIN
                    CREATE TABLE HiddenRoadSignAttempts
                    (
                        HiddenRoadSignAttemptId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        UserId INT NOT NULL,
                        HiddenRoadSignPracticeId INT NOT NULL,
                        StartedAt DATETIME2 NOT NULL,
                        CompletedAt DATETIME2 NOT NULL,
                        CorrectCount INT NOT NULL,
                        TotalItems INT NOT NULL,
                        CONSTRAINT FK_HiddenRoadSignAttempts_Users FOREIGN KEY (UserId) REFERENCES Users (UserId),
                        CONSTRAINT FK_HiddenRoadSignAttempts_Practices FOREIGN KEY (HiddenRoadSignPracticeId) REFERENCES HiddenRoadSignPractices (HiddenRoadSignPracticeId),
                        CONSTRAINT CK_HiddenRoadSignAttempts_Counts CHECK (TotalItems > 0 AND CorrectCount >= 0 AND CorrectCount <= TotalItems)
                    );
                END";

            const string answersSql = @"
                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'HiddenRoadSignAttemptAnswers')
                BEGIN
                    CREATE TABLE HiddenRoadSignAttemptAnswers
                    (
                        HiddenRoadSignAttemptAnswerId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        HiddenRoadSignAttemptId INT NOT NULL,
                        HiddenRoadSignItemId INT NOT NULL,
                        SelectedChoiceId INT NOT NULL,
                        IsCorrect BIT NOT NULL,
                        FoundOrder INT NOT NULL,
                        CONSTRAINT FK_HiddenRoadSignAttemptAnswers_Attempts FOREIGN KEY (HiddenRoadSignAttemptId) REFERENCES HiddenRoadSignAttempts (HiddenRoadSignAttemptId),
                        CONSTRAINT FK_HiddenRoadSignAttemptAnswers_Items FOREIGN KEY (HiddenRoadSignItemId) REFERENCES HiddenRoadSignItems (HiddenRoadSignItemId),
                        CONSTRAINT FK_HiddenRoadSignAttemptAnswers_Choices FOREIGN KEY (SelectedChoiceId) REFERENCES HiddenRoadSignChoices (HiddenRoadSignChoiceId),
                        CONSTRAINT UQ_HiddenRoadSignAttemptAnswers_Attempt_Item UNIQUE (HiddenRoadSignAttemptId, HiddenRoadSignItemId),
                        CONSTRAINT CK_HiddenRoadSignAttemptAnswers_FoundOrder CHECK (FoundOrder > 0)
                    );
                END";

            const string attemptsUserIndexSql = @"
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_HiddenRoadSignAttempts_User' AND object_id = OBJECT_ID('HiddenRoadSignAttempts'))
                BEGIN
                    CREATE INDEX IX_HiddenRoadSignAttempts_User ON HiddenRoadSignAttempts (UserId, HiddenRoadSignPracticeId);
                END";

            DatabaseHelper.ExecuteNonQuery(assetsSql);
            DatabaseHelper.ExecuteNonQuery(practicesSql);
            DatabaseHelper.ExecuteNonQuery(itemsSql);
            UpgradeHiddenRoadSignItems();
            DatabaseHelper.ExecuteNonQuery(choicesSql);
            DatabaseHelper.ExecuteNonQuery(choicesCorrectIndexSql);
            DatabaseHelper.ExecuteNonQuery(attemptsSql);
            DatabaseHelper.ExecuteNonQuery(answersSql);
            DatabaseHelper.ExecuteNonQuery(attemptsUserIndexSql);
        }

        // In-place upgrade of an EXISTING HiddenRoadSignItems table to the Draft/Configured model.
        // Every step is guarded, so repeated start-ups change nothing, and no step recreates the table or
        // removes any row (Road Signs, choices and learner attempt history are all kept).
        //   1. IsConfigured is added. Existing Road Signs that are complete (a question, exactly three
        //      non-empty choices, exactly one correct) become 1; anything malformed becomes 0 rather than
        //      inventing missing content. New rows default to 0 (Draft).
        //   2. QuestionText becomes nullable so a Draft can exist before its question is written.
        //   3. HiddenRoadSignItems.DisplayOrder, and every unique constraint/index/check/default that
        //      involves it, is dropped. (HiddenRoadSignChoices.DisplayOrder, A/B/C, is untouched.)
        private static void UpgradeHiddenRoadSignItems()
        {
            const string addIsConfiguredSql = @"
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
                END";

            const string questionNullableSql = @"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('HiddenRoadSignItems') AND name = 'QuestionText' AND is_nullable = 0)
                BEGIN
                    ALTER TABLE HiddenRoadSignItems ALTER COLUMN QuestionText NVARCHAR(1000) NULL;
                END";

            const string configuredCheckSql = @"
                IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_HiddenRoadSignItems_Configured' AND parent_object_id = OBJECT_ID('HiddenRoadSignItems'))
                BEGIN
                    EXEC('ALTER TABLE HiddenRoadSignItems ADD CONSTRAINT CK_HiddenRoadSignItems_Configured
                          CHECK (IsConfigured = 0 OR (QuestionText IS NOT NULL AND LEN(LTRIM(RTRIM(QuestionText))) > 0))');
                END";

            const string dropDisplayOrderSql = @"
                IF COL_LENGTH('HiddenRoadSignItems', 'DisplayOrder') IS NOT NULL
                BEGIN
                    DECLARE @tbl INT = OBJECT_ID('HiddenRoadSignItems');
                    DECLARE @col INT = COLUMNPROPERTY(@tbl, 'DisplayOrder', 'ColumnId');
                    DECLARE @sql NVARCHAR(MAX);

                    -- unique constraints / indexes that include DisplayOrder (the (set, DisplayOrder) unique key)
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

                    -- check constraints on DisplayOrder (the DisplayOrder > 0 check)
                    WHILE 1 = 1
                    BEGIN
                        SET @sql = NULL;
                        SELECT TOP 1 @sql = N'ALTER TABLE HiddenRoadSignItems DROP CONSTRAINT ' + QUOTENAME(name)
                        FROM sys.check_constraints
                        WHERE parent_object_id = @tbl AND (parent_column_id = @col OR definition LIKE N'%[[]DisplayOrder]%');
                        IF @sql IS NULL BREAK;
                        EXEC(@sql);
                    END

                    -- default constraints on DisplayOrder
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
                END";

            DatabaseHelper.ExecuteNonQuery(addIsConfiguredSql);
            DatabaseHelper.ExecuteNonQuery(questionNullableSql);
            DatabaseHelper.ExecuteNonQuery(configuredCheckSql);
            DatabaseHelper.ExecuteNonQuery(dropDisplayOrderSql);
        }

        // Phase 4: Learning module. Creation order respects foreign keys:
        // LearningChapters -> Lessons -> LearningProgress. No data is ever seeded.
        private static void EnsureLearningTablesExist()
        {
            const string chaptersSql = @"
                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'LearningChapters')
                BEGIN
                    CREATE TABLE LearningChapters
                    (
                        ChapterId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        Title NVARCHAR(150) NOT NULL,
                        Description NVARCHAR(1000) NOT NULL,
                        DisplayOrder INT NOT NULL,
                        IsActive BIT NOT NULL CONSTRAINT DF_LearningChapters_IsActive DEFAULT 1,
                        CreatedAt DATETIME2 NOT NULL,
                        UpdatedAt DATETIME2 NOT NULL,
                        CONSTRAINT UQ_LearningChapters_DisplayOrder UNIQUE (DisplayOrder)
                    );
                END";

            const string lessonsSql = @"
                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Lessons')
                BEGIN
                    CREATE TABLE Lessons
                    (
                        LessonId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        ChapterId INT NOT NULL,
                        Title NVARCHAR(200) NOT NULL,
                        Content NVARCHAR(MAX) NOT NULL,
                        ImagePath NVARCHAR(500) NULL,
                        DisplayOrder INT NOT NULL,
                        IsActive BIT NOT NULL CONSTRAINT DF_Lessons_IsActive DEFAULT 1,
                        CreatedAt DATETIME2 NOT NULL,
                        UpdatedAt DATETIME2 NOT NULL,
                        CONSTRAINT FK_Lessons_LearningChapters FOREIGN KEY (ChapterId) REFERENCES LearningChapters (ChapterId),
                        CONSTRAINT UQ_Lessons_Chapter_DisplayOrder UNIQUE (ChapterId, DisplayOrder)
                    );
                END";

            const string progressSql = @"
                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'LearningProgress')
                BEGIN
                    CREATE TABLE LearningProgress
                    (
                        LearningProgressId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        UserId INT NOT NULL,
                        LessonId INT NOT NULL,
                        CompletedAt DATETIME2 NOT NULL,
                        CONSTRAINT FK_LearningProgress_Users FOREIGN KEY (UserId) REFERENCES Users (UserId),
                        CONSTRAINT FK_LearningProgress_Lessons FOREIGN KEY (LessonId) REFERENCES Lessons (LessonId),
                        CONSTRAINT UQ_LearningProgress_User_Lesson UNIQUE (UserId, LessonId)
                    );
                END";

            DatabaseHelper.ExecuteNonQuery(chaptersSql);
            DatabaseHelper.ExecuteNonQuery(lessonsSql);
            DatabaseHelper.ExecuteNonQuery(progressSql);
        }

        // Phase 4 enhancement: lesson image position (Above/Below content). Added as a safe,
        // idempotent column upgrade so existing Lessons rows and their images are never touched;
        // the DEFAULT constraint backfills every existing row as 'Above', preserving current behavior.
        private static void EnsureLessonImagePositionColumnExists()
        {
            const string sql = @"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Lessons') AND name = 'ImagePosition')
                BEGIN
                    ALTER TABLE Lessons ADD ImagePosition NVARCHAR(20) NOT NULL CONSTRAINT DF_Lessons_ImagePosition DEFAULT 'Above';
                END";

            DatabaseHelper.ExecuteNonQuery(sql);
        }

        // The application database is identified by the configured connection string, which
        // attaches App_Data/MyKPPLearn.mdf by file path (LocalDB derives its own internal name).
        // A database is only created when that MDF genuinely does not exist, and never under
        // a fixed logical name, so another attached MDF called "MyKPPLearn" cannot collide.
        private static void EnsureDatabaseFileExists(string appDataPhysicalPath)
        {
            string mdfPath = Path.Combine(appDataPhysicalPath, "MyKPPLearn.mdf");

            if (File.Exists(mdfPath))
            {
                // Existing database: never create, overwrite or recreate it. Just verify the configured
                // connection works; any failure is surfaced as-is instead of being treated as "missing".
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                }

                return;
            }

            if (!Directory.Exists(appDataPhysicalPath))
            {
                Directory.CreateDirectory(appDataPhysicalPath);
            }

            CreateDatabaseFile(mdfPath);
        }

        // Creates the new MDF under a unique logical name, then detaches it so the normal
        // configured connection string attaches it by path. Only ever touches the file just created.
        private static void CreateDatabaseFile(string mdfPath)
        {
            const string masterConnectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;Integrated Security=True";
            string databaseName = "MyKPPLearn_" + Guid.NewGuid().ToString("N");

            using (SqlConnection connection = new SqlConnection(masterConnectionString))
            {
                connection.Open();

                string createDatabaseSql = string.Format(
                    "CREATE DATABASE [{0}] ON PRIMARY (NAME = [{0}], FILENAME = '{1}')",
                    databaseName,
                    mdfPath.Replace("'", "''"));

                using (SqlCommand command = new SqlCommand(createDatabaseSql, connection))
                {
                    command.ExecuteNonQuery();
                }

                using (SqlCommand command = new SqlCommand("EXEC sp_detach_db @dbname = @Name", connection))
                {
                    command.Parameters.AddWithValue("@Name", databaseName);
                    command.ExecuteNonQuery();
                }
            }

            SqlConnection.ClearAllPools();
        }

        private static void EnsureUsersTableExists()
        {
            const string sql = @"
                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Users')
                BEGIN
                    CREATE TABLE Users
                    (
                        UserId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        FullName NVARCHAR(150) NOT NULL,
                        Email NVARCHAR(254) NOT NULL,
                        PasswordHash NVARCHAR(256) NOT NULL,
                        Role NVARCHAR(20) NOT NULL,
                        ProfileImagePath NVARCHAR(500) NULL,
                        Status NVARCHAR(20) NOT NULL CONSTRAINT DF_Users_Status DEFAULT 'Active',
                        CreatedAt DATETIME2 NOT NULL,
                        UpdatedAt DATETIME2 NOT NULL,
                        CONSTRAINT UQ_Users_Email UNIQUE (Email),
                        CONSTRAINT CK_Users_Status CHECK (Status IN ('Active', 'Disabled'))
                    );
                END";

            DatabaseHelper.ExecuteNonQuery(sql);
        }

        // Profile picture: one nullable path column on the existing Users table (no separate Profiles table, no image data in
        // SQL Server). Existing users simply get NULL and keep showing their initials. Idempotent; never touches existing data.
        private static void EnsureProfileImagePathColumnExists()
        {
            const string sql = @"
                IF COL_LENGTH('Users', 'ProfileImagePath') IS NULL
                BEGIN
                    ALTER TABLE Users ADD ProfileImagePath NVARCHAR(500) NULL;
                END";

            DatabaseHelper.ExecuteNonQuery(sql);
        }
    }
}
