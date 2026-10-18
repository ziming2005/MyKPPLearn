using System;
using System.Data;
using System.Data.SqlClient;

namespace MyKPPLearn.Data
{
    /// <summary>
    /// Profile access to the existing Users table, always scoped to ONE user id taken from the session by the caller
    /// (never from the query string). All SQL is parameterized. The password hash is read only by the password-change path.
    /// </summary>
    public static class UserProfileRepository
    {
        public static UserProfile GetProfile(int userId)
        {
            DataTable table = DatabaseHelper.ExecuteQuery(
                "SELECT UserId, FullName, Email, Role, ProfileImagePath, CreatedAt, UpdatedAt FROM Users WHERE UserId = @UserId",
                new SqlParameter("@UserId", userId));

            if (table.Rows.Count == 0)
            {
                return null;
            }

            DataRow row = table.Rows[0];

            return new UserProfile
            {
                UserId = Convert.ToInt32(row["UserId"]),
                FullName = Convert.ToString(row["FullName"]),
                Email = Convert.ToString(row["Email"]),
                Role = Convert.ToString(row["Role"]),
                ProfileImagePath = row["ProfileImagePath"] == DBNull.Value ? null : Convert.ToString(row["ProfileImagePath"]),
                CreatedAt = Convert.ToDateTime(row["CreatedAt"]),
                UpdatedAt = Convert.ToDateTime(row["UpdatedAt"])
            };
        }

        /// <summary>Updates ONLY the Full Name (and UpdatedAt). Email, Role, picture and password are never touched.</summary>
        public static bool UpdateFullName(int userId, string fullName)
        {
            int affected = DatabaseHelper.ExecuteNonQuery(
                "UPDATE Users SET FullName = @FullName, UpdatedAt = SYSUTCDATETIME() WHERE UserId = @UserId",
                new SqlParameter("@UserId", userId),
                new SqlParameter("@FullName", fullName));

            return affected > 0;
        }

        /// <summary>
        /// Replaces the profile picture path (and UpdatedAt) with a NEW, non-empty path. Returns the previously stored path so
        /// the caller can delete the old file after this has succeeded.
        /// </summary>
        public static ProfileUpdateResult ReplaceProfileImage(int userId, string newImagePath)
        {
            if (string.IsNullOrEmpty(newImagePath))
            {
                throw new ArgumentException("A new profile image path is required.", "newImagePath");
            }

            DataTable table = DatabaseHelper.ExecuteQuery(@"
                UPDATE Users
                SET ProfileImagePath = @ImagePath, UpdatedAt = SYSUTCDATETIME()
                OUTPUT deleted.ProfileImagePath
                WHERE UserId = @UserId",
                new SqlParameter("@UserId", userId),
                new SqlParameter("@ImagePath", newImagePath));

            ProfileUpdateResult result = new ProfileUpdateResult();

            if (table.Rows.Count > 0)
            {
                result.Found = true;
                result.PreviousImagePath = table.Rows[0][0] == DBNull.Value ? null : Convert.ToString(table.Rows[0][0]);
            }

            return result;
        }

        /// <summary>
        /// Removes the profile picture (the avatar goes back to the initials). Returns the previously stored path so the caller
        /// can delete the file after this has succeeded.
        /// </summary>
        public static ProfileUpdateResult ClearProfileImage(int userId)
        {
            DataTable table = DatabaseHelper.ExecuteQuery(@"
                UPDATE Users
                SET ProfileImagePath = NULL, UpdatedAt = SYSUTCDATETIME()
                OUTPUT deleted.ProfileImagePath
                WHERE UserId = @UserId",
                new SqlParameter("@UserId", userId));

            ProfileUpdateResult result = new ProfileUpdateResult();

            if (table.Rows.Count > 0)
            {
                result.Found = true;
                result.PreviousImagePath = table.Rows[0][0] == DBNull.Value ? null : Convert.ToString(table.Rows[0][0]);
            }

            return result;
        }

        /// <summary>Only for the password-change path. Returns null when the user does not exist.</summary>
        public static string GetPasswordHash(int userId)
        {
            object result = DatabaseHelper.ExecuteScalar(
                "SELECT PasswordHash FROM Users WHERE UserId = @UserId",
                new SqlParameter("@UserId", userId));

            return result == null || result == DBNull.Value ? null : Convert.ToString(result);
        }

        public static bool UpdatePasswordHash(int userId, string passwordHash)
        {
            int affected = DatabaseHelper.ExecuteNonQuery(
                "UPDATE Users SET PasswordHash = @PasswordHash, UpdatedAt = SYSUTCDATETIME() WHERE UserId = @UserId",
                new SqlParameter("@UserId", userId),
                new SqlParameter("@PasswordHash", passwordHash));

            return affected > 0;
        }
    }
}
