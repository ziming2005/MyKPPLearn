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
    }
}
