using System;
using System.Data;
using System.Data.SqlClient;

namespace MyKPPLearn.Data
{
    /// <summary>
    /// Data access for Lessons. Lessons.DisplayOrder is only the position inside the chapter;
    /// the learner-visible number "X.Y" is composed from ChapterOrder and DisplayOrder at display time.
    /// "Valid learning content" = Lesson.IsActive = 1 AND parent Chapter.IsActive = 1.
    /// </summary>
    public static class LessonRepository
    {
        private const int SqlDuplicateKey = 2601;
        private const int SqlUniqueConstraint = 2627;

        public const string ImagePositionAbove = "Above";
        public const string ImagePositionBelow = "Below";

        private const string AdminSelect = @"
            SELECT l.LessonId, l.ChapterId, l.Title, l.Content, l.ImagePath, l.ImagePosition, l.DisplayOrder, l.IsActive,
                   c.DisplayOrder AS ChapterOrder, c.Title AS ChapterTitle, c.IsActive AS ChapterIsActive
            FROM Lessons l
            INNER JOIN LearningChapters c ON c.ChapterId = l.ChapterId";

        public static string FormatLessonNumber(object chapterOrder, object lessonOrder)
        {
            return Convert.ToInt32(chapterOrder) + "." + Convert.ToInt32(lessonOrder);
        }

        /// <summary>Never trusts a posted value: anything other than "Above"/"Below" becomes "Above".</summary>
        public static string NormalizeImagePosition(string value)
        {
            return string.Equals(value, ImagePositionBelow, StringComparison.Ordinal)
                ? ImagePositionBelow
                : ImagePositionAbove;
        }

        /// <summary>Lesson numbers (DisplayOrder) already in use, grouped by Chapter id, ascending. Lessons of other Chapters never mix.</summary>
        public static System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<int>> GetUsedNumbersByChapter()
        {
            System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<int>> map =
                new System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<int>>();

            foreach (DataRow row in DatabaseHelper.ExecuteQuery("SELECT ChapterId, DisplayOrder FROM Lessons ORDER BY ChapterId ASC, DisplayOrder ASC").Rows)
            {
                int chapterId = Convert.ToInt32(row["ChapterId"]);

                if (!map.ContainsKey(chapterId))
                {
                    map[chapterId] = new System.Collections.Generic.List<int>();
                }

                map[chapterId].Add(Convert.ToInt32(row["DisplayOrder"]));
            }

            return map;
        }

        public static DataTable GetAllLessonsForAdmin()
        {
            return DatabaseHelper.ExecuteQuery(AdminSelect + " ORDER BY c.DisplayOrder ASC, l.DisplayOrder ASC");
        }

        public static DataTable GetLessonsByChapterForAdmin(int chapterId)
        {
            return DatabaseHelper.ExecuteQuery(
                AdminSelect + " WHERE l.ChapterId = @ChapterId ORDER BY c.DisplayOrder ASC, l.DisplayOrder ASC",
                new SqlParameter("@ChapterId", chapterId));
        }

        /// <summary>Active lessons of a chapter with the learner's completion flag (IsCompleted).</summary>
        public static DataTable GetActiveLessonsByChapter(int chapterId, int userId)
        {
            const string sql = @"
                SELECT l.LessonId, l.ChapterId, l.Title, l.DisplayOrder,
                       c.DisplayOrder AS ChapterOrder,
                       CAST(CASE WHEN p.LearningProgressId IS NULL THEN 0 ELSE 1 END AS BIT) AS IsCompleted
                FROM Lessons l
                INNER JOIN LearningChapters c ON c.ChapterId = l.ChapterId
                LEFT JOIN LearningProgress p ON p.LessonId = l.LessonId AND p.UserId = @UserId
                WHERE l.ChapterId = @ChapterId AND l.IsActive = 1
                ORDER BY l.DisplayOrder ASC";

            return DatabaseHelper.ExecuteQuery(sql,
                new SqlParameter("@ChapterId", chapterId),
                new SqlParameter("@UserId", userId));
        }

