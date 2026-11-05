using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace MyKPPLearn.Data
{
    public enum MockExamSetDeleteResult { Deleted, NotFound, HasResults }

    /// <summary>
    /// Data access for Mock Exam Sets (Admin Mock Exam and the Learner Mock Exam pages) and the questions chosen for
    /// each set (MockExamSetQuestions, ordered by DisplayOrder). A set is "complete" when every section of its exam
    /// type has exactly the required number of ACTIVE questions from that section's category; only a complete set can
    /// be Active, and learners only see sets that are Active and still complete. All queries are parameterized.
    /// </summary>
    public static class MockExamSetRepository
    {
        private const string SelectSql = @"
            SELECT s.MockExamSetId, s.ExamTypeCode, s.SetNumber, s.IsActive,
                   (SELECT COUNT(*) FROM MockExamAttempts a WHERE a.MockExamSetId = s.MockExamSetId) AS AttemptCount,
                   (SELECT COUNT(DISTINCT a.UserId) FROM MockExamAttempts a WHERE a.MockExamSetId = s.MockExamSetId) AS LearnerCount
            FROM MockExamSets s";

        public static DataTable GetAll()
        {
            return DatabaseHelper.ExecuteQuery(SelectSql + " ORDER BY s.ExamTypeCode, s.SetNumber");
        }

        public static DataRow GetById(int id)
        {
            DataTable table = DatabaseHelper.ExecuteQuery(SelectSql + " WHERE s.MockExamSetId = @Id", new SqlParameter("@Id", id));
            return table.Rows.Count == 0 ? null : table.Rows[0];
        }

        public static DataRow GetByTypeAndNumber(string typeCode, int setNumber)
        {
            DataTable table = DatabaseHelper.ExecuteQuery(SelectSql + " WHERE s.ExamTypeCode = @Type AND s.SetNumber = @Number",
                new SqlParameter("@Type", typeCode),
                new SqlParameter("@Number", setNumber));
            return table.Rows.Count == 0 ? null : table.Rows[0];
        }

        /// <summary>Every chosen question of every set, with what completeness needs: its category and whether it is active.</summary>
        public static DataTable GetAllSelections()
        {
            return DatabaseHelper.ExecuteQuery(@"
                SELECT sq.MockExamSetId, sq.QuestionId, sq.DisplayOrder, c.Code AS CategoryCode, q.IsActive
                FROM MockExamSetQuestions sq
                INNER JOIN Questions q ON q.QuestionId = sq.QuestionId
                INNER JOIN QuestionCategories c ON c.QuestionCategoryId = q.QuestionCategoryId
                ORDER BY sq.MockExamSetId, sq.DisplayOrder");
        }

        /// <summary>The questions an administrator can choose from: every question in the four exam categories.</summary>
        public static DataTable GetQuestionChoices()
        {
            return DatabaseHelper.ExecuteQuery(@"
                SELECT q.QuestionId, c.Code AS CategoryCode, q.QuestionText, q.ImagePath, q.IsActive
                FROM Questions q
                INNER JOIN QuestionCategories c ON c.QuestionCategoryId = q.QuestionCategoryId
                ORDER BY c.DisplayOrder, q.QuestionId");
        }

        /// <summary>
        /// The exam paper of a set in DisplayOrder, one row per option (three per question), with everything needed to
        /// show the question and to mark it.
        /// </summary>
        public static DataTable GetPaper(int setId)
        {
            return DatabaseHelper.ExecuteQuery(@"
                SELECT sq.DisplayOrder, q.QuestionId, c.Code AS CategoryCode, q.QuestionText, q.Explanation,
                       q.ImagePath, q.IsActive,
                       o.QuestionOptionId, o.OptionOrder, o.OptionText, o.IsCorrect
                FROM MockExamSetQuestions sq
                INNER JOIN Questions q ON q.QuestionId = sq.QuestionId
                INNER JOIN QuestionCategories c ON c.QuestionCategoryId = q.QuestionCategoryId
                INNER JOIN QuestionOptions o ON o.QuestionId = q.QuestionId
                WHERE sq.MockExamSetId = @SetId
                ORDER BY sq.DisplayOrder, o.OptionOrder",
                new SqlParameter("@SetId", setId));
        }

        /// <summary>
        /// True when every section has exactly its required number of active questions of the right category, and
        /// nothing else is selected. rows: CategoryCode + IsActive of each chosen question.
        /// </summary>
        public static bool IsComplete(MockExamType type, IEnumerable<DataRow> rows)
        {
            List<DataRow> list = rows.ToList();

            if (list.Count != type.TotalCount)
            {
                return false;
            }

            return type.Sections.All(section =>
                list.Count(r => Convert.ToString(r["CategoryCode"]) == section.CategoryCode && Convert.ToBoolean(r["IsActive"])) == section.Count);
        }

        public static bool IsComplete(int setId, string typeCode)
        {
            MockExamType type = MockExamRules.Find(typeCode);

            if (type == null)
            {
                return false;
            }

            DataTable rows = DatabaseHelper.ExecuteQuery(@"
                SELECT c.Code AS CategoryCode, q.IsActive
                FROM MockExamSetQuestions sq
                INNER JOIN Questions q ON q.QuestionId = sq.QuestionId
                INNER JOIN QuestionCategories c ON c.QuestionCategoryId = q.QuestionCategoryId
                WHERE sq.MockExamSetId = @SetId",
                new SqlParameter("@SetId", setId));

            return IsComplete(type, rows.Rows.Cast<DataRow>());
        }

        /// <summary>Set numbers of the type a learner may start now (Active and still complete), ascending.</summary>
        public static List<int> GetStartableSetNumbers(string typeCode)
        {
            MockExamType type = MockExamRules.Find(typeCode);
            List<int> numbers = new List<int>();

            if (type == null)
            {
                return numbers;
            }

            DataTable sets = DatabaseHelper.ExecuteQuery(
                "SELECT MockExamSetId, SetNumber FROM MockExamSets WHERE ExamTypeCode = @Type AND IsActive = 1 ORDER BY SetNumber",
                new SqlParameter("@Type", typeCode));

            if (sets.Rows.Count == 0)
            {
                return numbers;
            }

            ILookup<int, DataRow> selections = GetAllSelections().Rows.Cast<DataRow>()
                .ToLookup(r => Convert.ToInt32(r["MockExamSetId"]));

            foreach (DataRow set in sets.Rows)
            {
                if (IsComplete(type, selections[Convert.ToInt32(set["MockExamSetId"])]))
                {
                    numbers.Add(Convert.ToInt32(set["SetNumber"]));
                }
            }

            return numbers;
        }

        /// <summary>Activating requires a complete selection; returns false when that rule (or the id) fails.</summary>
        public static bool SetActive(int id, bool active)
        {
            DataRow set = GetById(id);

            if (set == null || (active && !IsComplete(id, Convert.ToString(set["ExamTypeCode"]))))
            {
                return false;
            }

            return DatabaseHelper.ExecuteNonQuery(
                "UPDATE MockExamSets SET IsActive = @Active, UpdatedAt = SYSUTCDATETIME() WHERE MockExamSetId = @Id",
                new SqlParameter("@Id", id),
                new SqlParameter("@Active", active)) == 1;
        }

        /// <summary>
        /// Saves a set and its questions in one step (Add and Edit Mock Exam Set). id is 0 for a new set and receives
        /// the saved set's id. questionIds may be incomplete (no section above its required count, active questions only);
        /// they are stored in section order (Colour Blind first), keeping the given order within a section.
        ///   - A new set gets the lowest unused number of its exam type, and is Active when its selection is complete.
        ///   - An existing set keeps its status, except that an incomplete selection makes it Inactive. Its exam type can
        ///     change until learners have taken it; it then gets the lowest unused number of the new type.
        /// Returns null when saved, otherwise a message for the administrator.
        /// </summary>
        public static string Save(ref int id, string typeCode, IList<int> questionIds, out bool complete, out bool deactivated)
        {
            complete = false;
            deactivated = false;

            MockExamType type = MockExamRules.Find(typeCode);

            if (type == null)
            {
                return "Please select an exam type.";
            }

            DataRow current = null;

            if (id != 0)
            {
                current = GetById(id);

                if (current == null)
                {
                    return "Mock Exam Set not found.";
                }

                if (Convert.ToString(current["ExamTypeCode"]) != type.Code && Convert.ToInt32(current["AttemptCount"]) > 0)
                {
                    return "The exam type can't be changed because learners have already taken this set.";
                }
            }

            List<int> ordered;
            string problem = OrderSelection(type, questionIds ?? new List<int>(), out ordered, out complete);

            if (problem != null)
            {
                return problem;
            }

            bool typeChanged = current != null && Convert.ToString(current["ExamTypeCode"]) != type.Code;
            bool wasActive = current != null && Convert.ToBoolean(current["IsActive"]);

            for (int attempt = 0; ; attempt++)
            {
                try
                {
                    id = SaveInTransaction(id, type.Code, current == null || typeChanged, current == null ? complete : wasActive && complete, ordered);
                    break;
                }
                catch (SqlException ex) when ((ex.Number == 2601 || ex.Number == 2627) && attempt == 0)
                {
                    // Another admin took the same number at the same moment; the retry picks the next free one.
                }
            }

            deactivated = wasActive && !complete;
            return null;
        }

        /// <summary>
        /// Checks a selection against the exam type and puts it in section order. Every question must be distinct, active
        /// and in one of the type's sections, with no section above its required count. complete: every section is full.
        /// </summary>
        private static string OrderSelection(MockExamType type, IList<int> questionIds, out List<int> ordered, out bool complete)
        {
            ordered = new List<int>();
            complete = false;

            if (questionIds.Distinct().Count() != questionIds.Count)
            {
                return "A question can only be chosen once.";
            }

            Dictionary<int, DataRow> choices = GetQuestionChoices().Rows.Cast<DataRow>()
                .ToDictionary(r => Convert.ToInt32(r["QuestionId"]));

            bool allFull = true;

            foreach (MockExamSection section in type.Sections)
            {
                List<int> inSection = questionIds.Where(q => choices.ContainsKey(q)
                    && Convert.ToString(choices[q]["CategoryCode"]) == section.CategoryCode).ToList();

                if (inSection.Any(q => !Convert.ToBoolean(choices[q]["IsActive"])))
                {
                    return section.Label + " contains an inactive question. Untick it or activate it in the Question Bank.";
                }

                if (inSection.Count > section.Count)
                {
                    return section.Label + " has " + inSection.Count + " questions selected, but " + type.Name + " needs only "
                        + section.Count + ". Untick " + (inSection.Count - section.Count) + ".";
                }

                allFull &= inSection.Count == section.Count;
                ordered.AddRange(inSection);
            }

            if (ordered.Count != questionIds.Count)
            {
                return "Some chosen questions do not belong to any section of this exam type.";
            }

            complete = allFull;
            return null;
        }

        /// <summary>
        /// Inserts or updates the set and replaces its questions in one transaction. With newNumber the set takes the
        /// lowest unused number of its type, chosen under UPDLOCK/HOLDLOCK so two admins never get the same one.
        /// </summary>
        private static int SaveInTransaction(int id, string typeCode, bool newNumber, bool active, List<int> ordered)
        {
            const string lowestFreeNumber = @"
                (SELECT MIN(n.Number)
                 FROM (SELECT 1 AS Number
                       UNION ALL
                       SELECT SetNumber + 1 FROM MockExamSets WITH (UPDLOCK, HOLDLOCK) WHERE ExamTypeCode = @Type) n
                 WHERE NOT EXISTS (SELECT 1 FROM MockExamSets s
                                   WHERE s.ExamTypeCode = @Type AND s.SetNumber = n.Number AND s.MockExamSetId <> @Id))";

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string sql = id == 0
                            ? @"INSERT INTO MockExamSets (ExamTypeCode, SetNumber, IsActive, CreatedAt, UpdatedAt)
                                OUTPUT INSERTED.MockExamSetId
                                VALUES (@Type, " + lowestFreeNumber + @", @Active, SYSUTCDATETIME(), SYSUTCDATETIME());"
                            : @"UPDATE MockExamSets
                                SET ExamTypeCode = @Type, IsActive = @Active, UpdatedAt = SYSUTCDATETIME()"
                                + (newNumber ? ", SetNumber = " + lowestFreeNumber : "") + @"
                                WHERE MockExamSetId = @Id;
                                SELECT @Id;";

                        using (SqlCommand command = new SqlCommand(sql, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@Id", id);
                            command.Parameters.AddWithValue("@Type", typeCode);
                            command.Parameters.AddWithValue("@Active", active);
                            id = Convert.ToInt32(command.ExecuteScalar());
                        }

                        using (SqlCommand command = new SqlCommand("DELETE FROM MockExamSetQuestions WHERE MockExamSetId = @Id", connection, transaction))
                        {
                            command.Parameters.AddWithValue("@Id", id);
                            command.ExecuteNonQuery();
                        }

                        for (int i = 0; i < ordered.Count; i++)
                        {
                            using (SqlCommand command = new SqlCommand(
                                "INSERT INTO MockExamSetQuestions (MockExamSetId, QuestionId, DisplayOrder) VALUES (@Id, @QuestionId, @Order)",
                                connection, transaction))
                            {
                                command.Parameters.AddWithValue("@Id", id);
                                command.Parameters.AddWithValue("@QuestionId", ordered[i]);
                                command.Parameters.AddWithValue("@Order", i + 1);
                                command.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        return id;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Deletes a set and its question list, in one transaction; the questions themselves stay in the Question Bank.
        /// A set that learners have taken is never deleted (HasResults): it is deactivated instead, so their results stay.
        /// </summary>
        public static MockExamSetDeleteResult Delete(int id)
        {
            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        if (Count(connection, transaction, "SELECT COUNT(*) FROM MockExamSets WHERE MockExamSetId = @Id", id) == 0)
                        {
                            transaction.Rollback();
                            return MockExamSetDeleteResult.NotFound;
                        }

                        // Locked so no attempt can be saved between this check and the delete.
                        if (Count(connection, transaction, "SELECT COUNT(*) FROM MockExamAttempts WITH (UPDLOCK, HOLDLOCK) WHERE MockExamSetId = @Id", id) > 0)
                        {
                            transaction.Rollback();
                            return MockExamSetDeleteResult.HasResults;
                        }

                        using (SqlCommand command = new SqlCommand(@"
                            DELETE FROM MockExamSetQuestions WHERE MockExamSetId = @Id;
                            DELETE FROM MockExamSets WHERE MockExamSetId = @Id;",
                            connection, transaction))
                        {
                            command.Parameters.AddWithValue("@Id", id);
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return MockExamSetDeleteResult.Deleted;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Keeps "an Active set is complete": after a question was deactivated, every Active set using it becomes
        /// Inactive. Returns the names of the sets that changed (for the administrator's message).
        /// </summary>
        public static List<string> DeactivateSetsUsingQuestion(int questionId)
        {
            DataTable changed = DatabaseHelper.ExecuteQuery(@"
                UPDATE s SET IsActive = 0, UpdatedAt = SYSUTCDATETIME()
                OUTPUT INSERTED.ExamTypeCode, INSERTED.SetNumber
                FROM MockExamSets s
                WHERE s.IsActive = 1
                  AND EXISTS (SELECT 1 FROM MockExamSetQuestions sq WHERE sq.MockExamSetId = s.MockExamSetId AND sq.QuestionId = @QuestionId);",
                new SqlParameter("@QuestionId", questionId));

            return changed.Rows.Cast<DataRow>()
                .Select(r => "Mock Exam Set " + r["SetNumber"] + " (" + TypeName(Convert.ToString(r["ExamTypeCode"])) + ")")
                .ToList();
        }

        public static string TypeName(string typeCode)
        {
            MockExamType type = MockExamRules.Find(typeCode);
            return type == null ? typeCode : type.Name;
        }

        private static int Count(SqlConnection connection, SqlTransaction transaction, string sql, int id)
        {
            using (SqlCommand command = new SqlCommand(sql, connection, transaction))
            {
                command.Parameters.AddWithValue("@Id", id);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }
    }
}
