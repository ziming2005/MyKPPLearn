using System;
using System.Data;
using System.Data.SqlClient;
using MyKPPLearn.Security;

namespace MyKPPLearn.Data
{
    public enum LearnerSaveResult { Saved, NotFound, EmailTaken }

    /// <summary>
    /// Data access for Admin Learner Records and Learner Record Details. Only accounts with Role = Learner are ever
    /// read or changed here, so an administrator account cannot be edited, disabled or deleted from these pages.
    /// Users.Status (Active / Disabled) is the account status; a Disabled learner cannot sign in (see Login).
    /// All queries are parameterized.
    /// </summary>
    public static class LearnerRecordRepository
    {
        public const string StatusActive = "Active";
        public const string StatusDisabled = "Disabled";

        private const string SelectSql = @"
            SELECT UserId, FullName, Email, Status, CreatedAt, ProfileImagePath
            FROM Users
            WHERE Role = @Role";

        /// <summary>All learners, most recently joined first.</summary>
        public static DataTable GetAll()
        {
            return DatabaseHelper.ExecuteQuery(SelectSql + " ORDER BY CreatedAt DESC, UserId DESC", RoleParameter());
        }

        public static DataRow GetById(int userId)
        {
            DataTable table = DatabaseHelper.ExecuteQuery(SelectSql + " AND UserId = @UserId",
                RoleParameter(),
                new SqlParameter("@UserId", userId));

            return table.Rows.Count == 0 ? null : table.Rows[0];
        }

        /// <summary>Creates an Active learner account. The email must already be trimmed and lower-cased.</summary>
        public static LearnerSaveResult Create(string fullName, string normalizedEmail, string passwordHash, out int userId)
        {
            userId = 0;

            if (UserRepository.EmailExists(normalizedEmail))
            {
                return LearnerSaveResult.EmailTaken;
            }

            try
            {
                userId = UserRepository.CreateLearner(fullName, normalizedEmail, passwordHash);
                return LearnerSaveResult.Saved;
            }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
            {
                // Someone registered the same email between the check and the insert.
                return LearnerSaveResult.EmailTaken;
            }
        }

        /// <summary>Changes a learner's name and email. The email must not belong to any other account.</summary>
        public static LearnerSaveResult Update(int userId, string fullName, string normalizedEmail)
        {
            object taken = DatabaseHelper.ExecuteScalar(
                "SELECT COUNT(*) FROM Users WHERE Email = @Email AND UserId <> @UserId",
                new SqlParameter("@Email", normalizedEmail),
                new SqlParameter("@UserId", userId));

            if (Convert.ToInt32(taken) > 0)
            {
                return LearnerSaveResult.EmailTaken;
            }

            const string sql = @"
                UPDATE Users
                SET FullName = @FullName, Email = @Email, UpdatedAt = SYSUTCDATETIME()
                WHERE UserId = @UserId AND Role = @Role;";

            try
            {
                int rows = DatabaseHelper.ExecuteNonQuery(sql,
                    new SqlParameter("@FullName", fullName),
                    new SqlParameter("@Email", normalizedEmail),
                    new SqlParameter("@UserId", userId),
                    RoleParameter());

                return rows == 1 ? LearnerSaveResult.Saved : LearnerSaveResult.NotFound;
            }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
            {
                return LearnerSaveResult.EmailTaken;
            }
        }

        /// <summary>Enables or disables a learner account. Returns false when no such learner exists.</summary>
        public static bool SetStatus(int userId, bool active)
        {
            const string sql = @"
                UPDATE Users
                SET Status = @Status, UpdatedAt = SYSUTCDATETIME()
                WHERE UserId = @UserId AND Role = @Role;";

            return DatabaseHelper.ExecuteNonQuery(sql,
                new SqlParameter("@Status", active ? StatusActive : StatusDisabled),
                new SqlParameter("@UserId", userId),
                RoleParameter()) == 1;
        }

