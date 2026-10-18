using System;
using System.Data;
using System.Data.SqlClient;

namespace MyKPPLearn.Data
{
    /// <summary>
    /// Data access for LearningChapters. All queries are parameterized.
    /// DisplayOrder is the learner-visible Chapter number and is never renumbered automatically.
    /// </summary>
    public static class LearningChapterRepository
    {
        private const int SqlDuplicateKey = 2601;
        private const int SqlUniqueConstraint = 2627;

        /// <summary>Every Chapter number (DisplayOrder) already in use, ascending.</summary>
        public static System.Collections.Generic.List<int> GetUsedNumbers()
        {
            System.Collections.Generic.List<int> numbers = new System.Collections.Generic.List<int>();

            foreach (DataRow row in DatabaseHelper.ExecuteQuery("SELECT DisplayOrder FROM LearningChapters ORDER BY DisplayOrder ASC").Rows)
            {
                numbers.Add(Convert.ToInt32(row["DisplayOrder"]));
            }

            return numbers;
        }

        public static DataTable GetAllChaptersForAdmin()
        {
            const string sql = @"
                SELECT c.ChapterId, c.Title, c.Description, c.DisplayOrder, c.IsActive,
                       (SELECT COUNT(*) FROM Lessons l WHERE l.ChapterId = c.ChapterId) AS LessonCount
                FROM LearningChapters c
                ORDER BY c.DisplayOrder ASC";

            return DatabaseHelper.ExecuteQuery(sql);
        }

        /// <summary>
        /// Active chapters only, with the number of active lessons and how many of them
        /// the given learner has completed (used for per-chapter progress).
        /// </summary>
        public static DataTable GetActiveChaptersForLearner(int userId)
        {
            const string sql = @"
                SELECT c.ChapterId, c.Title, c.Description, c.DisplayOrder,
                       (SELECT COUNT(*) FROM Lessons l
                         WHERE l.ChapterId = c.ChapterId AND l.IsActive = 1) AS LessonCount,
                       (SELECT COUNT(*) FROM Lessons l
                         INNER JOIN LearningProgress p ON p.LessonId = l.LessonId AND p.UserId = @UserId
                         WHERE l.ChapterId = c.ChapterId AND l.IsActive = 1) AS CompletedCount
                FROM LearningChapters c
                WHERE c.IsActive = 1
                ORDER BY c.DisplayOrder ASC";

            return DatabaseHelper.ExecuteQuery(sql, new SqlParameter("@UserId", userId));
        }

        /// <summary>
        /// The next ACTIVE chapter after <paramref name="currentChapterOrder"/> (by DisplayOrder) that has at least one active lesson
        /// (ChapterId, DisplayOrder, Title). Null when the learner is already at the last chapter.
        /// </summary>
        public static DataRow GetNextChapterWithLesson(int currentChapterOrder)
        {
            const string sql = @"
                SELECT TOP 1 c.ChapterId, c.DisplayOrder, c.Title
                FROM LearningChapters c
                WHERE c.IsActive = 1
                  AND c.DisplayOrder > @Order
                  AND EXISTS (SELECT 1 FROM Lessons l WHERE l.ChapterId = c.ChapterId AND l.IsActive = 1)
                ORDER BY c.DisplayOrder ASC";

            DataTable table = DatabaseHelper.ExecuteQuery(sql, new SqlParameter("@Order", currentChapterOrder));
            return table.Rows.Count == 0 ? null : table.Rows[0];
        }

        public static DataRow GetChapterById(int chapterId)
        {
            const string sql = @"
                SELECT c.ChapterId, c.Title, c.Description, c.DisplayOrder, c.IsActive,
                       (SELECT COUNT(*) FROM Lessons l WHERE l.ChapterId = c.ChapterId) AS LessonCount
                FROM LearningChapters c
                WHERE c.ChapterId = @ChapterId";

            DataTable table = DatabaseHelper.ExecuteQuery(sql, new SqlParameter("@ChapterId", chapterId));
            return table.Rows.Count == 0 ? null : table.Rows[0];
        }

        /// <summary>Returns the new ChapterId, or 0 if the chapter number is already taken.</summary>
        public static int CreateChapter(int displayOrder, string title, string description)
        {
            const string sql = @"
                INSERT INTO LearningChapters (Title, Description, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
                OUTPUT INSERTED.ChapterId
                VALUES (@Title, @Description, @DisplayOrder, 1, SYSUTCDATETIME(), SYSUTCDATETIME());";

            try
            {
                object result = DatabaseHelper.ExecuteScalar(sql,
                    new SqlParameter("@Title", title),
                    new SqlParameter("@Description", description),
                    new SqlParameter("@DisplayOrder", displayOrder));

                return Convert.ToInt32(result);
            }
            catch (SqlException ex) when (IsDuplicate(ex))
            {
                return 0;
            }
        }

        /// <summary>Returns false if the new chapter number is already used by another chapter.</summary>
        public static bool UpdateChapter(int chapterId, int displayOrder, string title, string description)
        {
            const string sql = @"
                UPDATE LearningChapters
                SET Title = @Title, Description = @Description, DisplayOrder = @DisplayOrder,
                    UpdatedAt = SYSUTCDATETIME()
                WHERE ChapterId = @ChapterId";

            try
            {
                DatabaseHelper.ExecuteNonQuery(sql,
                    new SqlParameter("@ChapterId", chapterId),
                    new SqlParameter("@Title", title),
                    new SqlParameter("@Description", description),
                    new SqlParameter("@DisplayOrder", displayOrder));

                return true;
            }
            catch (SqlException ex) when (IsDuplicate(ex))
            {
                return false;
            }
        }

        /// <summary>Deletes the chapter only if it contains no lessons. Returns true if a row was deleted.</summary>
        public static bool DeleteChapter(int chapterId)
        {
            const string sql = @"
                DELETE FROM LearningChapters
                WHERE ChapterId = @ChapterId
                  AND NOT EXISTS (SELECT 1 FROM Lessons WHERE ChapterId = @ChapterId)";

            return DatabaseHelper.ExecuteNonQuery(sql, new SqlParameter("@ChapterId", chapterId)) > 0;
        }

        public static void SetChapterActive(int chapterId, bool isActive)
        {
            DatabaseHelper.ExecuteNonQuery(
                "UPDATE LearningChapters SET IsActive = @IsActive, UpdatedAt = SYSUTCDATETIME() WHERE ChapterId = @ChapterId",
                new SqlParameter("@ChapterId", chapterId),
                new SqlParameter("@IsActive", isActive));
        }

        /// <param name="excludeChapterId">Chapter being edited (0 when creating).</param>
        public static bool ChapterDisplayOrderExists(int displayOrder, int excludeChapterId)
        {
            object result = DatabaseHelper.ExecuteScalar(
                "SELECT COUNT(1) FROM LearningChapters WHERE DisplayOrder = @DisplayOrder AND ChapterId <> @ExcludeId",
                new SqlParameter("@DisplayOrder", displayOrder),
                new SqlParameter("@ExcludeId", excludeChapterId));

            return Convert.ToInt32(result) > 0;
        }

        private static bool IsDuplicate(SqlException ex)
        {
            return ex.Number == SqlDuplicateKey || ex.Number == SqlUniqueConstraint;
        }
    }
}
