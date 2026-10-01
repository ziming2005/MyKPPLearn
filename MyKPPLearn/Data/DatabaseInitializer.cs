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
            EnsureAdministratorSeed();
        }

        private static void EnsureDatabaseFileExists(string appDataPhysicalPath)
        {
            if (!Directory.Exists(appDataPhysicalPath))
            {
                Directory.CreateDirectory(appDataPhysicalPath);
            }

            string mdfPath = Path.Combine(appDataPhysicalPath, "MyKPPLearn.mdf");

            if (File.Exists(mdfPath))
            {
                return;
            }

            const string masterConnectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;Integrated Security=True";

            using (SqlConnection connection = new SqlConnection(masterConnectionString))
            {
                connection.Open();

                string createDatabaseSql = string.Format(
                    "CREATE DATABASE MyKPPLearn ON PRIMARY (NAME = MyKPPLearn, FILENAME = '{0}')",
                    mdfPath.Replace("'", "''"));

                using (SqlCommand command = new SqlCommand(createDatabaseSql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
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
                        CreatedAt DATETIME2 NOT NULL,
                        UpdatedAt DATETIME2 NOT NULL,
                        CONSTRAINT UQ_Users_Email UNIQUE (Email)
                    );
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
