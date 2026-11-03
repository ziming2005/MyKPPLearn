using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using MyKPPLearn.Security;

namespace MyKPPLearn.Data
{
    /// <summary>
    /// The learner activity feed shown on the Administrator dashboard (every learner) and the Learner dashboard (one learner).
    /// Kinds: "register", "lesson", "practice", "mock" and "game".
    /// </summary>
    public static class ActivityRepository
    {
        /// <summary>
        /// The latest learner events, newest first: Kind, Who, Detail and At (UTC). Pass a <paramref name="userId"/> to see
        /// only that learner's events. Detail is the lesson title for "lesson", "CategoryTitle|SetNumber" for "practice",
        /// "ExamTypeCode|SetNumber" for "mock" and "SetNumber|Correct|Total" for "game".
        /// Each kind is cut to its newest <paramref name="maxRows"/> rows BEFORE they are combined, so the query stays fast
        /// however many events there are.
        /// </summary>
        public static DataTable GetRecent(int maxRows, int? userId = null)
        {
            const string sql = @"
                SELECT TOP (@MaxRows) Kind, Who, Detail, At
                FROM (
                    SELECT * FROM (
                        SELECT TOP (@MaxRows) 'register' AS Kind, u.FullName AS Who, CAST('' AS NVARCHAR(300)) AS Detail, u.CreatedAt AS At
                        FROM Users u
                        WHERE u.Role = @Role AND (@UserId IS NULL OR u.UserId = @UserId)
                        ORDER BY u.CreatedAt DESC
                    ) registrations
                    UNION ALL
                    SELECT * FROM (
                        SELECT TOP (@MaxRows) 'lesson' AS Kind, u.FullName AS Who, CAST(l.Title AS NVARCHAR(300)) AS Detail, p.CompletedAt AS At
                        FROM LearningProgress p
                        INNER JOIN Users u ON u.UserId = p.UserId
                        INNER JOIN Lessons l ON l.LessonId = p.LessonId
                        WHERE @UserId IS NULL OR p.UserId = @UserId
                        ORDER BY p.CompletedAt DESC
                    ) lessons
                    UNION ALL
                    SELECT * FROM (
                        SELECT TOP (@MaxRows) 'practice' AS Kind, u.FullName AS Who,
                               CAST(c.Title + '|' + CAST(s.SetNumber AS NVARCHAR(20)) AS NVARCHAR(300)) AS Detail,
                               a.CompletedAt AS At
                        FROM PracticeAttempts a
                        INNER JOIN Users u ON u.UserId = a.UserId
                        INNER JOIN PracticeSets s ON s.PracticeSetId = a.PracticeSetId
                        INNER JOIN QuestionCategories c ON c.QuestionCategoryId = s.QuestionCategoryId
                        WHERE @UserId IS NULL OR a.UserId = @UserId
                        ORDER BY a.CompletedAt DESC
                    ) practices
                    UNION ALL
                    SELECT * FROM (
                        SELECT TOP (@MaxRows) 'mock' AS Kind, u.FullName AS Who,
                               CAST(s.ExamTypeCode + '|' + CAST(s.SetNumber AS NVARCHAR(20)) AS NVARCHAR(300)) AS Detail,
                               a.CompletedAt AS At
                        FROM MockExamAttempts a
                        INNER JOIN Users u ON u.UserId = a.UserId
                        INNER JOIN MockExamSets s ON s.MockExamSetId = a.MockExamSetId
                        WHERE @UserId IS NULL OR a.UserId = @UserId
                        ORDER BY a.CompletedAt DESC
                    ) mocks
                    UNION ALL
                    SELECT * FROM (
                        SELECT TOP (@MaxRows) 'game' AS Kind, u.FullName AS Who,
                               CAST(CAST(pr.SetNumber AS NVARCHAR(20)) + '|' + CAST(a.CorrectCount AS NVARCHAR(20)) + '|' + CAST(a.TotalItems AS NVARCHAR(20)) AS NVARCHAR(300)) AS Detail,
                               a.CompletedAt AS At
                        FROM HiddenRoadSignAttempts a
                        INNER JOIN Users u ON u.UserId = a.UserId
                        INNER JOIN HiddenRoadSignPractices pr ON pr.HiddenRoadSignPracticeId = a.HiddenRoadSignPracticeId
                        WHERE @UserId IS NULL OR a.UserId = @UserId
                        ORDER BY a.CompletedAt DESC
                    ) games
                ) x
                ORDER BY At DESC";

            return DatabaseHelper.ExecuteQuery(sql,
                new SqlParameter("@MaxRows", maxRows),
                new SqlParameter("@Role", SessionHelper.RoleLearner),
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId.HasValue ? (object)userId.Value : DBNull.Value });
        }

        /// <summary>What the learner did, as safe HTML without the subject: "completed <em>Lesson title</em>".</summary>
        public static string Describe(object kind, object detail)
        {
            string text = Convert.ToString(detail);
            string[] parts = text.Split('|');

            switch (Convert.ToString(kind))
            {
                case "register":
                    return "registered a learner account";

                case "lesson":
                    return "completed <em>" + HttpUtility.HtmlEncode(text) + "</em>";

                case "practice":
                    return parts.Length == 2
                        ? "finished " + HttpUtility.HtmlEncode(parts[0] + " — Practice Set " + parts[1])
                        : "finished a practice set";

                case "mock":
                    if (parts.Length == 2)
                    {
                        MockExamType type = MockExamRules.Find(parts[0]);
                        string name = type == null ? parts[0] : type.Name;
                        return "finished " + HttpUtility.HtmlEncode(name + " — Mock Exam Set " + parts[1]);
                    }

                    return "finished a mock exam";

                default:
                    return parts.Length == 3
                        ? "finished " + HttpUtility.HtmlEncode(HiddenRoadSignPracticeRepository.FormatSetName(parts[0]))
                        : "finished a Road Sign Game";
            }
        }
    }
}
