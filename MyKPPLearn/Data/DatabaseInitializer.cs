using System;
using System.Data.SqlClient;
using System.IO;
using MyKPPLearn.Security;

namespace MyKPPLearn.Data
{
    /// <summary>
    /// Ensures the LocalDB database file, Users table, and the single development
    /// Administrator account exist. Safe to run on every application start:
    /// every step here is idempotent and never wipes existing data.
    /// </summary>
    public static class DatabaseInitializer
    {
        private const string SeedAdminEmail = "admin@gmail.com";
        private const string SeedAdminFullName = "Administrator";

        // PBKDF2-SHA256 hash of the project owner's development Administrator password.
        // Generated once with a throwaway console tool using PasswordHasher's exact
        // algorithm/parameters; the plaintext password itself is never stored here
        // or anywhere else in the application.
        private const string SeedAdminPasswordHash =
            "PBKDF2-SHA256$100000$L5CrWM0iDmfTRb6PmhilbA==$2bc4As/yNJ8uTqtFJmmIMUMoNLL7Z9szzlc3k+ZzFos=";

        public static void Initialize(string appDataPhysicalPath)
        {
            EnsureDatabaseFileExists(appDataPhysicalPath);
            EnsureUsersTableExists();
            EnsureProfileImagePathColumnExists();
            EnsureAdministratorSeed();
            EnsureLearningTablesExist();
            EnsureLessonImagePositionColumnExists();
            EnsureHiddenRoadSignTablesExist();
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
                        CreatedAt DATETIME2 NOT NULL,
                        UpdatedAt DATETIME2 NOT NULL,
                        CONSTRAINT UQ_Users_Email UNIQUE (Email)
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

        private static void EnsureAdministratorSeed()
        {
            if (UserRepository.EmailExists(SeedAdminEmail))
            {
                return;
            }

            const string sql = @"
                INSERT INTO Users (FullName, Email, PasswordHash, Role, CreatedAt, UpdatedAt)
                VALUES (@FullName, @Email, @PasswordHash, @Role, SYSUTCDATETIME(), SYSUTCDATETIME());";

            DatabaseHelper.ExecuteNonQuery(sql,
                new SqlParameter("@FullName", SeedAdminFullName),
                new SqlParameter("@Email", SeedAdminEmail),
                new SqlParameter("@PasswordHash", SeedAdminPasswordHash),
                new SqlParameter("@Role", SessionHelper.RoleAdministrator));
        }
    }
}
