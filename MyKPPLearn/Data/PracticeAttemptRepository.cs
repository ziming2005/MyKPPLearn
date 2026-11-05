using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace MyKPPLearn.Data
{
    /// <summary>
    /// Learner Standard Practice attempts (also feeds Progress, the Learner Dashboard and Admin Learner Records).
    /// Tables: PracticeAttempts and PracticeAttemptQuestions, with Questions and QuestionOptions.
    /// A finished practice is marked again here against the database (the browser's score is never trusted) and saved
    /// with a snapshot of every question, so later Question Bank edits never change an old attempt.
    /// </summary>
    public static class PracticeAttemptRepository
    {
        private sealed class OptionRow
        {
            public int Id;
            public string Text;
            public bool IsCorrect;
        }

        private sealed class QuestionRow
        {
            public int Id;
            public string Text;
            public string Explanation;
            public string ImagePath;
            public OptionRow[] Options = new OptionRow[3];
        }

        /// <summary>
        /// Saves a finished practice of one Active set. questionIds are the questions in the order shown; answers holds
        /// 0-2 per question (A-C), or -1 when unanswered. Returns the new attempt id, or 0 when the submission does not
        /// match the set (unknown or repeated questions, wrong count) and nothing was saved.
        /// </summary>
        public static int Save(int userId, string categoryCode, int setNumber, DateTime startedUtc, IList<int> questionIds, IList<int> answers)
        {
            if (questionIds == null || answers == null || questionIds.Count == 0 || questionIds.Count != answers.Count
                || questionIds.Distinct().Count() != questionIds.Count)
            {
                return 0;
            }

            DataTable rows = DatabaseHelper.ExecuteQuery(@"
                SELECT p.PracticeSetId, q.QuestionId, q.QuestionText, q.Explanation, q.ImagePath,
                       o.QuestionOptionId, o.OptionOrder, o.OptionText, o.IsCorrect
                FROM PracticeSets p
                INNER JOIN QuestionCategories c ON c.QuestionCategoryId = p.QuestionCategoryId
                INNER JOIN Questions q ON q.PracticeSetId = p.PracticeSetId AND q.IsActive = 1
                INNER JOIN QuestionOptions o ON o.QuestionId = q.QuestionId
                WHERE c.Code = @Category AND p.SetNumber = @SetNumber AND p.IsActive = 1 AND c.IsActive = 1",
                new SqlParameter("@Category", categoryCode ?? string.Empty),
                new SqlParameter("@SetNumber", setNumber));

            if (rows.Rows.Count == 0)
            {
                return 0;
            }

            int setId = Convert.ToInt32(rows.Rows[0]["PracticeSetId"]);
            Dictionary<int, QuestionRow> questions = new Dictionary<int, QuestionRow>();

            foreach (DataRow row in rows.Rows)
            {
                int id = Convert.ToInt32(row["QuestionId"]);
                QuestionRow question;

                if (!questions.TryGetValue(id, out question))
                {
                    question = new QuestionRow
                    {
                        Id = id,
                        Text = Convert.ToString(row["QuestionText"]),
                        Explanation = Convert.ToString(row["Explanation"]),
                        ImagePath = row.IsNull("ImagePath") ? null : Convert.ToString(row["ImagePath"])
                    };
                    questions[id] = question;
                }

                int order = Convert.ToInt32(row["OptionOrder"]);

                if (order >= 1 && order <= 3)
                {
                    question.Options[order - 1] = new OptionRow
                    {
                        Id = Convert.ToInt32(row["QuestionOptionId"]),
                        Text = Convert.ToString(row["OptionText"]),
                        IsCorrect = Convert.ToBoolean(row["IsCorrect"])
                    };
                }
            }

            // The submission must be exactly this set's active questions.
            if (questionIds.Count != questions.Count || questionIds.Any(id => !questions.ContainsKey(id)))
            {
                return 0;
            }

            List<QuestionRow> paper = questionIds.Select(id => questions[id]).ToList();

            if (paper.Any(q => q.Options.Any(o => o == null) || q.Options.Count(o => o.IsCorrect) != 1))
            {
                return 0;
            }

            int correctCount = 0;
            int unanswered = 0;

            for (int i = 0; i < paper.Count; i++)
            {
                int answer = answers[i];

                if (answer < 0 || answer > 2) { unanswered++; }
                else if (paper[i].Options[answer].IsCorrect) { correctCount++; }
            }

            DateTime completedUtc = DateTime.UtcNow;

            if (startedUtc > completedUtc || startedUtc < completedUtc.AddHours(-12))
            {
                startedUtc = completedUtc;
            }

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        int attemptId;

                        using (SqlCommand command = new SqlCommand(@"
                            INSERT INTO PracticeAttempts (UserId, PracticeSetId, StartedAt, CompletedAt, TotalQuestions, CorrectCount, UnansweredCount)
                            OUTPUT INSERTED.PracticeAttemptId
                            VALUES (@UserId, @SetId, @Started, @Completed, @Total, @Correct, @Unanswered);", connection, transaction))
                        {
                            command.Parameters.AddWithValue("@UserId", userId);
                            command.Parameters.AddWithValue("@SetId", setId);
                            command.Parameters.AddWithValue("@Started", startedUtc);
                            command.Parameters.AddWithValue("@Completed", completedUtc);
                            command.Parameters.AddWithValue("@Total", paper.Count);
                            command.Parameters.AddWithValue("@Correct", correctCount);
                            command.Parameters.AddWithValue("@Unanswered", unanswered);
                            attemptId = Convert.ToInt32(command.ExecuteScalar());
                        }

                        for (int i = 0; i < paper.Count; i++)
                        {
                            QuestionRow q = paper[i];
                            int answer = answers[i];
                            bool answered = answer >= 0 && answer <= 2;
                            int correctIndex = Array.FindIndex(q.Options, o => o.IsCorrect);

                            using (SqlCommand command = new SqlCommand(@"
                                INSERT INTO PracticeAttemptQuestions
                                    (PracticeAttemptId, QuestionId, QuestionNumber, SelectedOptionId, IsCorrect,
                                     QuestionTextSnapshot, ExplanationSnapshot, OptionATextSnapshot, OptionBTextSnapshot, OptionCTextSnapshot,
                                     CorrectOptionSnapshot, SelectedOptionSnapshot, SelectedOptionTextSnapshot, ImagePathSnapshot)
                                VALUES (@AttemptId, @QuestionId, @Number, @SelectedId, @IsCorrect,
                                        @Text, @Explanation, @A, @B, @C, @Correct, @Selected, @SelectedText, @Image);", connection, transaction))
                            {
                                command.Parameters.AddWithValue("@AttemptId", attemptId);
                                command.Parameters.AddWithValue("@QuestionId", q.Id);
                                command.Parameters.AddWithValue("@Number", i + 1);
                                command.Parameters.AddWithValue("@SelectedId", answered ? (object)q.Options[answer].Id : DBNull.Value);
                                command.Parameters.AddWithValue("@IsCorrect", answered && answer == correctIndex);
                                command.Parameters.AddWithValue("@Text", q.Text);
                                command.Parameters.AddWithValue("@Explanation", q.Explanation);
                                command.Parameters.AddWithValue("@A", q.Options[0].Text);
                                command.Parameters.AddWithValue("@B", q.Options[1].Text);
                                command.Parameters.AddWithValue("@C", q.Options[2].Text);
                                command.Parameters.AddWithValue("@Correct", Letter(correctIndex));
                                command.Parameters.AddWithValue("@Selected", answered ? (object)Letter(answer) : DBNull.Value);
                                command.Parameters.AddWithValue("@SelectedText", answered ? (object)q.Options[answer].Text : DBNull.Value);
                                command.Parameters.AddWithValue("@Image", (object)q.ImagePath ?? DBNull.Value);
                                command.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        return attemptId;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// One saved attempt, one row per question in the order answered: the learner, the attempt's category code and
        /// set number plus the question snapshots. userId limits it to that learner's own attempts (null: any learner,
        /// for an administrator). Empty when there is no such attempt.
        /// </summary>
        public static DataTable GetSavedForUser(int attemptId, int? userId)
        {
            return DatabaseHelper.ExecuteQuery(@"
                SELECT a.UserId, c.Code AS CategoryCode, p.SetNumber, q.QuestionId, q.QuestionTextSnapshot, q.ExplanationSnapshot,
                       q.OptionATextSnapshot, q.OptionBTextSnapshot, q.OptionCTextSnapshot,
                       q.CorrectOptionSnapshot, q.SelectedOptionSnapshot, q.ImagePathSnapshot
                FROM PracticeAttempts a
                INNER JOIN PracticeSets p ON p.PracticeSetId = a.PracticeSetId
                INNER JOIN QuestionCategories c ON c.QuestionCategoryId = p.QuestionCategoryId
                INNER JOIN PracticeAttemptQuestions q ON q.PracticeAttemptId = a.PracticeAttemptId
                WHERE a.PracticeAttemptId = @AttemptId AND (@UserId IS NULL OR a.UserId = @UserId)
                ORDER BY q.QuestionNumber",
                new SqlParameter("@AttemptId", attemptId),
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId.HasValue ? (object)userId.Value : DBNull.Value });
        }

        private static string Letter(int index)
        {
            return ((char)('A' + index)).ToString();
        }
    }
}
