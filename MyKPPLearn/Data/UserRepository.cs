using System;
using System.Data;
using System.Data.SqlClient;
using MyKPPLearn.Security;

namespace MyKPPLearn.Data
{
    /// <summary>
    /// Data access for the Users table. All queries are parameterized.
    /// Callers are expected to pass an already-normalized (trimmed, lowercased) email.
    /// </summary>
    public static class UserRepository
    {
        public static bool EmailExists(string normalizedEmail)
        {
            object result = DatabaseHelper.ExecuteScalar(
                "SELECT COUNT(1) FROM Users WHERE Email = @Email",
                new SqlParameter("@Email", normalizedEmail));

            return Convert.ToInt32(result) > 0;
        }

        public static int CreateLearner(string fullName, string normalizedEmail, string passwordHash)
        {
            const string sql = @"
                INSERT INTO Users (FullName, Email, PasswordHash, Role, CreatedAt, UpdatedAt)
                OUTPUT INSERTED.UserId
                VALUES (@FullName, @Email, @PasswordHash, @Role, SYSUTCDATETIME(), SYSUTCDATETIME());";

            object result = DatabaseHelper.ExecuteScalar(sql,
                new SqlParameter("@FullName", fullName),
                new SqlParameter("@Email", normalizedEmail),
                new SqlParameter("@PasswordHash", passwordHash),
                new SqlParameter("@Role", SessionHelper.RoleLearner));

            return Convert.ToInt32(result);
        }

        public static int GetLearnerCount()
        {
            object result = DatabaseHelper.ExecuteScalar(
                "SELECT COUNT(*) FROM Users WHERE Role = @Role",
                new SqlParameter("@Role", SessionHelper.RoleLearner));

            return Convert.ToInt32(result);
        }

        public static AppUser GetUserByEmail(string normalizedEmail)
        {
            return ReadUser(DatabaseHelper.ExecuteQuery(
                "SELECT UserId, FullName, Email, PasswordHash, Role, ProfileImagePath, Status FROM Users WHERE Email = @Email",
                new SqlParameter("@Email", normalizedEmail)));
        }

        public static AppUser GetUserByGoogleSubject(string googleSubjectId)
        {
            return ReadUser(DatabaseHelper.ExecuteQuery(
                "SELECT UserId, FullName, Email, PasswordHash, Role, ProfileImagePath, Status FROM Users WHERE GoogleSubjectId = @Sub",
                new SqlParameter("@Sub", googleSubjectId)));
        }

        /// <summary>Links an existing account to a Google account, unless it is already linked to another one.</summary>
        public static void LinkGoogleSubject(int userId, string googleSubjectId)
        {
            DatabaseHelper.ExecuteNonQuery(
                "UPDATE Users SET GoogleSubjectId = @Sub, UpdatedAt = SYSUTCDATETIME() WHERE UserId = @UserId AND GoogleSubjectId IS NULL",
                new SqlParameter("@Sub", googleSubjectId),
                new SqlParameter("@UserId", userId));
        }

        /// <summary>
        /// A learner created from a Google sign-in. The password hash is of a random value nobody knows,
        /// so the account can only be entered through Google until a password is set some other way.
        /// </summary>
        public static int CreateGoogleLearner(string fullName, string normalizedEmail, string googleSubjectId)
        {
            const string sql = @"
                INSERT INTO Users (FullName, Email, PasswordHash, Role, GoogleSubjectId, CreatedAt, UpdatedAt)
                OUTPUT INSERTED.UserId
                VALUES (@FullName, @Email, @PasswordHash, @Role, @Sub, SYSUTCDATETIME(), SYSUTCDATETIME());";

            object result = DatabaseHelper.ExecuteScalar(sql,
                new SqlParameter("@FullName", fullName),
                new SqlParameter("@Email", normalizedEmail),
                new SqlParameter("@PasswordHash", PasswordHasher.HashPassword(Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"))),
                new SqlParameter("@Role", SessionHelper.RoleLearner),
                new SqlParameter("@Sub", googleSubjectId));

            return Convert.ToInt32(result);
        }

        private static AppUser ReadUser(DataTable table)
        {
            if (table.Rows.Count == 0)
            {
                return null;
            }

            DataRow row = table.Rows[0];
            return new AppUser
            {
                UserId = Convert.ToInt32(row["UserId"]),
                FullName = row["FullName"].ToString(),
                Email = row["Email"].ToString(),
                PasswordHash = row["PasswordHash"].ToString(),
                Role = row["Role"].ToString(),
                ProfileImagePath = row["ProfileImagePath"] == DBNull.Value ? null : row["ProfileImagePath"].ToString(),
                Status = row["Status"].ToString()
            };
        }
    }
}
