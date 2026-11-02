using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace MyKPPLearn.Data
{
    /// <summary>One question of a Mock Exam paper, in exam order, with its three options (A, B, C).</summary>
    public sealed class MockExamPaperQuestion
    {
        public int QuestionId;
        public MockExamSection Section;
        public string CategoryCode;
        public string Text;
        public string Explanation;
        public string ImagePath;
        public string ImageAltText;
        public bool IsActive;
        public int[] OptionIds = new int[3];
        public string[] Options = new string[3];
        public int CorrectIndex = -1;
    }

    /// <summary>The marked result of one exam: section scores and the pass decision.</summary>
    public sealed class MockExamMarking
    {
        public int ColourBlindCorrect;
        public int ColourBlindTotal;
        public int TheoryCorrect;
        public int TheoryTotal;
        public bool ColourBlindPassed;
        public bool TheoryPassed;
        public bool Passed;
        public Dictionary<string, int> CorrectBySection = new Dictionary<string, int>();
    }

    /// <summary>
    /// Learner Mock Exam attempts. The paper and the marking come from the database (never from the browser), and a
    /// completed attempt is saved in one transaction with a snapshot of every question as the learner saw it, so the
    /// result and review stay the same if the Question Bank is edited later. All queries are parameterized.
    /// </summary>
    public static class MockExamAttemptRepository
    {
        /// <summary>The set's questions in exam order (empty when the set has none).</summary>
        public static List<MockExamPaperQuestion> GetPaper(int setId, MockExamType type)
        {
            List<MockExamPaperQuestion> paper = new List<MockExamPaperQuestion>();
            MockExamPaperQuestion current = null;

            foreach (DataRow row in MockExamSetRepository.GetPaper(setId).Rows)
            {
                int questionId = Convert.ToInt32(row["QuestionId"]);

                if (current == null || current.QuestionId != questionId)
                {
                    string category = Convert.ToString(row["CategoryCode"]);
                    current = new MockExamPaperQuestion
                    {
                        QuestionId = questionId,
                        CategoryCode = category,
                        Section = MockExamRules.SectionForCategory(type, category),
                        Text = Convert.ToString(row["QuestionText"]),
                        Explanation = Convert.ToString(row["Explanation"]),
                        ImagePath = row.IsNull("ImagePath") ? null : Convert.ToString(row["ImagePath"]),
                        ImageAltText = row.IsNull("ImageAltText") ? null : Convert.ToString(row["ImageAltText"]),
                        IsActive = Convert.ToBoolean(row["IsActive"])
                    };
                    paper.Add(current);
                }

                int index = Convert.ToInt32(row["OptionOrder"]) - 1;

                if (index >= 0 && index < 3)
                {
                    current.OptionIds[index] = Convert.ToInt32(row["QuestionOptionId"]);
                    current.Options[index] = Convert.ToString(row["OptionText"]);

                    if (Convert.ToBoolean(row["IsCorrect"]))
                    {
                        current.CorrectIndex = index;
                    }
                }
            }

            return paper;
        }

        /// <summary>Marks the answers (index 0-2 per question, or -1 when unanswered) against the exam type's pass marks.</summary>
        public static MockExamMarking Mark(MockExamType type, IList<MockExamPaperQuestion> paper, IList<int> answers)
        {
            MockExamMarking marking = new MockExamMarking();

            foreach (MockExamSection section in type.Sections)
            {
                marking.CorrectBySection[section.Id] = 0;
            }

            for (int i = 0; i < paper.Count; i++)
            {
                MockExamPaperQuestion question = paper[i];
                bool colourBlind = question.Section != null && question.Section.IsColourBlind;
                bool correct = answers[i] >= 0 && answers[i] == question.CorrectIndex;

                if (colourBlind) { marking.ColourBlindTotal++; } else { marking.TheoryTotal++; }

                if (correct)
                {
                    if (colourBlind) { marking.ColourBlindCorrect++; } else { marking.TheoryCorrect++; }

                    if (question.Section != null)
                    {
                        marking.CorrectBySection[question.Section.Id]++;
                    }
                }
            }

            marking.ColourBlindPassed = marking.ColourBlindCorrect >= type.ColourBlindPass;
            marking.TheoryPassed = marking.TheoryCorrect >= type.TheoryPass;
            marking.Passed = marking.ColourBlindPassed && marking.TheoryPassed;
            return marking;
        }

        /// <summary>Saves a completed attempt with its question snapshots and flags; returns the new attempt id.</summary>
        public static int Save(int userId, int setId, DateTime startedUtc, DateTime completedUtc, int timeTakenSeconds, bool autoSubmitted,
            IList<MockExamPaperQuestion> paper, IList<int> answers, IList<bool> flags, MockExamMarking marking)
        {
            const string attemptSql = @"
                INSERT INTO MockExamAttempts (UserId, MockExamSetId, StartedAt, CompletedAt, ColourBlindCorrect, ColourBlindTotal,
                                              TheoryCorrect, TheoryTotal, IsPassed, TimeTakenSeconds, AutoSubmitted)
                OUTPUT INSERTED.MockExamAttemptId
                VALUES (@UserId, @SetId, @StartedAt, @CompletedAt, @CbCorrect, @CbTotal, @ThCorrect, @ThTotal, @Passed, @Seconds, @Auto);";

            const string questionSql = @"
                INSERT INTO MockExamAttemptQuestions (MockExamAttemptId, QuestionId, QuestionNumber, SelectedOptionId, IsCorrect, IsFlagged,
                    QuestionTextSnapshot, ExplanationSnapshot, OptionATextSnapshot, OptionBTextSnapshot, OptionCTextSnapshot,
                    CorrectOptionSnapshot, SelectedOptionSnapshot, SelectedOptionTextSnapshot, ImagePathSnapshot, ImageAltTextSnapshot,
                    QuestionCategoryCodeSnapshot)
                VALUES (@AttemptId, @QuestionId, @Number, @SelectedOptionId, @IsCorrect, @IsFlagged,
                    @Text, @Explanation, @OptionA, @OptionB, @OptionC,
                    @Correct, @Selected, @SelectedText, @ImagePath, @ImageAlt,
                    @Category);";

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        int attemptId;

                        using (SqlCommand command = new SqlCommand(attemptSql, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@UserId", userId);
                            command.Parameters.AddWithValue("@SetId", setId);
                            command.Parameters.AddWithValue("@StartedAt", startedUtc);
                            command.Parameters.AddWithValue("@CompletedAt", completedUtc);
                            command.Parameters.AddWithValue("@CbCorrect", marking.ColourBlindCorrect);
                            command.Parameters.AddWithValue("@CbTotal", marking.ColourBlindTotal);
                            command.Parameters.AddWithValue("@ThCorrect", marking.TheoryCorrect);
                            command.Parameters.AddWithValue("@ThTotal", marking.TheoryTotal);
                            command.Parameters.AddWithValue("@Passed", marking.Passed);
                            command.Parameters.AddWithValue("@Seconds", timeTakenSeconds);
                            command.Parameters.AddWithValue("@Auto", autoSubmitted);
                            attemptId = Convert.ToInt32(command.ExecuteScalar());
                        }

                        for (int i = 0; i < paper.Count; i++)
                        {
                            MockExamPaperQuestion q = paper[i];
                            int answer = answers[i];
                            bool answered = answer >= 0 && answer < 3;
                            bool hasImage = !string.IsNullOrEmpty(q.ImagePath);

                            using (SqlCommand command = new SqlCommand(questionSql, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@AttemptId", attemptId);
                                command.Parameters.AddWithValue("@QuestionId", q.QuestionId);
                                command.Parameters.AddWithValue("@Number", i + 1);
                                command.Parameters.AddWithValue("@SelectedOptionId", answered ? (object)q.OptionIds[answer] : DBNull.Value);
                                command.Parameters.AddWithValue("@IsCorrect", answered && answer == q.CorrectIndex);
                                command.Parameters.AddWithValue("@IsFlagged", flags != null && i < flags.Count && flags[i]);
                                command.Parameters.AddWithValue("@Text", q.Text);
                                command.Parameters.AddWithValue("@Explanation", q.Explanation);
                                command.Parameters.AddWithValue("@OptionA", q.Options[0] ?? string.Empty);
                                command.Parameters.AddWithValue("@OptionB", q.Options[1] ?? string.Empty);
                                command.Parameters.AddWithValue("@OptionC", q.Options[2] ?? string.Empty);
                                command.Parameters.AddWithValue("@Correct", Letter(q.CorrectIndex));
                                command.Parameters.AddWithValue("@Selected", answered ? (object)Letter(answer) : DBNull.Value);
                                command.Parameters.AddWithValue("@SelectedText", answered ? (object)(q.Options[answer] ?? string.Empty) : DBNull.Value);
                                command.Parameters.AddWithValue("@ImagePath", hasImage ? (object)q.ImagePath : DBNull.Value);
                                command.Parameters.AddWithValue("@ImageAlt", hasImage ? (object)q.ImageAltText : DBNull.Value);
                                command.Parameters.AddWithValue("@Category", q.CategoryCode);
                                command.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        return attemptId;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private static string Letter(int index)
        {
            return "ABC".Substring(Math.Max(0, Math.Min(2, index)), 1);
        }
    }
}
