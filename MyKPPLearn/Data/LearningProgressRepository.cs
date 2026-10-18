using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace MyKPPLearn.Data
{
    /// <summary>
    /// Data access for LearningProgress (lesson completion only).
    /// Counts and percentages only consider valid learning content:
    /// Lesson.IsActive = 1 AND parent Chapter.IsActive = 1.
    /// </summary>
    public static class LearningProgressRepository
    {
        private const int SqlDuplicateKey = 2601;
        private const int SqlUniqueConstraint = 2627;

        /// <summary>Records completion once. Returns true if a new row was inserted, false if it already existed.</summary>
        public static bool MarkLessonCompleted(int userId, int lessonId)
        {
            const string sql = @"
                IF NOT EXISTS (SELECT 1 FROM LearningProgress WHERE UserId = @UserId AND LessonId = @LessonId)
                    INSERT INTO LearningProgress (UserId, LessonId, CompletedAt)
                    VALUES (@UserId, @LessonId, SYSUTCDATETIME());";

            try
            {
                return DatabaseHelper.ExecuteNonQuery(sql,
                    new SqlParameter("@UserId", userId),
                    new SqlParameter("@LessonId", lessonId)) > 0;
            }
            catch (SqlException ex) when (ex.Number == SqlDuplicateKey || ex.Number == SqlUniqueConstraint)
            {
                return false;
            }
        }

        public static bool IsLessonCompleted(int userId, int lessonId)
        {
            object result = DatabaseHelper.ExecuteScalar(
                "SELECT COUNT(1) FROM LearningProgress WHERE UserId = @UserId AND LessonId = @LessonId",
                new SqlParameter("@UserId", userId),
                new SqlParameter("@LessonId", lessonId));

            return Convert.ToInt32(result) > 0;
        }

        public static bool LessonHasProgress(int lessonId)
        {
            object result = DatabaseHelper.ExecuteScalar(
                "SELECT COUNT(1) FROM LearningProgress WHERE LessonId = @LessonId",
                new SqlParameter("@LessonId", lessonId));

            return Convert.ToInt32(result) > 0;
        }

        public static int GetCompletedLessonCount(int userId)
        {
            const string sql = @"
                SELECT COUNT(*) FROM LearningProgress p
                INNER JOIN Lessons l ON l.LessonId = p.LessonId
                INNER JOIN LearningChapters c ON c.ChapterId = l.ChapterId
                WHERE p.UserId = @UserId AND l.IsActive = 1 AND c.IsActive = 1";

            return Convert.ToInt32(DatabaseHelper.ExecuteScalar(sql, new SqlParameter("@UserId", userId)));
        }

        /// <summary>Whole-number percentage (0-100); 0 when no active lessons exist.</summary>
        public static int GetLearningProgressPercentage(int userId)
        {
            int total = LessonRepository.GetTotalActiveLessonCount();

            if (total <= 0)
            {
                return 0;
            }

            int completed = GetCompletedLessonCount(userId);
            return (int)Math.Round(completed * 100.0 / total, MidpointRounding.AwayFromZero);
        }

        public static HashSet<int> GetCompletedLessonIds(int userId)
        {
            DataTable table = DatabaseHelper.ExecuteQuery(
                "SELECT LessonId FROM LearningProgress WHERE UserId = @UserId",
                new SqlParameter("@UserId", userId));

            HashSet<int> ids = new HashSet<int>();

            foreach (DataRow row in table.Rows)
            {
                ids.Add(Convert.ToInt32(row["LessonId"]));
            }

            return ids;
        }
    }
}
