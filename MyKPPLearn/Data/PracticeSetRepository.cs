using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace MyKPPLearn.Data
{
    public enum PracticeSetDeleteResult { Deleted, NotFound, HasResults }

    /// <summary>
    /// Data access for Practice Sets (Admin Practice Sets and the Learner Practice pages).
    /// A set's display name ("Practice Set N") comes from SetNumber, which is unique per category and assigned
    /// automatically. A set's category and number never change after it is created.
    /// All queries are parameterized.
    /// </summary>
    public static class PracticeSetRepository
    {
        private const string SelectSql = @"
            SELECT p.PracticeSetId, p.QuestionCategoryId, p.SetNumber, p.IsActive,
                   c.Code AS CategoryCode, c.Title AS CategoryTitle,
                   (SELECT COUNT(*) FROM Questions q WHERE q.PracticeSetId = p.PracticeSetId) AS QuestionCount,
                   (SELECT COUNT(*) FROM Questions q WHERE q.PracticeSetId = p.PracticeSetId AND q.IsActive = 1) AS ActiveQuestionCount,
                   (SELECT COUNT(*) FROM PracticeAttempts a WHERE a.PracticeSetId = p.PracticeSetId) AS AttemptCount,
                   (SELECT COUNT(DISTINCT a.UserId) FROM PracticeAttempts a WHERE a.PracticeSetId = p.PracticeSetId) AS LearnerCount
            FROM PracticeSets p
            INNER JOIN QuestionCategories c ON c.QuestionCategoryId = p.QuestionCategoryId";

        public static DataTable GetAllForAdmin()
        {
            return DatabaseHelper.ExecuteQuery(SelectSql + " ORDER BY c.DisplayOrder, p.SetNumber");
        }

        /// <summary>Sets a learner may open: the set and its category are active and it has at least one active question.</summary>
        public static DataTable GetActiveForLearner()
        {
            return DatabaseHelper.ExecuteQuery(SelectSql + @"
                WHERE p.IsActive = 1 AND c.IsActive = 1
                  AND EXISTS (SELECT 1 FROM Questions q WHERE q.PracticeSetId = p.PracticeSetId AND q.IsActive = 1)
                ORDER BY c.DisplayOrder, p.SetNumber");
        }

        public static DataRow GetById(int id)
        {
            DataTable table = DatabaseHelper.ExecuteQuery(SelectSql + " WHERE p.PracticeSetId = @Id", new SqlParameter("@Id", id));
            return table.Rows.Count == 0 ? null : table.Rows[0];
        }

        /// <summary>
        /// Creates an Inactive set in the category with the lowest unused number (1, 2, 3...; a gap left by a deleted set
        /// is filled first). The number is chosen and inserted in one transaction, with UPDLOCK/HOLDLOCK on the category's
        /// sets, so two admins adding at the same moment get different numbers. A set can only be active with an active
        /// question; saving its first question in the Question Bank activates it (QuestionBank.Save).
        /// </summary>
        public static int Create(int categoryId)
        {
            const string sql = @"
                SET XACT_ABORT ON;
                BEGIN TRANSACTION;

                DECLARE @Number INT =
                    (SELECT MIN(n.Number)
                     FROM (SELECT 1 AS Number
                           UNION ALL
                           SELECT SetNumber + 1 FROM PracticeSets WITH (UPDLOCK, HOLDLOCK) WHERE QuestionCategoryId = @CategoryId) n
                     WHERE NOT EXISTS (SELECT 1 FROM PracticeSets p
                                       WHERE p.QuestionCategoryId = @CategoryId AND p.SetNumber = n.Number));

                INSERT INTO PracticeSets (QuestionCategoryId, SetNumber, IsActive, CreatedAt, UpdatedAt)
                OUTPUT INSERTED.PracticeSetId
                VALUES (@CategoryId, @Number, 0, SYSUTCDATETIME(), SYSUTCDATETIME());

                COMMIT TRANSACTION;";

            return Convert.ToInt32(DatabaseHelper.ExecuteScalar(sql, new SqlParameter("@CategoryId", categoryId)));
        }

        /// <summary>Activating requires at least one active question; returns false when that rule (or the id) fails.</summary>
        public static bool SetActive(int id, bool active)
        {
            if (active)
            {
                object activeQuestions = DatabaseHelper.ExecuteScalar(
                    "SELECT COUNT(*) FROM Questions WHERE PracticeSetId = @Id AND IsActive = 1",
                    new SqlParameter("@Id", id));

                if (Convert.ToInt32(activeQuestions) == 0)
                {
                    return false;
                }
            }

            return DatabaseHelper.ExecuteNonQuery(
                "UPDATE PracticeSets SET IsActive = @Active, UpdatedAt = SYSUTCDATETIME() WHERE PracticeSetId = @Id",
                new SqlParameter("@Id", id),
                new SqlParameter("@Active", active)) == 1;
        }

        /// <summary>
        /// Keeps the rule "an Active set has at least one active question": when a question change leaves an Active set
        /// with none, the set becomes Inactive. One atomic statement. Returns true when the set was deactivated.
        /// </summary>
        public static bool DeactivateIfNoActiveQuestions(int id)
        {
            const string sql = @"
                UPDATE PracticeSets
                SET IsActive = 0, UpdatedAt = SYSUTCDATETIME()
                WHERE PracticeSetId = @Id AND IsActive = 1
                  AND NOT EXISTS (SELECT 1 FROM Questions WHERE PracticeSetId = @Id AND IsActive = 1);";

            return DatabaseHelper.ExecuteNonQuery(sql, new SqlParameter("@Id", id)) == 1;
        }

        /// <summary>
        /// Deletes a set with its questions (taken out of Mock Exam Sets; see QuestionRepository.DeleteWhere), in one
        /// transaction. A set that learners have results for is never deleted (HasResults): it is deactivated instead, so
        /// their history stays. Returns the deleted questions' image paths (for the caller to remove once unused) and the
        /// Mock Exam Sets that became Inactive.
        /// </summary>
        public static PracticeSetDeleteResult Delete(int id, out List<string> imagePaths, out List<string> deactivatedMockSets)
        {
            imagePaths = new List<string>();
            deactivatedMockSets = new List<string>();

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        if (Count(connection, transaction, "SELECT COUNT(*) FROM PracticeSets WHERE PracticeSetId = @Id", id) == 0)
                        {
                            transaction.Rollback();
                            return PracticeSetDeleteResult.NotFound;
                        }

                        // Locked so no attempt can be saved between this check and the delete.
                        if (Count(connection, transaction, "SELECT COUNT(*) FROM PracticeAttempts WITH (UPDLOCK, HOLDLOCK) WHERE PracticeSetId = @Id", id) > 0)
                        {
                            transaction.Rollback();
                            return PracticeSetDeleteResult.HasResults;
                        }

                        using (SqlCommand command = new SqlCommand(
                            "SELECT ImagePath FROM Questions WHERE PracticeSetId = @Id AND ImagePath IS NOT NULL", connection, transaction))
                        {
                            command.Parameters.AddWithValue("@Id", id);

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read()) { imagePaths.Add(reader.GetString(0)); }
                            }
                        }

                        deactivatedMockSets = QuestionRepository.DeleteWhere(connection, transaction, "q.PracticeSetId = @Id", id);

                        using (SqlCommand command = new SqlCommand("DELETE FROM PracticeSets WHERE PracticeSetId = @Id", connection, transaction))
                        {
                            command.Parameters.AddWithValue("@Id", id);
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return PracticeSetDeleteResult.Deleted;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private static int Count(SqlConnection connection, SqlTransaction transaction, string sql, int id)
        {
            using (SqlCommand command = new SqlCommand(sql, connection, transaction))
            {
                command.Parameters.AddWithValue("@Id", id);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }
    }
}
