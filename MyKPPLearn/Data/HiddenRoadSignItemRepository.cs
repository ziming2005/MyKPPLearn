using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace MyKPPLearn.Data
{
    /// <summary>
    /// The educational content of one placed Road Sign: question, explanation and its three choices.
    /// Position and size are not part of it; they travel in HiddenRoadSignEditorItem with the final Save.
    /// </summary>
    public sealed class HiddenRoadSignItemInput
    {
        public int ItemId;
        public int PracticeId;
        public string QuestionText;
        public string Explanation;
        /// <summary>Exactly three texts: A, B, C.</summary>
        public string[] ChoiceTexts;
        /// <summary>1 = A, 2 = B, 3 = C.</summary>
        public int CorrectChoice;
    }

    public enum HiddenRoadSignSaveResult
    {
        Saved,
        NotFound
    }

    /// <summary>
    /// Data access for the Road Signs placed in a Game Set (HiddenRoadSignItems) and their A/B/C choices
    /// (HiddenRoadSignChoices).
    /// Reads, validation rules and the shared choice/geometry helpers. Road signs are written only by the single
    /// final Save of a Game Set (HiddenRoadSignGameSetRepository), and every road sign saved that way is
    /// IsConfigured = 1. Older Draft rows (IsConfigured = 0) may still exist; learners only ever see configured ones.
    /// Choices are updated in place (never deleted and re-inserted) so their ids, which learner
    /// history references, stay stable when an Item is edited.
    /// </summary>
    public static class HiddenRoadSignItemRepository
    {
        public const int ChoiceCount = 3;
        // Same limits as the Question Bank and Mock Exam questions.
        public const int MaxQuestionLength = 500;
        public const int MaxChoiceLength = 200;
        public const int MaxExplanationLength = 500;

        private const int SqlForeignKeyViolation = 547;

        public static string ChoiceLetter(object displayOrder)
        {
            int order = Convert.ToInt32(displayOrder);
            return order >= 1 && order <= ChoiceCount ? ((char)('A' + order - 1)).ToString() : "?";
        }

        /// <summary>
        /// Server-side validation of a Road Sign's question content. Returns null when valid, otherwise a
        /// friendly message. A Road Sign is only ever marked configured when this passes completely.
        /// </summary>
        public static string ValidateConfiguration(HiddenRoadSignItemInput input)
        {
            if (string.IsNullOrWhiteSpace(input.QuestionText) || input.QuestionText.Trim().Length > MaxQuestionLength)
            {
                return "Question is required (up to " + MaxQuestionLength + " characters).";
            }

            if (input.ChoiceTexts == null || input.ChoiceTexts.Length != ChoiceCount)
            {
                return "Exactly three answer options (A, B and C) are required.";
            }

            foreach (string text in input.ChoiceTexts)
            {
                if (string.IsNullOrWhiteSpace(text) || text.Trim().Length > MaxChoiceLength)
                {
                    return "Options A, B and C are all required (up to " + MaxChoiceLength + " characters each).";
                }
            }

            for (int i = 0; i < input.ChoiceTexts.Length; i++)
            {
                for (int j = i + 1; j < input.ChoiceTexts.Length; j++)
                {
                    if (string.Equals(input.ChoiceTexts[i].Trim(), input.ChoiceTexts[j].Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        return "Option " + (char)('A' + j) + " repeats Option " + (char)('A' + i) + ". Each option must be different.";
                    }
                }
            }

            if (input.CorrectChoice < 1 || input.CorrectChoice > ChoiceCount)
            {
                return "Please select exactly one correct answer (A, B or C).";
            }

            if (string.IsNullOrWhiteSpace(input.Explanation) || input.Explanation.Trim().Length > MaxExplanationLength)
            {
                return "Explanation is required (up to " + MaxExplanationLength + " characters).";
            }

            return null;
        }

        /// <summary>Geometry is in percent of the Scene Image and must stay completely inside it.</summary>
        public static string ValidateGeometry(decimal x, decimal y, decimal width, decimal height)
        {
            if (x < 0 || y < 0 || width <= 0 || height <= 0 || x + width > 100 || y + height > 100)
            {
                return "The road sign must be placed inside the Scene Image.";
            }

            return null;
        }

        /// <summary>
        /// Parses a percentage posted by the editor (invariant culture, two decimals). Returns false
        /// for anything that is not a finite number.
        /// </summary>
        public static bool TryParsePercent(string text, out decimal value)
        {
            if (decimal.TryParse((text ?? string.Empty).Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out value))
            {
                value = Math.Round(value, 2, MidpointRounding.AwayFromZero);
                return true;
            }

            return false;
        }

        // ------------------------------------------------------------------
        // Reads
        // ------------------------------------------------------------------

        /// <summary>
        /// The Road Signs of a Set with their Asset (name, image, active flag), in a stable internal order
        /// (CreatedAt, then Id). That order is not a learner-visible sequence. With
        /// <paramref name="configuredOnly"/> only playable (IsConfigured = 1) Road Signs are returned: the
        /// Learner side always asks for that.
        /// </summary>
        public static DataTable GetItemsBySet(int practiceId, bool configuredOnly = false)
        {
            string sql = @"
                SELECT i.HiddenRoadSignItemId, i.HiddenRoadSignPracticeId, i.RoadSignAssetId, i.QuestionText, i.Explanation,
                       i.HotspotXPercent, i.HotspotYPercent, i.HotspotWidthPercent, i.HotspotHeightPercent, i.IsConfigured,
                       a.Name AS AssetName, a.ImagePath AS AssetImagePath, a.IsActive AS AssetIsActive,
                       (SELECT COUNT(*) FROM HiddenRoadSignAttemptAnswers x WHERE x.HiddenRoadSignItemId = i.HiddenRoadSignItemId) AS AnswerCount
                FROM HiddenRoadSignItems i
                INNER JOIN HiddenRoadSignAssets a ON a.RoadSignAssetId = i.RoadSignAssetId
                WHERE i.HiddenRoadSignPracticeId = @PracticeId"
                + (configuredOnly ? " AND i.IsConfigured = 1" : string.Empty) + @"
                ORDER BY i.CreatedAt ASC, i.HiddenRoadSignItemId ASC";

            return DatabaseHelper.ExecuteQuery(sql, new SqlParameter("@PracticeId", practiceId));
        }

        public static DataRow GetItemById(int itemId)
        {
            const string sql = @"
                SELECT i.HiddenRoadSignItemId, i.HiddenRoadSignPracticeId, i.RoadSignAssetId, i.QuestionText, i.Explanation,
                       i.HotspotXPercent, i.HotspotYPercent, i.HotspotWidthPercent, i.HotspotHeightPercent, i.IsConfigured,
                       a.Name AS AssetName, a.ImagePath AS AssetImagePath, a.IsActive AS AssetIsActive
                FROM HiddenRoadSignItems i
                INNER JOIN HiddenRoadSignAssets a ON a.RoadSignAssetId = i.RoadSignAssetId
                WHERE i.HiddenRoadSignItemId = @ItemId";

            DataTable table = DatabaseHelper.ExecuteQuery(sql, new SqlParameter("@ItemId", itemId));
            return table.Rows.Count == 0 ? null : table.Rows[0];
        }

        /// <summary>
        /// All choices of every hotspot in a Set, ordered A/B/C per Item. Includes IsCorrect, so callers
        /// that build learner-facing data must NOT forward that column.
        /// </summary>
        public static DataTable GetChoicesBySet(int practiceId)
        {
            const string sql = @"
                SELECT c.HiddenRoadSignChoiceId, c.HiddenRoadSignItemId, c.ChoiceText, c.DisplayOrder, c.IsCorrect
                FROM HiddenRoadSignChoices c
                INNER JOIN HiddenRoadSignItems i ON i.HiddenRoadSignItemId = c.HiddenRoadSignItemId
                WHERE i.HiddenRoadSignPracticeId = @PracticeId
                ORDER BY c.HiddenRoadSignItemId ASC, c.DisplayOrder ASC";

            return DatabaseHelper.ExecuteQuery(sql, new SqlParameter("@PracticeId", practiceId));
        }

        public static bool HasAttemptAnswers(int itemId)
        {
            object result = DatabaseHelper.ExecuteScalar(
                "SELECT COUNT(1) FROM HiddenRoadSignAttemptAnswers WHERE HiddenRoadSignItemId = @ItemId",
                new SqlParameter("@ItemId", itemId));

            return Convert.ToInt32(result) > 0;
        }

        // ------------------------------------------------------------------
        // Writes
        // ------------------------------------------------------------------
        // Road signs are written ONLY by HiddenRoadSignGameSetRepository.Save, as part of the one final
        // Save Game Set / Save Changes transaction. The helpers below are shared with it.

        internal static SqlParameter Percent(string name, decimal value)
        {
            SqlParameter parameter = new SqlParameter(name, SqlDbType.Decimal);
            parameter.Precision = 5;
            parameter.Scale = 2;
            parameter.Value = value;
            return parameter;
        }

        internal static void UpsertChoice(SqlConnection connection, SqlTransaction transaction, int itemId, int displayOrder, string text, bool isCorrect)
        {
            using (SqlCommand command = new SqlCommand(
                "UPDATE HiddenRoadSignChoices SET ChoiceText = @Text, IsCorrect = @IsCorrect WHERE HiddenRoadSignItemId = @ItemId AND DisplayOrder = @Order",
                connection, transaction))
            {
                command.Parameters.AddWithValue("@ItemId", itemId);
                command.Parameters.AddWithValue("@Text", text.Trim());
                command.Parameters.AddWithValue("@Order", displayOrder);
                command.Parameters.AddWithValue("@IsCorrect", isCorrect);

                if (command.ExecuteNonQuery() > 0)
                {
                    return;
                }
            }

            // First-time configuration of a Draft (or a malformed item missing this choice): create the row.
            using (SqlCommand command = new SqlCommand(
                "INSERT INTO HiddenRoadSignChoices (HiddenRoadSignItemId, ChoiceText, DisplayOrder, IsCorrect) VALUES (@ItemId, @Text, @Order, @IsCorrect)",
                connection, transaction))
            {
                command.Parameters.AddWithValue("@ItemId", itemId);
                command.Parameters.AddWithValue("@Text", text.Trim());
                command.Parameters.AddWithValue("@Order", displayOrder);
                command.Parameters.AddWithValue("@IsCorrect", isCorrect);
                command.ExecuteNonQuery();
            }
        }
    }
}