        public static DataRow GetLessonById(int lessonId)
        {
            DataTable table = DatabaseHelper.ExecuteQuery(AdminSelect + " WHERE l.LessonId = @LessonId",
                new SqlParameter("@LessonId", lessonId));

            return table.Rows.Count == 0 ? null : table.Rows[0];
        }

        /// <summary>Returns the new LessonId, or 0 if the lesson number already exists in the chapter.</summary>
        public static int CreateLesson(int chapterId, int displayOrder, string title, string content, string imagePath, string imagePosition)
        {
            const string sql = @"
                INSERT INTO Lessons (ChapterId, Title, Content, ImagePath, ImagePosition, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
                OUTPUT INSERTED.LessonId
                VALUES (@ChapterId, @Title, @Content, @ImagePath, @ImagePosition, @DisplayOrder, 1, SYSUTCDATETIME(), SYSUTCDATETIME());";

            try
            {
                object result = DatabaseHelper.ExecuteScalar(sql,
                    new SqlParameter("@ChapterId", chapterId),
                    new SqlParameter("@Title", title),
                    new SqlParameter("@Content", content),
                    new SqlParameter("@ImagePath", (object)imagePath ?? DBNull.Value),
                    new SqlParameter("@ImagePosition", NormalizeImagePosition(imagePosition)),
                    new SqlParameter("@DisplayOrder", displayOrder));

                return Convert.ToInt32(result);
            }
            catch (SqlException ex) when (IsDuplicate(ex))
            {
                return 0;
            }
        }

        /// <param name="imagePath">New image path, or null to keep the existing image unchanged.</param>
        /// <param name="removeImage">True to clear the existing image when no new image is given (a new image always replaces the old one).</param>
        public static bool UpdateLesson(int lessonId, int chapterId, int displayOrder, string title, string content, string imagePath, string imagePosition, bool removeImage = false)
        {
            const string sql = @"
                UPDATE Lessons
                SET ChapterId = @ChapterId, Title = @Title, Content = @Content, DisplayOrder = @DisplayOrder,
                    ImagePath = CASE WHEN @RemoveImage = 1 AND @ImagePath IS NULL THEN NULL ELSE ISNULL(@ImagePath, ImagePath) END,
                    ImagePosition = @ImagePosition, UpdatedAt = SYSUTCDATETIME()
                WHERE LessonId = @LessonId";

            try
            {
                DatabaseHelper.ExecuteNonQuery(sql,
                    new SqlParameter("@LessonId", lessonId),
                    new SqlParameter("@ChapterId", chapterId),
                    new SqlParameter("@Title", title),
                    new SqlParameter("@Content", content),
                    new SqlParameter("@ImagePath", (object)imagePath ?? DBNull.Value),
                    new SqlParameter("@ImagePosition", NormalizeImagePosition(imagePosition)),
                    new SqlParameter("@DisplayOrder", displayOrder),
                    new SqlParameter("@RemoveImage", removeImage));

                return true;
            }
            catch (SqlException ex) when (IsDuplicate(ex))
            {
                return false;
            }
        }

        /// <summary>Deletes the lesson only if no learner progress exists for it. Returns true if deleted.</summary>
        public static bool DeleteLesson(int lessonId)
        {
            const string sql = @"
                DELETE FROM Lessons
                WHERE LessonId = @LessonId
                  AND NOT EXISTS (SELECT 1 FROM LearningProgress WHERE LessonId = @LessonId)";

            return DatabaseHelper.ExecuteNonQuery(sql, new SqlParameter("@LessonId", lessonId)) > 0;
        }

        public static void SetLessonActive(int lessonId, bool isActive)
        {
            DatabaseHelper.ExecuteNonQuery(
                "UPDATE Lessons SET IsActive = @IsActive, UpdatedAt = SYSUTCDATETIME() WHERE LessonId = @LessonId",
                new SqlParameter("@LessonId", lessonId),
                new SqlParameter("@IsActive", isActive));
        }

        /// <param name="excludeLessonId">Lesson being edited (0 when creating).</param>
        public static bool LessonDisplayOrderExists(int chapterId, int displayOrder, int excludeLessonId)
        {
            object result = DatabaseHelper.ExecuteScalar(
                "SELECT COUNT(1) FROM Lessons WHERE ChapterId = @ChapterId AND DisplayOrder = @DisplayOrder AND LessonId <> @ExcludeId",
                new SqlParameter("@ChapterId", chapterId),
                new SqlParameter("@DisplayOrder", displayOrder),
                new SqlParameter("@ExcludeId", excludeLessonId));

            return Convert.ToInt32(result) > 0;
        }

        /// <summary>Active + inactive lessons (administrator management count).</summary>
        public static int GetTotalLessonCount()
        {
            return Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Lessons"));
        }

        /// <summary>Lessons that are active inside an active chapter (learner-visible content).</summary>
        public static int GetTotalActiveLessonCount()
        {
            const string sql = @"
                SELECT COUNT(*) FROM Lessons l
                INNER JOIN LearningChapters c ON c.ChapterId = l.ChapterId
                WHERE l.IsActive = 1 AND c.IsActive = 1";

            return Convert.ToInt32(DatabaseHelper.ExecuteScalar(sql));
        }

        /// <summary>First valid lesson the learner has not completed (Chapter order, then Lesson order), or null.</summary>
        public static DataRow GetFirstIncompleteLesson(int userId)
        {
            const string sql = @"
                SELECT TOP 1 l.LessonId, l.Title, l.DisplayOrder, c.DisplayOrder AS ChapterOrder
                FROM Lessons l
                INNER JOIN LearningChapters c ON c.ChapterId = l.ChapterId
                WHERE l.IsActive = 1 AND c.IsActive = 1
                  AND NOT EXISTS (SELECT 1 FROM LearningProgress p WHERE p.LessonId = l.LessonId AND p.UserId = @UserId)
                ORDER BY c.DisplayOrder ASC, l.DisplayOrder ASC";

            DataTable table = DatabaseHelper.ExecuteQuery(sql, new SqlParameter("@UserId", userId));
            return table.Rows.Count == 0 ? null : table.Rows[0];
        }

        /// <summary>
        /// Nearest active lesson before the current one inside the same (active) chapter, or null.
        /// Does not assume DisplayOrder values are contiguous.
        /// </summary>
        public static DataRow GetPreviousActiveLessonInChapter(int chapterId, int currentDisplayOrder)
        {
            const string sql = @"
                SELECT TOP 1 l.LessonId, l.Title, l.DisplayOrder, c.DisplayOrder AS ChapterOrder
                FROM Lessons l
                INNER JOIN LearningChapters c ON c.ChapterId = l.ChapterId
                WHERE l.ChapterId = @ChapterId AND l.IsActive = 1 AND c.IsActive = 1
                  AND l.DisplayOrder < @CurrentOrder
                ORDER BY l.DisplayOrder DESC";

            DataTable table = DatabaseHelper.ExecuteQuery(sql,
                new SqlParameter("@ChapterId", chapterId),
                new SqlParameter("@CurrentOrder", currentDisplayOrder));

            return table.Rows.Count == 0 ? null : table.Rows[0];
        }

        /// <summary>Nearest active lesson after the current one inside the same (active) chapter, or null.</summary>
        public static DataRow GetNextActiveLessonInChapter(int chapterId, int currentDisplayOrder)
        {
            const string sql = @"
                SELECT TOP 1 l.LessonId, l.Title, l.DisplayOrder, c.DisplayOrder AS ChapterOrder
                FROM Lessons l
                INNER JOIN LearningChapters c ON c.ChapterId = l.ChapterId
                WHERE l.ChapterId = @ChapterId AND l.IsActive = 1 AND c.IsActive = 1
                  AND l.DisplayOrder > @CurrentOrder
                ORDER BY l.DisplayOrder ASC";

            DataTable table = DatabaseHelper.ExecuteQuery(sql,
                new SqlParameter("@ChapterId", chapterId),
                new SqlParameter("@CurrentOrder", currentDisplayOrder));

            return table.Rows.Count == 0 ? null : table.Rows[0];
        }

        private static bool IsDuplicate(SqlException ex)
        {
            return ex.Number == SqlDuplicateKey || ex.Number == SqlUniqueConstraint;
        }
    }
}