        /// <summary>
        /// Permanently deletes a learner and everything that belongs to them (learning progress and all Practice,
        /// Mock Exam and Road Sign Game attempts with their answers) in one transaction, children first.
        /// Returns false when no such learner exists. profileImagePath is the picture to remove after the delete.
        /// </summary>
        public static bool Delete(int userId, out string profileImagePath)
        {
            profileImagePath = null;

            DataRow learner = GetById(userId);

            if (learner == null)
            {
                return false;
            }

            profileImagePath = learner.IsNull("ProfileImagePath") ? null : Convert.ToString(learner["ProfileImagePath"]);

            const string sql = @"
                DELETE x FROM HiddenRoadSignAttemptAnswers x
                    INNER JOIN HiddenRoadSignAttempts a ON a.HiddenRoadSignAttemptId = x.HiddenRoadSignAttemptId
                    WHERE a.UserId = @UserId;
                DELETE FROM HiddenRoadSignAttempts WHERE UserId = @UserId;

                DELETE x FROM PracticeAttemptQuestions x
                    INNER JOIN PracticeAttempts a ON a.PracticeAttemptId = x.PracticeAttemptId
                    WHERE a.UserId = @UserId;
                DELETE FROM PracticeAttempts WHERE UserId = @UserId;

                DELETE x FROM MockExamAttemptQuestions x
                    INNER JOIN MockExamAttempts a ON a.MockExamAttemptId = x.MockExamAttemptId
                    WHERE a.UserId = @UserId;
                DELETE FROM MockExamAttempts WHERE UserId = @UserId;

                DELETE FROM LearningProgress WHERE UserId = @UserId;

                DELETE FROM Users WHERE UserId = @UserId AND Role = @Role;
                SELECT @@ROWCOUNT;";

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        int deleted;

                        using (SqlCommand command = new SqlCommand(sql, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@UserId", userId);
                            command.Parameters.AddWithValue("@Role", SessionHelper.RoleLearner);
                            deleted = Convert.ToInt32(command.ExecuteScalar());
                        }

                        if (deleted != 1)
                        {
                            transaction.Rollback();
                            return false;
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        // ------------------------------------------------------------------
        // Learner Record Details
        // ------------------------------------------------------------------

        /// <summary>Completed Standard Practice attempts, newest first.</summary>
        public static DataTable GetPracticeHistory(int userId)
        {
            const string sql = @"
                SELECT c.Title AS CategoryTitle, p.SetNumber, a.CorrectCount, a.TotalQuestions, a.CompletedAt
                FROM PracticeAttempts a
                INNER JOIN PracticeSets p ON p.PracticeSetId = a.PracticeSetId
                INNER JOIN QuestionCategories c ON c.QuestionCategoryId = p.QuestionCategoryId
                WHERE a.UserId = @UserId
                ORDER BY a.CompletedAt DESC, a.PracticeAttemptId DESC";

            return DatabaseHelper.ExecuteQuery(sql, new SqlParameter("@UserId", userId));
        }

        /// <summary>Completed Mock Exam attempts, newest first.</summary>
        public static DataTable GetMockExamHistory(int userId)
        {
            const string sql = @"
                SELECT s.ExamTypeCode, s.SetNumber, a.ColourBlindCorrect, a.ColourBlindTotal,
                       a.TheoryCorrect, a.TheoryTotal, a.IsPassed, a.CompletedAt
                FROM MockExamAttempts a
                INNER JOIN MockExamSets s ON s.MockExamSetId = a.MockExamSetId
                WHERE a.UserId = @UserId
                ORDER BY a.CompletedAt DESC, a.MockExamAttemptId DESC";

            return DatabaseHelper.ExecuteQuery(sql, new SqlParameter("@UserId", userId));
        }

        private static SqlParameter RoleParameter()
        {
            return new SqlParameter("@Role", SessionHelper.RoleLearner);
        }
    }
}
