using System;
using System.Data;
using System.Data.SqlClient;
using MyKPPLearn.Security;

namespace MyKPPLearn.Data
{
    /// <summary>Read-only queries behind the Administrator dashboard.</summary>
    public static class AdminDashboardRepository
    {
        /// <summary>Learner accounts created in the last <paramref name="days"/> days.</summary>
        public static int GetNewLearnerCount(int days)
        {
            object result = DatabaseHelper.ExecuteScalar(
                "SELECT COUNT(*) FROM Users WHERE Role = @Role AND CreatedAt >= DATEADD(day, -@Days, SYSUTCDATETIME())",
                new SqlParameter("@Role", SessionHelper.RoleLearner),
                new SqlParameter("@Days", days));

            return Convert.ToInt32(result);
        }

        public static int GetActiveChapterCount()
        {
            return Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM LearningChapters WHERE IsActive = 1"));
        }

        /// <summary>Road sign assets in the library (Active and Inactive).</summary>
        public static int GetRoadSignAssetCount()
        {
            return Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM HiddenRoadSignAssets"));
        }

        /// <summary>
        /// The latest learner events, newest first: Kind ("register", "lesson" or "game"), Who, Detail and At (UTC).
        /// Detail is the lesson title for "lesson", and "SetNumber|Correct|Total" for "game".
        /// Each kind is cut to its newest <paramref name="maxRows"/> rows BEFORE they are combined, so the query stays fast
        /// however many registrations, lessons and games there are.
        /// </summary>
        public static DataTable GetRecentActivity(int maxRows)
        {
            const string sql = @"
                SELECT TOP (@MaxRows) Kind, Who, Detail, At
                FROM (
                    SELECT * FROM (
                        SELECT TOP (@MaxRows) 'register' AS Kind, u.FullName AS Who, CAST('' AS NVARCHAR(300)) AS Detail, u.CreatedAt AS At
                        FROM Users u
                        WHERE u.Role = @Role
                        ORDER BY u.CreatedAt DESC
                    ) registrations
                    UNION ALL
                    SELECT * FROM (
                        SELECT TOP (@MaxRows) 'lesson' AS Kind, u.FullName AS Who, CAST(l.Title AS NVARCHAR(300)) AS Detail, p.CompletedAt AS At
                        FROM LearningProgress p
                        INNER JOIN Users u ON u.UserId = p.UserId
                        INNER JOIN Lessons l ON l.LessonId = p.LessonId
                        ORDER BY p.CompletedAt DESC
                    ) lessons
                    UNION ALL
                    SELECT * FROM (
                        SELECT TOP (@MaxRows) 'game' AS Kind, u.FullName AS Who,
                               CAST(CAST(pr.SetNumber AS NVARCHAR(20)) + '|' + CAST(a.CorrectCount AS NVARCHAR(20)) + '|' + CAST(a.TotalItems AS NVARCHAR(20)) AS NVARCHAR(300)) AS Detail,
                               a.CompletedAt AS At
                        FROM HiddenRoadSignAttempts a
                        INNER JOIN Users u ON u.UserId = a.UserId
                        INNER JOIN HiddenRoadSignPractices pr ON pr.HiddenRoadSignPracticeId = a.HiddenRoadSignPracticeId
                        ORDER BY a.CompletedAt DESC
                    ) games
                ) x
                ORDER BY At DESC";

            return DatabaseHelper.ExecuteQuery(sql,
                new SqlParameter("@MaxRows", maxRows),
                new SqlParameter("@Role", SessionHelper.RoleLearner));
        }
    }
}
