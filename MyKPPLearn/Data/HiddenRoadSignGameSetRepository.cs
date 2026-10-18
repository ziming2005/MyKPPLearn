using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace MyKPPLearn.Data
{
    /// <summary>One road sign of the final editor state. ItemId is 0 for a road sign that does not exist in the database yet.</summary>
    public sealed class HiddenRoadSignEditorItem
    {
        public int ItemId;
        /// <summary>Used for NEW road signs only; an existing road sign keeps the asset it already has.</summary>
        public int RoadSignAssetId;
        public decimal X;
        public decimal Y;
        public decimal Width;
        public decimal Height;
        public string QuestionText;
        public string Explanation;
        /// <summary>Exactly three texts: A, B, C.</summary>
        public string[] ChoiceTexts;
        /// <summary>1 = A, 2 = B, 3 = C.</summary>
        public int CorrectChoice;
    }

    /// <summary>Outcome of the final Save. Error is null on success.</summary>
    public sealed class HiddenRoadSignSaveSetResult
    {
        public int PracticeId;
        public string Error;
        /// <summary>The Scene Image path that was replaced (only set when a new one was supplied); delete it AFTER the commit.</summary>
        public string ReplacedScenePath;
    }

    /// <summary>
    /// The single persistence point of the Add/Edit Game Set editor. One transaction writes the Game Set details
    /// and the complete road sign state (inserts, updates, removals and their A/B/C choices); any failure rolls
    /// everything back. Existing choice rows are updated IN PLACE so learner attempt history that points at them stays valid.
    /// Callers must have validated every item (HiddenRoadSignItemRepository.ValidateConfiguration / ValidateGeometry).
    /// </summary>
    public static class HiddenRoadSignGameSetRepository
    {
        public const string DuplicateSetMessage = "This set number already exists.";
        public const string ChangedMessage = "This game set was changed since the page was opened. Please reload the page and try again.";
        public const string HistoryMessage = "This Hidden Road Sign Game has learner attempt history, so a road sign that learners have answered cannot be removed. Keep it in the scene, or deactivate the whole set.";

        private const int SqlDuplicateKey = 2601;
        private const int SqlUniqueConstraint = 2627;
        private const int SqlForeignKeyViolation = 547;

        /// <param name="practiceId">0 to create a new Game Set, otherwise the Game Set being edited.</param>
        /// <param name="newScenePath">Required when creating; when editing, null keeps the current Scene Image.</param>
        /// <param name="removedItemIds">Existing road signs the administrator removed.</param>
        public static HiddenRoadSignSaveSetResult Save(
            int practiceId, int setNumber, bool isActive, string newScenePath,
            IList<HiddenRoadSignEditorItem> items, IList<int> removedItemIds)
        {
            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        HiddenRoadSignSaveSetResult result = new HiddenRoadSignSaveSetResult();
                        int id = practiceId;

                        if (practiceId == 0)
                        {
                            id = InsertSet(connection, transaction, setNumber, newScenePath, isActive);
                        }
                        else
                        {
                            object oldPath;
                            using (SqlCommand command = new SqlCommand(
                                "SELECT SceneImagePath FROM HiddenRoadSignPractices WITH (UPDLOCK, HOLDLOCK) WHERE HiddenRoadSignPracticeId = @P",
                                connection, transaction))
                            {
                                command.Parameters.AddWithValue("@P", practiceId);
                                oldPath = command.ExecuteScalar();
                            }

                            if (oldPath == null)
                            {
                                transaction.Rollback();
                                return Fail("That game set no longer exists.");
                            }

                            if (newScenePath != null)
                            {
                                result.ReplacedScenePath = Convert.ToString(oldPath);
                            }

                            UpdateSet(connection, transaction, practiceId, setNumber, newScenePath, isActive);
                        }

                        // Reconcile the submitted road signs with what is stored: every existing road sign must be
                        // either kept or explicitly removed, and nothing unknown may be referenced.
                        HashSet<int> existing = new HashSet<int>();
                        using (SqlCommand command = new SqlCommand(
                            "SELECT HiddenRoadSignItemId FROM HiddenRoadSignItems WITH (UPDLOCK) WHERE HiddenRoadSignPracticeId = @P",
                            connection, transaction))
                        {
                            command.Parameters.AddWithValue("@P", id);
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    existing.Add(reader.GetInt32(0));
                                }
                            }
                        }

                        List<int> kept = items.Where(i => i.ItemId > 0).Select(i => i.ItemId).ToList();
                        List<int> removed = removedItemIds.ToList();

                        if (kept.Count != new HashSet<int>(kept).Count
                            || removed.Count != new HashSet<int>(removed).Count
                            || kept.Any(removed.Contains)
                            || kept.Any(k => !existing.Contains(k))
                            || removed.Any(r => !existing.Contains(r))
                            || kept.Count + removed.Count != existing.Count)
                        {
                            transaction.Rollback();
                            return Fail(ChangedMessage);
                        }

                        foreach (int itemId in removed)
                        {
                            // History wins: a road sign that learners have answered is never deleted.
                            if (ScalarInt(connection, transaction,
                                    "SELECT COUNT(1) FROM HiddenRoadSignAttemptAnswers WITH (UPDLOCK, HOLDLOCK) WHERE HiddenRoadSignItemId = @I", itemId) > 0)
                            {
                                transaction.Rollback();
                                return Fail(HistoryMessage);
                            }

                            Execute(connection, transaction, "DELETE FROM HiddenRoadSignChoices WHERE HiddenRoadSignItemId = @I", itemId);
                            Execute(connection, transaction, "DELETE FROM HiddenRoadSignItems WHERE HiddenRoadSignItemId = @I", itemId);
                        }

                        foreach (HiddenRoadSignEditorItem item in items)
                        {
                            int itemId = item.ItemId;

                            if (itemId == 0)
                            {
                                itemId = InsertItem(connection, transaction, id, item);

                                if (itemId == 0)
                                {
                                    transaction.Rollback();
                                    return Fail("A road sign in the scene is no longer available. Remove it and choose another one.");
                                }
                            }
                            else
                            {
                                UpdateItem(connection, transaction, id, item);

                                // Clear the flags first so the "one correct choice" unique index is never
                                // violated while the new correct choice is being set.
                                Execute(connection, transaction, "UPDATE HiddenRoadSignChoices SET IsCorrect = 0 WHERE HiddenRoadSignItemId = @I", itemId);
                            }

                            for (int order = 1; order <= HiddenRoadSignItemRepository.ChoiceCount; order++)
                            {
                                HiddenRoadSignItemRepository.UpsertChoice(connection, transaction, itemId, order,
                                    item.ChoiceTexts[order - 1], order == item.CorrectChoice);
                            }
                        }

                        transaction.Commit();
                        result.PracticeId = id;
                        return result;
                    }
                    catch (SqlException ex) when (ex.Number == SqlDuplicateKey || ex.Number == SqlUniqueConstraint)
                    {
                        Rollback(transaction);
                        return Fail(DuplicateSetMessage);
                    }
                    catch (SqlException ex) when (ex.Number == SqlForeignKeyViolation)
                    {
                        Rollback(transaction);
                        return Fail(HistoryMessage);
                    }
                    catch
                    {
                        Rollback(transaction);
                        throw;
                    }
                }
            }
        }

        private static HiddenRoadSignSaveSetResult Fail(string message)
        {
            return new HiddenRoadSignSaveSetResult { Error = message };
        }

        private static void Rollback(SqlTransaction transaction)
        {
            try { transaction.Rollback(); } catch (InvalidOperationException) { }
        }

        private static int InsertSet(SqlConnection connection, SqlTransaction transaction, int setNumber, string scenePath, bool isActive)
        {
            using (SqlCommand command = new SqlCommand(@"
                INSERT INTO HiddenRoadSignPractices (SetNumber, SceneImagePath, IsActive, CreatedAt, UpdatedAt)
                OUTPUT INSERTED.HiddenRoadSignPracticeId
                VALUES (@SetNumber, @Scene, @IsActive, SYSUTCDATETIME(), SYSUTCDATETIME());", connection, transaction))
            {
                command.Parameters.AddWithValue("@SetNumber", setNumber);
                command.Parameters.AddWithValue("@Scene", scenePath);
                command.Parameters.AddWithValue("@IsActive", isActive);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        private static void UpdateSet(SqlConnection connection, SqlTransaction transaction, int practiceId, int setNumber, string scenePath, bool isActive)
        {
            using (SqlCommand command = new SqlCommand(@"
                UPDATE HiddenRoadSignPractices
                SET SetNumber = @SetNumber, SceneImagePath = ISNULL(@Scene, SceneImagePath),
                    IsActive = @IsActive, UpdatedAt = SYSUTCDATETIME()
                WHERE HiddenRoadSignPracticeId = @P", connection, transaction))
            {
                command.Parameters.AddWithValue("@P", practiceId);
                command.Parameters.AddWithValue("@SetNumber", setNumber);
                command.Parameters.AddWithValue("@Scene", (object)scenePath ?? DBNull.Value);
                command.Parameters.AddWithValue("@IsActive", isActive);
                command.ExecuteNonQuery();
            }
        }

        /// <summary>Inserts a new, fully configured road sign. Returns 0 when the asset does not exist or is not Active.</summary>
        private static int InsertItem(SqlConnection connection, SqlTransaction transaction, int practiceId, HiddenRoadSignEditorItem item)
        {
            using (SqlCommand command = new SqlCommand(@"
                INSERT INTO HiddenRoadSignItems
                    (HiddenRoadSignPracticeId, RoadSignAssetId, QuestionText, Explanation,
                     HotspotXPercent, HotspotYPercent, HotspotWidthPercent, HotspotHeightPercent, IsConfigured, CreatedAt, UpdatedAt)
                OUTPUT INSERTED.HiddenRoadSignItemId
                SELECT @P, a.RoadSignAssetId, @Question, @Explanation, @X, @Y, @W, @H, 1, SYSUTCDATETIME(), SYSUTCDATETIME()
                FROM HiddenRoadSignAssets a
                WHERE a.RoadSignAssetId = @AssetId AND a.IsActive = 1;", connection, transaction))
            {
                command.Parameters.AddWithValue("@P", practiceId);
                command.Parameters.AddWithValue("@AssetId", item.RoadSignAssetId);
                AddContent(command, item);

                object result = command.ExecuteScalar();
                return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
            }
        }

        /// <summary>Updates an existing road sign's content and geometry. Its asset never changes.</summary>
        private static void UpdateItem(SqlConnection connection, SqlTransaction transaction, int practiceId, HiddenRoadSignEditorItem item)
        {
            using (SqlCommand command = new SqlCommand(@"
                UPDATE HiddenRoadSignItems
                SET QuestionText = @Question, Explanation = @Explanation,
                    HotspotXPercent = @X, HotspotYPercent = @Y, HotspotWidthPercent = @W, HotspotHeightPercent = @H,
                    IsConfigured = 1, UpdatedAt = SYSUTCDATETIME()
                WHERE HiddenRoadSignItemId = @I AND HiddenRoadSignPracticeId = @P", connection, transaction))
            {
                command.Parameters.AddWithValue("@I", item.ItemId);
                command.Parameters.AddWithValue("@P", practiceId);
                AddContent(command, item);
                command.ExecuteNonQuery();
            }
        }

        private static void AddContent(SqlCommand command, HiddenRoadSignEditorItem item)
        {
            command.Parameters.AddWithValue("@Question", item.QuestionText.Trim());
            command.Parameters.AddWithValue("@Explanation", string.IsNullOrWhiteSpace(item.Explanation) ? (object)DBNull.Value : item.Explanation.Trim());
            command.Parameters.Add(HiddenRoadSignItemRepository.Percent("@X", item.X));
            command.Parameters.Add(HiddenRoadSignItemRepository.Percent("@Y", item.Y));
            command.Parameters.Add(HiddenRoadSignItemRepository.Percent("@W", item.Width));
            command.Parameters.Add(HiddenRoadSignItemRepository.Percent("@H", item.Height));
        }

        private static int ScalarInt(SqlConnection connection, SqlTransaction transaction, string sql, int id)
        {
            using (SqlCommand command = new SqlCommand(sql, connection, transaction))
            {
                command.Parameters.AddWithValue("@I", id);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        private static void Execute(SqlConnection connection, SqlTransaction transaction, string sql, int id)
        {
            using (SqlCommand command = new SqlCommand(sql, connection, transaction))
            {
                command.Parameters.AddWithValue("@I", id);
                command.ExecuteNonQuery();
            }
        }
    }
}
