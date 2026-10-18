using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace MyKPPLearn.Data
{
    /// <summary>One answered hotspot as reported by the browser. Nothing here is trusted.</summary>
    public sealed class HiddenRoadSignAnswerInput
    {
        public int ItemId;
        public int ChoiceId;
        public int FoundOrder;
    }

    /// <summary>Outcome of checking one answer mid-game (nothing is stored).</summary>
    public sealed class HiddenRoadSignAnswerCheck
    {
        public bool IsValid;
        public bool IsCorrect;
        public int CorrectChoiceId;
        public string Explanation;
    }

    /// <summary>Outcome of persisting a completed attempt. Error is null on success.</summary>
    public sealed class HiddenRoadSignSaveAttemptResult
    {
        public int AttemptId;
        public string Error;
    }

    /// <summary>
    /// Data access for Hidden Road Sign learner attempts. Attempts and their answers exist ONLY for
    /// completed games: <see cref="SaveCompletedAttempt"/> is the single place that inserts them,
    /// in one transaction, after the server re-validated everything and recomputed correctness
    /// from the stored configuration. <see cref="CheckAnswer"/> never writes anything.
    /// </summary>
    public static class HiddenRoadSignAttemptRepository
    {
        /// <summary>
        /// Checks a single answer against the stored configuration without saving anything.
        /// IsValid is false when the Set is not active, the Item is not part of the Set, or the
        /// choice does not belong to the Item.
        /// </summary>
        public static HiddenRoadSignAnswerCheck CheckAnswer(int practiceId, int itemId, int choiceId)
        {
            const string sql = @"
                SELECT c.HiddenRoadSignChoiceId, c.IsCorrect, i.Explanation
                FROM HiddenRoadSignItems i
                INNER JOIN HiddenRoadSignPractices p ON p.HiddenRoadSignPracticeId = i.HiddenRoadSignPracticeId
                INNER JOIN HiddenRoadSignChoices c ON c.HiddenRoadSignItemId = i.HiddenRoadSignItemId
                WHERE i.HiddenRoadSignItemId = @ItemId
                  AND i.IsConfigured = 1
                  AND p.HiddenRoadSignPracticeId = @PracticeId
                  AND p.IsActive = 1";

            DataTable rows = DatabaseHelper.ExecuteQuery(sql,
                new SqlParameter("@ItemId", itemId),
                new SqlParameter("@PracticeId", practiceId));

            HiddenRoadSignAnswerCheck check = new HiddenRoadSignAnswerCheck();
            bool selectedBelongsToItem = false;

            foreach (DataRow row in rows.Rows)
            {
                int id = Convert.ToInt32(row["HiddenRoadSignChoiceId"]);

                if (id == choiceId)
                {
                    selectedBelongsToItem = true;
                }

                if ((bool)row["IsCorrect"])
                {
                    check.CorrectChoiceId = id;
                }

                check.Explanation = row["Explanation"] as string;
            }

            check.IsValid = selectedBelongsToItem && check.CorrectChoiceId != 0;
            check.IsCorrect = check.IsValid && check.CorrectChoiceId == choiceId;
            return check;
        }

        /// <summary>
        /// Persists a COMPLETED game in one transaction (Attempt + every Answer, or nothing).
        /// Re-validates: the Set is still active; the answers cover every CONFIGURED Item (Drafts never count) exactly once;
        /// no unknown Items; every selected choice belongs to its Item; FoundOrder is 1..N without
        /// duplicates. IsCorrect, CorrectCount and TotalItems are recomputed here from the database.
        /// </summary>
        public static HiddenRoadSignSaveAttemptResult SaveCompletedAttempt(int userId, int practiceId, DateTime startedAtUtc, IList<HiddenRoadSignAnswerInput> answers)
        {
            const string changedMessage = "This game could not be saved because its content changed. Please start the game again.";

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Lock the Set row so it cannot be deleted/deactivated half-way through the save.
                        object active;
                        using (SqlCommand command = new SqlCommand(
                            "SELECT IsActive FROM HiddenRoadSignPractices WITH (UPDLOCK, HOLDLOCK) WHERE HiddenRoadSignPracticeId = @PracticeId",
                            connection, transaction))
                        {
                            command.Parameters.AddWithValue("@PracticeId", practiceId);
                            active = command.ExecuteScalar();
                        }

                        if (active == null || !(bool)active)
                        {
                            transaction.Rollback();
                            return Fail("This Hidden Road Sign Game is no longer available.");
                        }

                        List<int> itemIds = new List<int>();
                        using (SqlCommand command = new SqlCommand(
                            "SELECT HiddenRoadSignItemId FROM HiddenRoadSignItems WHERE HiddenRoadSignPracticeId = @PracticeId AND IsConfigured = 1",
                            connection, transaction))
                        {
                            command.Parameters.AddWithValue("@PracticeId", practiceId);
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    itemIds.Add(reader.GetInt32(0));
                                }
                            }
                        }

                        int total = itemIds.Count;

                        if (total == 0 || answers == null || answers.Count != total)
                        {
                            transaction.Rollback();
                            return Fail(changedMessage);
                        }

                        // Every configured Item exactly once, nothing unknown.
                        HashSet<int> expectedItems = new HashSet<int>(itemIds);
                        HashSet<int> seenItems = new HashSet<int>();
                        HashSet<int> seenOrders = new HashSet<int>();

                        foreach (HiddenRoadSignAnswerInput answer in answers)
                        {
                            if (!expectedItems.Contains(answer.ItemId) || !seenItems.Add(answer.ItemId))
                            {
                                transaction.Rollback();
                                return Fail(changedMessage);
                            }

                            if (answer.FoundOrder < 1 || answer.FoundOrder > total || !seenOrders.Add(answer.FoundOrder))
                            {
                                transaction.Rollback();
                                return Fail("This game could not be saved because it was not completed correctly.");
                            }
                        }

                        // choiceId -> (itemId, isCorrect), read from the database.
                        Dictionary<int, KeyValuePair<int, bool>> choices = new Dictionary<int, KeyValuePair<int, bool>>();
                        using (SqlCommand command = new SqlCommand(@"
                            SELECT c.HiddenRoadSignChoiceId, c.HiddenRoadSignItemId, c.IsCorrect
                            FROM HiddenRoadSignChoices c
                            INNER JOIN HiddenRoadSignItems i ON i.HiddenRoadSignItemId = c.HiddenRoadSignItemId
                            WHERE i.HiddenRoadSignPracticeId = @PracticeId AND i.IsConfigured = 1",
                            connection, transaction))
                        {
                            command.Parameters.AddWithValue("@PracticeId", practiceId);
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    choices[reader.GetInt32(0)] = new KeyValuePair<int, bool>(reader.GetInt32(1), reader.GetBoolean(2));
                                }
                            }
                        }

                        List<KeyValuePair<HiddenRoadSignAnswerInput, bool>> graded = new List<KeyValuePair<HiddenRoadSignAnswerInput, bool>>();
                        int correctCount = 0;

                        foreach (HiddenRoadSignAnswerInput answer in answers.OrderBy(a => a.FoundOrder))
                        {
                            KeyValuePair<int, bool> choice;

                            if (!choices.TryGetValue(answer.ChoiceId, out choice) || choice.Key != answer.ItemId)
                            {
                                transaction.Rollback();
                                return Fail("This game could not be saved because an answer was not valid.");
                            }

                            if (choice.Value)
                            {
                                correctCount++;
                            }

                            graded.Add(new KeyValuePair<HiddenRoadSignAnswerInput, bool>(answer, choice.Value));
                        }

                        int attemptId;
                        using (SqlCommand command = new SqlCommand(@"
                            INSERT INTO HiddenRoadSignAttempts (UserId, HiddenRoadSignPracticeId, StartedAt, CompletedAt, CorrectCount, TotalItems)
                            OUTPUT INSERTED.HiddenRoadSignAttemptId
                            VALUES (@UserId, @PracticeId, @StartedAt, SYSUTCDATETIME(), @CorrectCount, @TotalItems);",
                            connection, transaction))
                        {
                            command.Parameters.AddWithValue("@UserId", userId);
                            command.Parameters.AddWithValue("@PracticeId", practiceId);
                            command.Parameters.Add("@StartedAt", SqlDbType.DateTime2).Value = startedAtUtc;
                            command.Parameters.AddWithValue("@CorrectCount", correctCount);
                            command.Parameters.AddWithValue("@TotalItems", total);
                            attemptId = Convert.ToInt32(command.ExecuteScalar());
                        }

                        foreach (KeyValuePair<HiddenRoadSignAnswerInput, bool> entry in graded)
                        {
                            using (SqlCommand command = new SqlCommand(@"
                                INSERT INTO HiddenRoadSignAttemptAnswers (HiddenRoadSignAttemptId, HiddenRoadSignItemId, SelectedChoiceId, IsCorrect, FoundOrder)
                                VALUES (@AttemptId, @ItemId, @ChoiceId, @IsCorrect, @FoundOrder);",
                                connection, transaction))
                            {
                                command.Parameters.AddWithValue("@AttemptId", attemptId);
                                command.Parameters.AddWithValue("@ItemId", entry.Key.ItemId);
                                command.Parameters.AddWithValue("@ChoiceId", entry.Key.ChoiceId);
                                command.Parameters.AddWithValue("@IsCorrect", entry.Value);
                                command.Parameters.AddWithValue("@FoundOrder", entry.Key.FoundOrder);
                                command.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        return new HiddenRoadSignSaveAttemptResult { AttemptId = attemptId };
                    }
                    catch
                    {
                        // Any failure leaves no Attempt without its Answers (and no partial rows at all).
                        try { transaction.Rollback(); } catch (InvalidOperationException) { }
                        throw;
                    }
                }
            }
        }

        private static HiddenRoadSignSaveAttemptResult Fail(string message)
        {
            return new HiddenRoadSignSaveAttemptResult { Error = message };
        }

        // ------------------------------------------------------------------
        // Result / Review (always scoped to the owning learner)
        // ------------------------------------------------------------------

        /// <summary>Number of completed games the learner has (every stored attempt is a completed game).</summary>
        public static int GetCompletedCount(int userId)
        {
            object result = DatabaseHelper.ExecuteScalar(
                "SELECT COUNT(*) FROM HiddenRoadSignAttempts WHERE UserId = @UserId",
                new SqlParameter("@UserId", userId));

            return Convert.ToInt32(result);
        }

        /// <summary>
        /// The learner's completed games, most recent first, for the Progress page: attempt id, set number, score and date only
        /// (no answer rows). Historical: an attempt stays listed even if its Set was deactivated later.
        /// </summary>
        /// <summary>
        /// One row per Set the learner has finished: HiddenRoadSignPracticeId, BestCorrect, BestTotal (the best game by
        /// score ratio) and Plays (how many times it was finished).
        /// </summary>
        public static DataTable GetBestByPracticeForUser(int userId)
        {
            const string sql = @"
                SELECT x.HiddenRoadSignPracticeId, x.CorrectCount AS BestCorrect, x.TotalItems AS BestTotal, x.Plays
                FROM (
                    SELECT a.HiddenRoadSignPracticeId, a.CorrectCount, a.TotalItems,
                           ROW_NUMBER() OVER (PARTITION BY a.HiddenRoadSignPracticeId
                                              ORDER BY CAST(a.CorrectCount AS float) / NULLIF(a.TotalItems, 0) DESC, a.CompletedAt DESC) AS rn,
                           COUNT(*) OVER (PARTITION BY a.HiddenRoadSignPracticeId) AS Plays
                    FROM HiddenRoadSignAttempts a
                    WHERE a.UserId = @UserId
                ) x
                WHERE x.rn = 1";

            return DatabaseHelper.ExecuteQuery(sql, new SqlParameter("@UserId", userId));
        }

        public static DataTable GetHistoryForUser(int userId, int maxRows)
        {
            const string sql = @"
                SELECT TOP (@MaxRows) a.HiddenRoadSignAttemptId, p.SetNumber, a.CorrectCount, a.TotalItems, a.CompletedAt
                FROM HiddenRoadSignAttempts a
                INNER JOIN HiddenRoadSignPractices p ON p.HiddenRoadSignPracticeId = a.HiddenRoadSignPracticeId
                WHERE a.UserId = @UserId
                ORDER BY a.CompletedAt DESC, a.HiddenRoadSignAttemptId DESC";

            return DatabaseHelper.ExecuteQuery(sql,
                new SqlParameter("@UserId", userId),
                new SqlParameter("@MaxRows", maxRows));
        }

        /// <summary>The attempt, only if it belongs to <paramref name="userId"/>; otherwise null.</summary>
        public static DataRow GetAttemptForUser(int attemptId, int userId)
        {
            const string sql = @"
                SELECT a.HiddenRoadSignAttemptId, a.HiddenRoadSignPracticeId, p.SetNumber,
                       a.StartedAt, a.CompletedAt, a.CorrectCount, a.TotalItems
                FROM HiddenRoadSignAttempts a
                INNER JOIN HiddenRoadSignPractices p ON p.HiddenRoadSignPracticeId = a.HiddenRoadSignPracticeId
                WHERE a.HiddenRoadSignAttemptId = @AttemptId AND a.UserId = @UserId";

            DataTable table = DatabaseHelper.ExecuteQuery(sql,
                new SqlParameter("@AttemptId", attemptId),
                new SqlParameter("@UserId", userId));

            return table.Rows.Count == 0 ? null : table.Rows[0];
        }

        /// <summary>Review rows of the learner's own attempt, in the order the road signs were found.</summary>
        public static DataTable GetReviewRows(int attemptId, int userId)
        {
            const string sql = @"
                SELECT ans.FoundOrder, ans.IsCorrect,
                       i.QuestionText, i.Explanation,
                       r.Name AS AssetName, r.ImagePath AS AssetImagePath,
                       sc.ChoiceText AS SelectedText, sc.DisplayOrder AS SelectedOrder,
                       cc.ChoiceText AS CorrectText, cc.DisplayOrder AS CorrectOrder
                FROM HiddenRoadSignAttempts t
                INNER JOIN HiddenRoadSignAttemptAnswers ans ON ans.HiddenRoadSignAttemptId = t.HiddenRoadSignAttemptId
                INNER JOIN HiddenRoadSignItems i ON i.HiddenRoadSignItemId = ans.HiddenRoadSignItemId
                INNER JOIN HiddenRoadSignAssets r ON r.RoadSignAssetId = i.RoadSignAssetId
                INNER JOIN HiddenRoadSignChoices sc ON sc.HiddenRoadSignChoiceId = ans.SelectedChoiceId
                INNER JOIN HiddenRoadSignChoices cc ON cc.HiddenRoadSignItemId = i.HiddenRoadSignItemId AND cc.IsCorrect = 1
                WHERE t.HiddenRoadSignAttemptId = @AttemptId AND t.UserId = @UserId
                ORDER BY ans.FoundOrder ASC";

            return DatabaseHelper.ExecuteQuery(sql,
                new SqlParameter("@AttemptId", attemptId),
                new SqlParameter("@UserId", userId));
        }
    }
}
