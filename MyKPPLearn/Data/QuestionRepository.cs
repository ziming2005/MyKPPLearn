using System;
using System.Data;
using System.Data.SqlClient;

namespace MyKPPLearn.Data
{
    /// <summary>One question as entered on the Question Bank form. CorrectOption is 1 to 3 (A to C).</summary>
    public sealed class QuestionInput
    {
        public int QuestionId;
        public int CategoryId;
        public int? PracticeSetId;
        public string QuestionText;
        public string Explanation;
        public string ImagePath;
        public string[] Options;
        public int CorrectOption;
    }

    public enum QuestionDeleteResult { Deleted, NotFound, HasAttempts, UsedByMockExam }

    /// <summary>
    /// Data access for the shared Question Bank (Standard Practice and Mock Exam) and each question's three options.
    /// A question with no Practice Set (PracticeSetId NULL) is available to Mock Exams only. All queries are parameterized.
    /// </summary>
    public static class QuestionRepository
    {
        public const int MaxQuestionLength = 500;
        public const int MaxOptionLength = 200;
        public const int MaxExplanationLength = 500;

        private const string SelectSql = @"
            SELECT q.QuestionId, q.QuestionCategoryId, q.PracticeSetId, q.QuestionText, q.Explanation,
                   q.ImagePath, q.IsActive,
                   c.Code AS CategoryCode, c.Title AS CategoryTitle, p.SetNumber,
                   (SELECT COUNT(*) FROM PracticeAttemptQuestions x WHERE x.QuestionId = q.QuestionId)
                 + (SELECT COUNT(*) FROM MockExamAttemptQuestions x WHERE x.QuestionId = q.QuestionId) AS AttemptCount,
                   (SELECT COUNT(*) FROM MockExamSetQuestions x WHERE x.QuestionId = q.QuestionId) AS MockExamUseCount
            FROM Questions q
            INNER JOIN QuestionCategories c ON c.QuestionCategoryId = q.QuestionCategoryId
            LEFT JOIN PracticeSets p ON p.PracticeSetId = q.PracticeSetId";

