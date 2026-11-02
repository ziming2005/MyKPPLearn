using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace MyKPPLearn.Data
{
    public enum MockExamSetSaveResult { Saved, NotFound, DuplicateNumber, TypeLocked }

    public enum MockExamSetDeleteResult { Deleted, NotFound, HasAttempts }

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
                   (SELECT COUNT(*) FROM MockExamAttempts a WHERE a.MockExamSetId = s.MockExamSetId) AS AttemptCount
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
                       q.ImagePath, q.ImageAltText, q.IsActive,
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

        /// <summary>New sets start Inactive: a set can only be activated once its question selection is complete.</summary>
        public static MockExamSetSaveResult Create(string typeCode, int setNumber, out int id)
        {
            id = 0;

            try
            {
                id = Convert.ToInt32(DatabaseHelper.ExecuteScalar(@"
                    INSERT INTO MockExamSets (ExamTypeCode, SetNumber, IsActive, CreatedAt, UpdatedAt)
                    OUTPUT INSERTED.MockExamSetId
                    VALUES (@Type, @Number, 0, SYSUTCDATETIME(), SYSUTCDATETIME());",
                    new SqlParameter("@Type", typeCode),
                    new SqlParameter("@Number", setNumber)));

                return MockExamSetSaveResult.Saved;
            }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
            {
                return MockExamSetSaveResult.DuplicateNumber;
            }
        }

        /// <summary>
        /// Changes a set's exam type and number. The type cannot change once learners have attempted the set (their
        /// results would change meaning). Changing the type keeps, per section, only as many chosen questions as the new
        /// type needs; if the selection is then incomplete the set becomes Inactive (deactivated = true).
        /// </summary>
        public static MockExamSetSaveResult Update(int id, string typeCode, int setNumber, out bool deactivated)
        {
            deactivated = false;
            DataRow current = GetById(id);

            if (current == null)
            {
                return MockExamSetSaveResult.NotFound;
            }

            bool typeChanged = Convert.ToString(current["ExamTypeCode"]) != typeCode;

            if (typeChanged && Convert.ToInt32(current["AttemptCount"]) > 0)
            {
                return MockExamSetSaveResult.TypeLocked;
            }

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand command = new SqlCommand(
                            "UPDATE MockExamSets SET ExamTypeCode = @Type, SetNumber = @Number, UpdatedAt = SYSUTCDATETIME() WHERE MockExamSetId = @Id",
                            connection, transaction))
                        {
                            command.Parameters.AddWithValue("@Type", typeCode);
                            command.Parameters.AddWithValue("@Number", setNumber);
                            command.Parameters.AddWithValue("@Id", id);
                            command.ExecuteNonQuery();
                        }

                        if (typeChanged)
                        {
                            TrimToType(connection, transaction, id, MockExamRules.Find(typeCode));
                        }

                        transaction.Commit();
                    }
                    catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
                    {
                        transaction.Rollback();
                        return MockExamSetSaveResult.DuplicateNumber;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }

            if (typeChanged && Convert.ToBoolean(current["IsActive"]) && !IsComplete(id, typeCode))
            {
                SetActive(id, false);
                deactivated = true;
            }

            return MockExamSetSaveResult.Saved;
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
        /// Replaces a set's questions. questionIds must hold exactly the required number of active questions for every
        /// section; they are stored in section order (Colour Blind first), keeping the given order within a section.
        /// Returns null when saved, otherwise a message for the administrator.
        /// </summary>
        public static string SaveQuestions(int id, IList<int> questionIds)
        {
            DataRow set = GetById(id);

            if (set == null)
            {
                return "Mock Exam Set not found.";
            }

            MockExamType type = MockExamRules.Find(Convert.ToString(set["ExamTypeCode"]));
            questionIds = questionIds ?? new List<int>();

            if (questionIds.Distinct().Count() != questionIds.Count)
            {
                return "A question can only be chosen once.";
            }

            Dictionary<int, DataRow> choices = GetQuestionChoices().Rows.Cast<DataRow>()
                .ToDictionary(r => Convert.ToInt32(r["QuestionId"]));

            List<int> ordered = new List<int>();
            List<string> problems = new List<string>();

            foreach (MockExamSection section in type.Sections)
            {
                List<int> inSection = questionIds.Where(q => choices.ContainsKey(q)
                    && Convert.ToString(choices[q]["CategoryCode"]) == section.CategoryCode).ToList();

                if (inSection.Any(q => !Convert.ToBoolean(choices[q]["IsActive"])))
                {
                    return section.Label + " contains an inactive question. Untick it or activate it in the Question Bank.";
                }

                if (inSection.Count != section.Count)
                {
                    problems.Add(section.Label + ": " + inSection.Count + " of " + section.Count);
                }

                ordered.AddRange(inSection);
            }

            if (ordered.Count != questionIds.Count)
            {
                return "Some chosen questions do not belong to any section of this exam type.";
            }

            if (problems.Count > 0)
            {
                return "Selection incomplete. Select exactly the required number in every section: " + string.Join("; ", problems) + ".";
            }

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
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
                        return null;
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
        /// Deletes a set and its question list. Refused when learners have attempted it, so their results stay intact;
        /// deactivate it instead.
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

                        if (Count(connection, transaction, "SELECT COUNT(*) FROM MockExamAttempts WITH (UPDLOCK, HOLDLOCK) WHERE MockExamSetId = @Id", id) > 0)
                        {
                            transaction.Rollback();
                            return MockExamSetDeleteResult.HasAttempts;
                        }

                        using (SqlCommand command = new SqlCommand(
                            "DELETE FROM MockExamSetQuestions WHERE MockExamSetId = @Id; DELETE FROM MockExamSets WHERE MockExamSetId = @Id;",
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

        /// <summary>After a type change: per section keep the first N chosen questions (N = the new required count).</summary>
        private static void TrimToType(SqlConnection connection, SqlTransaction transaction, int id, MockExamType type)
        {
            DataTable rows = new DataTable();

            using (SqlCommand command = new SqlCommand(@"
                SELECT sq.QuestionId, c.Code AS CategoryCode
                FROM MockExamSetQuestions sq
                INNER JOIN Questions q ON q.QuestionId = sq.QuestionId
                INNER JOIN QuestionCategories c ON c.QuestionCategoryId = q.QuestionCategoryId
                WHERE sq.MockExamSetId = @Id
                ORDER BY sq.DisplayOrder", connection, transaction))
            {
                command.Parameters.AddWithValue("@Id", id);

                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    adapter.Fill(rows);
                }
            }

            List<int> keep = new List<int>();

            foreach (MockExamSection section in type.Sections)
            {
                keep.AddRange(rows.Rows.Cast<DataRow>()
                    .Where(r => Convert.ToString(r["CategoryCode"]) == section.CategoryCode)
                    .Take(section.Count)
                    .Select(r => Convert.ToInt32(r["QuestionId"])));
            }

            using (SqlCommand command = new SqlCommand("DELETE FROM MockExamSetQuestions WHERE MockExamSetId = @Id", connection, transaction))
            {
                command.Parameters.AddWithValue("@Id", id);
                command.ExecuteNonQuery();
            }

            for (int i = 0; i < keep.Count; i++)
            {
                using (SqlCommand command = new SqlCommand(
                    "INSERT INTO MockExamSetQuestions (MockExamSetId, QuestionId, DisplayOrder) VALUES (@Id, @QuestionId, @Order)",
                    connection, transaction))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@QuestionId", keep[i]);
                    command.Parameters.AddWithValue("@Order", i + 1);
                    command.ExecuteNonQuery();
                }
            }
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
