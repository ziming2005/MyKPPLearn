using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace MyKPPLearn.Data
{
    /// <summary>
    /// Forgot-password one-time codes (6 digits, emailed). The table keeps only a SHA-256 hash of
    /// "userId:code". A code works once, for <see cref="Lifetime"/>, allows <see cref="MaxAttempts"/>
    /// wrong tries, and a newer code replaces any older one.
    /// </summary>
    public static class PasswordResetRepository
    {
        public static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(10);

        public const int MaxAttempts = 5;

        // Requests closer together than this send no new email (stops the form being used to flood an inbox).
        private static readonly TimeSpan MinimumGap = TimeSpan.FromMinutes(1);

        /// <summary>Creates a code for the user and returns it, or null if one was issued under a minute ago.</summary>
        public static string CreateCode(int userId)
        {
            string code = RandomCode();

            const string sql = @"
                IF EXISTS (SELECT 1 FROM PasswordResetTokens WHERE UserId = @UserId AND CreatedAt > DATEADD(SECOND, -@GapSeconds, SYSUTCDATETIME()))
                    SELECT 0;
                ELSE
                BEGIN
                    UPDATE PasswordResetTokens SET UsedAt = SYSUTCDATETIME() WHERE UserId = @UserId AND UsedAt IS NULL;
                    INSERT INTO PasswordResetTokens (UserId, TokenHash, ExpiresAt, CreatedAt)
                    VALUES (@UserId, @TokenHash, DATEADD(MINUTE, @LifetimeMinutes, SYSUTCDATETIME()), SYSUTCDATETIME());
                    SELECT 1;
                END";

            object created = DatabaseHelper.ExecuteScalar(sql,
                new SqlParameter("@UserId", userId),
                new SqlParameter("@TokenHash", Hash(userId, code)),
                new SqlParameter("@GapSeconds", (int)MinimumGap.TotalSeconds),
                new SqlParameter("@LifetimeMinutes", (int)Lifetime.TotalMinutes));

            return Convert.ToInt32(created) == 1 ? code : null;
        }

        /// <summary>
        /// Step 2: checks the code against the user's current one. On a match the code is marked verified and
        /// given a fresh <see cref="Lifetime"/> to choose the new password; its id is returned. A wrong code
        /// counts as a failed try (after <see cref="MaxAttempts"/> the code stops working) and returns null.
        /// </summary>
        public static int? VerifyCode(int userId, string code)
        {
            if (string.IsNullOrEmpty(code) || code.Length != 6)
            {
                RecordFailure(userId);
                return null;
            }

            const string sql = @"
                DECLARE @TokenId INT;
                UPDATE TOP (1) t SET VerifiedAt = SYSUTCDATETIME(), @TokenId = t.TokenId,
                        ExpiresAt = DATEADD(MINUTE, @LifetimeMinutes, SYSUTCDATETIME())
                    FROM PasswordResetTokens t INNER JOIN Users u ON u.UserId = t.UserId
                    WHERE t.UserId = @UserId AND t.TokenHash = @TokenHash AND t.UsedAt IS NULL
                      AND t.ExpiresAt > SYSUTCDATETIME() AND t.FailedAttempts < @MaxAttempts AND u.Status = 'Active';
                SELECT @TokenId;";

            object result = DatabaseHelper.ExecuteScalar(sql,
                new SqlParameter("@UserId", userId),
                new SqlParameter("@TokenHash", Hash(userId, code)),
                new SqlParameter("@MaxAttempts", MaxAttempts),
                new SqlParameter("@LifetimeMinutes", (int)Lifetime.TotalMinutes));

            if (result != null && result != DBNull.Value)
            {
                return Convert.ToInt32(result);
            }

            RecordFailure(userId);
            return null;
        }

        /// <summary>
        /// Step 3: sets the new password with a code verified in step 2, and uses the code up, in one statement
        /// so it can never reset twice. Returns false if the code has since expired, been used or replaced.
        /// </summary>
        public static bool ResetPassword(int userId, int tokenId, string newPasswordHash)
        {
            const string sql = @"
                SET XACT_ABORT ON;
                BEGIN TRANSACTION;
                UPDATE t SET UsedAt = SYSUTCDATETIME()
                    FROM PasswordResetTokens t INNER JOIN Users u ON u.UserId = t.UserId
                    WHERE t.TokenId = @TokenId AND t.UserId = @UserId AND t.VerifiedAt IS NOT NULL AND t.UsedAt IS NULL
                      AND t.ExpiresAt > SYSUTCDATETIME() AND u.Status = 'Active';
                DECLARE @Done INT = @@ROWCOUNT;
                IF @Done = 1
                    UPDATE Users SET PasswordHash = @PasswordHash, UpdatedAt = SYSUTCDATETIME() WHERE UserId = @UserId;
                COMMIT TRANSACTION;
                SELECT @Done;";

            object result = DatabaseHelper.ExecuteScalar(sql,
                new SqlParameter("@TokenId", tokenId),
                new SqlParameter("@UserId", userId),
                new SqlParameter("@PasswordHash", newPasswordHash));

            return Convert.ToInt32(result) == 1;
        }

        private static void RecordFailure(int userId)
        {
            DatabaseHelper.ExecuteNonQuery(
                "UPDATE PasswordResetTokens SET FailedAttempts = FailedAttempts + 1 WHERE UserId = @UserId AND UsedAt IS NULL",
                new SqlParameter("@UserId", userId));
        }

        private static string RandomCode()
        {
            byte[] bytes = new byte[4];

            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            // 0..999999, zero-padded ("004215"). The bias from 2^32 mod 10^6 is negligible.
            return (BitConverter.ToUInt32(bytes, 0) % 1000000).ToString("D6");
        }

        private static string Hash(int userId, string code)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] digest = sha.ComputeHash(Encoding.UTF8.GetBytes(userId + ":" + code));
                StringBuilder hex = new StringBuilder(64);

                foreach (byte b in digest)
                {
                    hex.Append(b.ToString("x2"));
                }

                return hex.ToString();
            }
        }
    }
}
