using System;
using System.Data;
using System.Data.SqlClient;

namespace MyKPPLearn.Data
{
    public enum HiddenRoadSignDeleteResult
    {
        Deleted,
        NotFound,
        /// <summary>Blocked because completed learner attempt history references the record.</summary>
        HasAttempts
    }

    /// <summary>
    /// Data access for Hidden Road Sign Game Sets (table HiddenRoadSignPractices). SetNumber is the
    /// only identifier an administrator types; the display name "Road Sign Game Set N" is
    /// generated from it by <see cref="FormatSetName"/>.
    /// </summary>
    public static class HiddenRoadSignPracticeRepository
    {
        private const int SqlDuplicateKey = 2601;
        private const int SqlUniqueConstraint = 2627;
        private const int SqlForeignKeyViolation = 547;

        public static string FormatSetName(object setNumber)
        {
            return "Road Sign Game Set " + Convert.ToInt32(setNumber);
        }

        private const string AdminSelect = @"
            SELECT p.HiddenRoadSignPracticeId, p.SetNumber, p.SceneImagePath, p.IsActive,
                   (SELECT COUNT(*) FROM HiddenRoadSignItems i WHERE i.HiddenRoadSignPracticeId = p.HiddenRoadSignPracticeId) AS ItemCount,
                   (SELECT COUNT(*) FROM HiddenRoadSignItems i WHERE i.HiddenRoadSignPracticeId = p.HiddenRoadSignPracticeId AND i.IsConfigured = 1) AS ConfiguredCount,
                   (SELECT COUNT(*) FROM HiddenRoadSignAttempts a WHERE a.HiddenRoadSignPracticeId = p.HiddenRoadSignPracticeId) AS AttemptCount,
                   (SELECT COUNT(DISTINCT a.UserId) FROM HiddenRoadSignAttempts a WHERE a.HiddenRoadSignPracticeId = p.HiddenRoadSignPracticeId) AS LearnerCount
            FROM HiddenRoadSignPractices p";

        public static DataTable GetAllForAdmin()
        {
            return DatabaseHelper.ExecuteQuery(AdminSelect + " ORDER BY p.SetNumber ASC");
        }

        public static DataRow GetById(int practiceId)
        {
            DataTable table = DatabaseHelper.ExecuteQuery(
                AdminSelect + " WHERE p.HiddenRoadSignPracticeId = @PracticeId",
                new SqlParameter("@PracticeId", practiceId));

            return table.Rows.Count == 0 ? null : table.Rows[0];
        }