        public static DataTable GetAllForAdmin()
        {
            // By category, then set (mock-exam-only questions last), then the order the questions were added.
            return DatabaseHelper.ExecuteQuery(SelectSql + @"
                ORDER BY c.DisplayOrder, CASE WHEN p.SetNumber IS NULL THEN 1 ELSE 0 END, p.SetNumber, q.QuestionId");
        }

        public static DataRow GetById(int id)
        {
            DataTable table = DatabaseHelper.ExecuteQuery(SelectSql + " WHERE q.QuestionId = @Id", new SqlParameter("@Id", id));
            return table.Rows.Count == 0 ? null : table.Rows[0];
        }

        public static DataTable GetOptions(int id)
        {
            return DatabaseHelper.ExecuteQuery(
                "SELECT OptionOrder, OptionText, IsCorrect FROM QuestionOptions WHERE QuestionId = @Id ORDER BY OptionOrder",
                new SqlParameter("@Id", id));
        }

        /// <summary>All options of all questions in one query, for the admin list (avoids one query per question).</summary>
        public static DataTable GetAllOptions()
        {
            return DatabaseHelper.ExecuteQuery("SELECT QuestionId, OptionOrder, OptionText, IsCorrect FROM QuestionOptions ORDER BY QuestionId, OptionOrder");
        }

        /// <summary>
        /// Every active question in an active Practice Set of an active category, one row per option (three per question),
        /// in a single query. Mock-exam-only questions (no Practice Set) are left out.
        /// </summary>
        public static DataTable GetActiveForLearnerPractice()
        {
            return DatabaseHelper.ExecuteQuery(@"
                SELECT q.QuestionId, q.PracticeSetId, c.Code AS CategoryCode, q.QuestionText, q.Explanation,
                       q.ImagePath, o.OptionOrder, o.OptionText, o.IsCorrect
                FROM Questions q
                INNER JOIN PracticeSets p ON p.PracticeSetId = q.PracticeSetId
                INNER JOIN QuestionCategories c ON c.QuestionCategoryId = p.QuestionCategoryId
                INNER JOIN QuestionOptions o ON o.QuestionId = q.QuestionId
                WHERE q.IsActive = 1 AND p.IsActive = 1 AND c.IsActive = 1
                ORDER BY q.QuestionId, o.OptionOrder");
        }

        /// <summary>Server-side form validation. Returns null when valid, otherwise a message for the administrator.</summary>
        public static string Validate(QuestionInput input)
        {
            if (input == null || input.CategoryId < 1)
            {
                return "Select a category.";
            }

            if (string.IsNullOrWhiteSpace(input.QuestionText) || input.QuestionText.Trim().Length > MaxQuestionLength)
            {
                return "Question is required (up to 500 characters).";
            }

            if (string.IsNullOrWhiteSpace(input.Explanation) || input.Explanation.Trim().Length > MaxExplanationLength)
            {
                return "Explanation is required (up to 500 characters).";
            }

            if (input.Options == null || input.Options.Length != 3)
            {
                return "Exactly three answer options (A, B and C) are required.";
            }

            for (int i = 0; i < 3; i++)
            {
                if (string.IsNullOrWhiteSpace(input.Options[i]) || input.Options[i].Trim().Length > MaxOptionLength)
                {
                    return "Options A, B and C are required (up to 200 characters each).";
                }

                for (int j = 0; j < i; j++)
                {
                    if (string.Equals(input.Options[i].Trim(), input.Options[j].Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        return "Each option must be different.";
                    }
                }
            }

            if (input.CorrectOption < 1 || input.CorrectOption > 3)
            {
                return "Select exactly one correct answer.";
            }

            return null;
        }

        /// <summary>
        /// Inserts (QuestionId 0) or updates a question and its three options in one transaction.
        /// Throws ArgumentException with a friendly message when the input is invalid.
        /// </summary>
        public static int Save(QuestionInput input)
        {
            string error = Validate(input);

            if (error != null)
            {
                throw new ArgumentException(error);
            }

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        if (input.PracticeSetId.HasValue && !SetMatchesCategory(connection, transaction, input.PracticeSetId.Value, input.CategoryId))
                        {
                            throw new ArgumentException("The selected Practice Set does not belong to this category.");
                        }

                        int id = input.QuestionId;

                        if (id == 0)
                        {
                            const string insertSql = @"
                                INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, IsActive, CreatedAt, UpdatedAt)
                                OUTPUT INSERTED.QuestionId
                                VALUES (@CategoryId, @PracticeSetId, @Text, @Explanation, @ImagePath, 1, SYSUTCDATETIME(), SYSUTCDATETIME());";

                            using (SqlCommand command = new SqlCommand(insertSql, connection, transaction))
                            {
                                AddQuestionParameters(command, input);
                                id = Convert.ToInt32(command.ExecuteScalar());
                            }
                        }
                        else
                        {
                            const string updateSql = @"
                                UPDATE Questions
                                SET QuestionCategoryId = @CategoryId, PracticeSetId = @PracticeSetId, QuestionText = @Text, Explanation = @Explanation,
                                    ImagePath = @ImagePath, UpdatedAt = SYSUTCDATETIME()
                                WHERE QuestionId = @Id;";

                            using (SqlCommand command = new SqlCommand(updateSql, connection, transaction))
                            {
                                AddQuestionParameters(command, input);
                                command.Parameters.AddWithValue("@Id", id);

                                if (command.ExecuteNonQuery() != 1)
                                {
                                    throw new ArgumentException("Question not found.");
                                }
                            }
                        }

                        // Clear the old answer first: the database allows at most one correct option per question.
                        using (SqlCommand command = new SqlCommand("UPDATE QuestionOptions SET IsCorrect = 0 WHERE QuestionId = @Id", connection, transaction))
                        {
                            command.Parameters.AddWithValue("@Id", id);
                            command.ExecuteNonQuery();
                        }

                        const string optionSql = @"
                            IF EXISTS (SELECT 1 FROM QuestionOptions WHERE QuestionId = @Id AND OptionOrder = @Order)
                                UPDATE QuestionOptions SET OptionText = @Text, IsCorrect = @Correct WHERE QuestionId = @Id AND OptionOrder = @Order
                            ELSE
                                INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES (@Id, @Text, @Order, @Correct);";

                        for (int i = 0; i < 3; i++)
                        {
                            using (SqlCommand command = new SqlCommand(optionSql, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@Id", id);
                                command.Parameters.AddWithValue("@Order", i + 1);
                                command.Parameters.AddWithValue("@Text", input.Options[i].Trim());
                                command.Parameters.AddWithValue("@Correct", i + 1 == input.CorrectOption);
                                command.ExecuteNonQuery();
                            }
                        }

                        if (Count(connection, transaction, "SELECT COUNT(*) FROM QuestionOptions WHERE QuestionId = @Id", id) != 3
                            || Count(connection, transaction, "SELECT COUNT(*) FROM QuestionOptions WHERE QuestionId = @Id AND IsCorrect = 1", id) != 1)
                        {
                            throw new InvalidOperationException("The question must keep exactly three options and one correct answer.");
                        }

                        transaction.Commit();
                        return id;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public static bool SetActive(int id, bool active)
        {
            return DatabaseHelper.ExecuteNonQuery(
                "UPDATE Questions SET IsActive = @Active, UpdatedAt = SYSUTCDATETIME() WHERE QuestionId = @Id",
                new SqlParameter("@Id", id),
                new SqlParameter("@Active", active)) == 1;
        }

        /// <summary>
        /// Deletes a question and its options only when no learner attempt or Mock Exam Set uses it.
        /// The checks and the delete run in one transaction so nothing can start using it in between.
        /// </summary>
        public static QuestionDeleteResult Delete(int id)
        {
            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        if (Count(connection, transaction, "SELECT COUNT(*) FROM Questions WHERE QuestionId = @Id", id) == 0)
                        {
                            transaction.Rollback();
                            return QuestionDeleteResult.NotFound;
                        }

                        if (Count(connection, transaction, "SELECT COUNT(*) FROM PracticeAttemptQuestions WITH (UPDLOCK, HOLDLOCK) WHERE QuestionId = @Id", id) > 0
                            || Count(connection, transaction, "SELECT COUNT(*) FROM MockExamAttemptQuestions WITH (UPDLOCK, HOLDLOCK) WHERE QuestionId = @Id", id) > 0)
                        {
                            transaction.Rollback();
                            return QuestionDeleteResult.HasAttempts;
                        }

                        if (Count(connection, transaction, "SELECT COUNT(*) FROM MockExamSetQuestions WITH (UPDLOCK, HOLDLOCK) WHERE QuestionId = @Id", id) > 0)
                        {
                            transaction.Rollback();
                            return QuestionDeleteResult.UsedByMockExam;
                        }

                        using (SqlCommand command = new SqlCommand("DELETE FROM QuestionOptions WHERE QuestionId = @Id; DELETE FROM Questions WHERE QuestionId = @Id;", connection, transaction))
                        {
                            command.Parameters.AddWithValue("@Id", id);
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return QuestionDeleteResult.Deleted;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private static void AddQuestionParameters(SqlCommand command, QuestionInput input)
        {
            command.Parameters.AddWithValue("@CategoryId", input.CategoryId);
            command.Parameters.AddWithValue("@PracticeSetId", (object)input.PracticeSetId ?? DBNull.Value);
            command.Parameters.AddWithValue("@Text", input.QuestionText.Trim());
            command.Parameters.AddWithValue("@Explanation", input.Explanation.Trim());
            command.Parameters.AddWithValue("@ImagePath", (object)input.ImagePath ?? DBNull.Value);
        }

        private static bool SetMatchesCategory(SqlConnection connection, SqlTransaction transaction, int setId, int categoryId)
        {
            using (SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM PracticeSets WHERE PracticeSetId = @SetId AND QuestionCategoryId = @CategoryId", connection, transaction))
            {
                command.Parameters.AddWithValue("@SetId", setId);
                command.Parameters.AddWithValue("@CategoryId", categoryId);
                return Convert.ToInt32(command.ExecuteScalar()) == 1;
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
