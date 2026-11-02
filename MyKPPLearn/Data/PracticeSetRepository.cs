using System;
using System.Data;
using System.Data.SqlClient;

namespace MyKPPLearn.Data
{
    public enum PracticeSetDeleteResult { Deleted, NotFound, HasQuestions, HasAttempts }

    public enum PracticeSetUpdateResult { Updated, NotFound, CategoryLocked }

    /// <summary>
    /// Data access for Practice Sets (Admin Practice Sets and the Learner Practice pages).
    /// A set's display name ("Practice Set N") comes from SetNumber, which is unique per category.
    /// All queries are parameterized.
    /// </summary>
    public static class PracticeSetRepository
    {
        private const string SelectSql = @"
            SELECT p.PracticeSetId, p.QuestionCategoryId, p.SetNumber, p.IsActive,
                   c.Code AS CategoryCode, c.Title AS CategoryTitle,
                   (SELECT COUNT(*) FROM Questions q WHERE q.PracticeSetId = p.PracticeSetId) AS QuestionCount,
                   (SELECT COUNT(*) FROM Questions q WHERE q.PracticeSetId = p.PracticeSetId AND q.IsActive = 1) AS ActiveQuestionCount,
                   (SELECT COUNT(*) FROM PracticeAttempts a WHERE a.PracticeSetId = p.PracticeSetId) AS AttemptCount
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

        /// <summary>New sets start Inactive: a set can only be activated once it has an active question.</summary>
        public static int Create(int categoryId, int setNumber)
        {
            const string sql = @"
                INSERT INTO PracticeSets (QuestionCategoryId, SetNumber, IsActive, CreatedAt, UpdatedAt)
                OUTPUT INSERTED.PracticeSetId
                VALUES (@CategoryId, @SetNumber, 0, SYSUTCDATETIME(), SYSUTCDATETIME());";

            return Convert.ToInt32(DatabaseHelper.ExecuteScalar(sql,
                new SqlParameter("@CategoryId", categoryId),
                new SqlParameter("@SetNumber", setNumber)));
        }

        /// <summary>
        /// Changes a set's number and category. The category cannot change while the set has questions,
        /// because each question's category must match its set's category (enforced by a composite foreign key).
        /// </summary>
        public static PracticeSetUpdateResult Update(int id, int categoryId, int setNumber)
        {
            DataRow current = GetById(id);

            if (current == null)
            {
                return PracticeSetUpdateResult.NotFound;
            }

            if (Convert.ToInt32(current["QuestionCategoryId"]) != categoryId && Convert.ToInt32(current["QuestionCount"]) > 0)
            {
                return PracticeSetUpdateResult.CategoryLocked;
            }

            const string sql = @"
                UPDATE PracticeSets
                SET QuestionCategoryId = @CategoryId, SetNumber = @SetNumber, UpdatedAt = SYSUTCDATETIME()
                WHERE PracticeSetId = @Id;";

            int rows = DatabaseHelper.ExecuteNonQuery(sql,
                new SqlParameter("@Id", id),
                new SqlParameter("@CategoryId", categoryId),
                new SqlParameter("@SetNumber", setNumber));

            return rows == 1 ? PracticeSetUpdateResult.Updated : PracticeSetUpdateResult.NotFound;
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
        /// Deletes a set only when nothing depends on it. The checks and the delete run in one transaction,
        /// with UPDLOCK/HOLDLOCK so a question or attempt cannot be added between the check and the delete.
        /// </summary>
        public static PracticeSetDeleteResult Delete(int id)
        {
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

                        if (Count(connection, transaction, "SELECT COUNT(*) FROM PracticeAttempts WITH (UPDLOCK, HOLDLOCK) WHERE PracticeSetId = @Id", id) > 0)
                        {
                            transaction.Rollback();
                            return PracticeSetDeleteResult.HasAttempts;
                        }

                        if (Count(connection, transaction, "SELECT COUNT(*) FROM Questions WITH (UPDLOCK, HOLDLOCK) WHERE PracticeSetId = @Id", id) > 0)
                        {
                            transaction.Rollback();
                            return PracticeSetDeleteResult.HasQuestions;
                        }

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