        /// <summary>
        /// Sets a learner may start: Active and with at least one CONFIGURED Road Sign. Incomplete
        /// (Draft) Road Signs do not count, so a Set holding only Drafts is never offered.
        /// </summary>
        public static DataTable GetPlayableForLearner()
        {
            return DatabaseHelper.ExecuteQuery(
                AdminSelect + @"
                WHERE p.IsActive = 1
                  AND EXISTS (SELECT 1 FROM HiddenRoadSignItems i WHERE i.HiddenRoadSignPracticeId = p.HiddenRoadSignPracticeId AND i.IsConfigured = 1)
                ORDER BY p.SetNumber ASC");
        }

        /// <summary>The Set, only if it is Active and playable (has a configured Road Sign); otherwise null.</summary>
        public static DataRow GetPlayableById(int practiceId)
        {
            DataTable table = DatabaseHelper.ExecuteQuery(
                AdminSelect + @"
                WHERE p.HiddenRoadSignPracticeId = @PracticeId
                  AND p.IsActive = 1
                  AND EXISTS (SELECT 1 FROM HiddenRoadSignItems i WHERE i.HiddenRoadSignPracticeId = p.HiddenRoadSignPracticeId AND i.IsConfigured = 1)",
                new SqlParameter("@PracticeId", practiceId));

            return table.Rows.Count == 0 ? null : table.Rows[0];
        }

        /// <summary>The number the next new Game Set will get: the lowest one not in use (a deleted set's gap first).</summary>
        public static int NextFreeNumber()
        {
            return Convert.ToInt32(DatabaseHelper.ExecuteScalar(@"
                SELECT MIN(n.Number)
                FROM (SELECT 1 AS Number UNION ALL SELECT SetNumber + 1 FROM HiddenRoadSignPractices) n
                WHERE NOT EXISTS (SELECT 1 FROM HiddenRoadSignPractices p WHERE p.SetNumber = n.Number)"));
        }

        /// <summary>Returns the new HiddenRoadSignPracticeId, or 0 if the Set Number already exists.</summary>
        public static int Create(int setNumber, string sceneImagePath, bool isActive)
        {
            const string sql = @"
                INSERT INTO HiddenRoadSignPractices (SetNumber, SceneImagePath, IsActive, CreatedAt, UpdatedAt)
                OUTPUT INSERTED.HiddenRoadSignPracticeId
                VALUES (@SetNumber, @SceneImagePath, @IsActive, SYSUTCDATETIME(), SYSUTCDATETIME());";

            try
            {
                object result = DatabaseHelper.ExecuteScalar(sql,
                    new SqlParameter("@SetNumber", setNumber),
                    new SqlParameter("@SceneImagePath", sceneImagePath),
                    new SqlParameter("@IsActive", isActive));

                return Convert.ToInt32(result);
            }
            catch (SqlException ex) when (IsDuplicate(ex))
            {
                return 0;
            }
        }

        /// <param name="sceneImagePath">New Scene Image path, or null to keep the existing image.</param>
        /// <returns>False if the Set Number already belongs to another Set.</returns>
        public static bool Update(int practiceId, int setNumber, string sceneImagePath, bool isActive)
        {
            const string sql = @"
                UPDATE HiddenRoadSignPractices
                SET SetNumber = @SetNumber, SceneImagePath = ISNULL(@SceneImagePath, SceneImagePath),
                    IsActive = @IsActive, UpdatedAt = SYSUTCDATETIME()
                WHERE HiddenRoadSignPracticeId = @PracticeId";

            try
            {
                DatabaseHelper.ExecuteNonQuery(sql,
                    new SqlParameter("@PracticeId", practiceId),
                    new SqlParameter("@SetNumber", setNumber),
                    new SqlParameter("@SceneImagePath", (object)sceneImagePath ?? DBNull.Value),
                    new SqlParameter("@IsActive", isActive));

                return true;
            }
            catch (SqlException ex) when (IsDuplicate(ex))
            {
                return false;
            }
        }

        public static void SetActive(int practiceId, bool isActive)
        {
            DatabaseHelper.ExecuteNonQuery(
                "UPDATE HiddenRoadSignPractices SET IsActive = @IsActive, UpdatedAt = SYSUTCDATETIME() WHERE HiddenRoadSignPracticeId = @PracticeId",
                new SqlParameter("@PracticeId", practiceId),
                new SqlParameter("@IsActive", isActive));
        }

        public static bool HasAttempts(int practiceId)
        {
            object result = DatabaseHelper.ExecuteScalar(
                "SELECT COUNT(1) FROM HiddenRoadSignAttempts WHERE HiddenRoadSignPracticeId = @PracticeId",
                new SqlParameter("@PracticeId", practiceId));

            return Convert.ToInt32(result) > 0;
        }

        /// <summary>
        /// Deletes a never-attempted Set together with its hotspots and choices in ONE transaction.
        /// A Set with completed learner attempts is never deleted (history is preserved); the
        /// attempt check is part of the same transaction and the foreign keys are a second safety net.
        /// </summary>
        public static HiddenRoadSignDeleteResult Delete(int practiceId)
        {
            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        if (Scalar(connection, transaction,
                                "SELECT COUNT(1) FROM HiddenRoadSignPractices WHERE HiddenRoadSignPracticeId = @P", practiceId) == 0)
                        {
                            transaction.Rollback();
                            return HiddenRoadSignDeleteResult.NotFound;
                        }

                        // Learners' finished games of this set (and their answers) go with it; the admin was warned how many.
                        Execute(connection, transaction, @"
                            DELETE FROM HiddenRoadSignAttemptAnswers
                            WHERE HiddenRoadSignAttemptId IN (SELECT HiddenRoadSignAttemptId FROM HiddenRoadSignAttempts WHERE HiddenRoadSignPracticeId = @P)", practiceId);
                        Execute(connection, transaction,
                            "DELETE FROM HiddenRoadSignAttempts WHERE HiddenRoadSignPracticeId = @P", practiceId);

                        Execute(connection, transaction, @"
                            DELETE FROM HiddenRoadSignChoices
                            WHERE HiddenRoadSignItemId IN (SELECT HiddenRoadSignItemId FROM HiddenRoadSignItems WHERE HiddenRoadSignPracticeId = @P)", practiceId);
                        Execute(connection, transaction,
                            "DELETE FROM HiddenRoadSignItems WHERE HiddenRoadSignPracticeId = @P", practiceId);
                        Execute(connection, transaction,
                            "DELETE FROM HiddenRoadSignPractices WHERE HiddenRoadSignPracticeId = @P", practiceId);

                        transaction.Commit();
                        return HiddenRoadSignDeleteResult.Deleted;
                    }
                    catch (SqlException ex) when (ex.Number == SqlForeignKeyViolation)
                    {
                        transaction.Rollback();
                        return HiddenRoadSignDeleteResult.HasAttempts;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private static int Scalar(SqlConnection connection, SqlTransaction transaction, string sql, int practiceId)
        {
            using (SqlCommand command = new SqlCommand(sql, connection, transaction))
            {
                command.Parameters.AddWithValue("@P", practiceId);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        private static void Execute(SqlConnection connection, SqlTransaction transaction, string sql, int practiceId)
        {
            using (SqlCommand command = new SqlCommand(sql, connection, transaction))
            {
                command.Parameters.AddWithValue("@P", practiceId);
                command.ExecuteNonQuery();
            }
        }

        private static bool IsDuplicate(SqlException ex)
        {
            return ex.Number == SqlDuplicateKey || ex.Number == SqlUniqueConstraint;
        }
    }
}
