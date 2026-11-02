using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Services;
using MyKPPLearn.Common;
using MyKPPLearn.Data;
using MyKPPLearn.Security;

namespace MyKPPLearn.Admin
{
    /// <summary>
    /// Admin Question Bank. The page markup is a shell; Scripts/practice-admin-db.js calls the page methods below
    /// (JSON over POST) to list, add, edit, activate/deactivate and delete questions. Every method re-checks the
    /// Administrator role because page methods bypass Page_Load.
    /// </summary>
    public partial class QuestionBank : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SessionHelper.RequireAdministrator(this);
            ((AdminMaster)Master).PageHeading = "Question Bank";
        }

        public sealed class QuestionDto
        {
            public int id;
            public int categoryId;
            public int? setId;
            public string text;
            public string explanation;
            public string image;
            public string imageAlt;
            public string status;
            public string[] options;
            public int correctIndex;
            public int attemptCount;
            public int mockExamUseCount;
            public string error;
            public string notice;
        }

        public sealed class QuestionDataDto
        {
            public List<PracticeSets.CategoryDto> categories;
            public List<PracticeSets.SetDto> sets;
            public List<QuestionDto> questions;
        }

        [WebMethod(EnableSession = true)]
        public static QuestionDataDto LoadData()
        {
            PracticeSets.RequireAdmin();

            // All options in one query, grouped by question, instead of one query per question.
            Dictionary<int, DataRow[]> optionsByQuestion = new Dictionary<int, DataRow[]>();

            foreach (DataRow option in QuestionRepository.GetAllOptions().Rows)
            {
                int questionId = Convert.ToInt32(option["QuestionId"]);
                DataRow[] options;

                if (!optionsByQuestion.TryGetValue(questionId, out options))
                {
                    options = new DataRow[3];
                    optionsByQuestion[questionId] = options;
                }

                int index = Convert.ToInt32(option["OptionOrder"]) - 1;

                if (index >= 0 && index < 3)
                {
                    options[index] = option;
                }
            }

            QuestionDataDto output = new QuestionDataDto
            {
                categories = PracticeSets.LoadCategories(),
                sets = PracticeSets.LoadSets(),
                questions = new List<QuestionDto>()
            };

            foreach (DataRow row in QuestionRepository.GetAllForAdmin().Rows)
            {
                DataRow[] options;
                optionsByQuestion.TryGetValue(Convert.ToInt32(row["QuestionId"]), out options);
                output.questions.Add(ToQuestion(row, options ?? new DataRow[3]));
            }

            return output;
        }

        [WebMethod(EnableSession = true)]
        public static QuestionDto Get(int id)
        {
            PracticeSets.RequireAdmin();

            DataRow row = QuestionRepository.GetById(id);
            return row == null ? new QuestionDto { error = "Question not found." } : ToQuestion(row);
        }

        /// <summary>
        /// Saves a question. The image is either a new upload (a data URL, stored by QuestionImageStorage),
        /// the question's current path (unchanged), or empty (no image / removed).
        /// </summary>
        [WebMethod(EnableSession = true)]
        public static QuestionDto Save(QuestionDto question)
        {
            PracticeSets.RequireAdmin();

            string oldPath = null;
            string newPath = null;
            int? oldSetId = null;
            bool uploaded = false;

            try
            {
                if (question == null)
                {
                    return new QuestionDto { error = "Question data is required." };
                }

                if (question.id != 0)
                {
                    DataRow previous = QuestionRepository.GetById(question.id);

                    if (previous == null)
                    {
                        return new QuestionDto { error = "Question not found." };
                    }

                    oldPath = previous.IsNull("ImagePath") ? null : Convert.ToString(previous["ImagePath"]);
                    oldSetId = previous.IsNull("PracticeSetId") ? (int?)null : Convert.ToInt32(previous["PracticeSetId"]);

                    // A Mock Exam section takes its questions from one category, so a question in a Mock Exam Set keeps its category.
                    if (Convert.ToInt32(previous["QuestionCategoryId"]) != question.categoryId && Convert.ToInt32(previous["MockExamUseCount"]) > 0)
                    {
                        return new QuestionDto { error = "This question's category can't change while it is used in a Mock Exam Set. Remove it from those sets first." };
                    }
                }

                if (!string.IsNullOrEmpty(question.image) && question.image.StartsWith("data:image/", StringComparison.Ordinal))
                {
                    newPath = QuestionImageStorage.SaveDataUrl(question.image);
                    uploaded = true;
                }
                else if (!string.IsNullOrEmpty(question.image))
                {
                    // Only the question's own stored path may be sent back; any other path is refused.
                    if (!string.Equals(question.image, oldPath, StringComparison.Ordinal))
                    {
                        return new QuestionDto { error = "The image could not be found. Please upload it again." };
                    }

                    newPath = oldPath;
                }

                QuestionInput input = new QuestionInput
                {
                    QuestionId = question.id,
                    CategoryId = question.categoryId,
                    PracticeSetId = question.setId,
                    QuestionText = question.text,
                    Explanation = question.explanation,
                    ImagePath = newPath,
                    ImageAltText = question.imageAlt,
                    Options = question.options,
                    CorrectOption = question.correctIndex + 1
                };

                int id = QuestionRepository.Save(input);

                if (oldPath != null && !string.Equals(oldPath, newPath, StringComparison.Ordinal))
                {
                    QuestionImageStorage.DeleteIfUnreferenced(oldPath);
                }

                QuestionDto saved = ToQuestion(QuestionRepository.GetById(id));

                // Moving the question out of a set may leave that set Active with no active questions.
                if (oldSetId.HasValue && oldSetId != question.setId)
                {
                    saved.notice = DeactivatedNotice(oldSetId.Value);
                }

                return saved;
            }
            catch (ArgumentException ex)
            {
                if (uploaded)
                {
                    QuestionImageStorage.DeleteIfUnreferenced(newPath);
                }

                return new QuestionDto { error = ex.Message };
            }
            catch (Exception)
            {
                if (uploaded)
                {
                    QuestionImageStorage.DeleteIfUnreferenced(newPath);
                }

                return new QuestionDto { error = "The question could not be saved. Please try again." };
            }
        }

        [WebMethod(EnableSession = true)]
        public static QuestionDto Toggle(int id, bool active)
        {
            PracticeSets.RequireAdmin();

            if (!QuestionRepository.SetActive(id, active))
            {
                return new QuestionDto { error = "Question not found." };
            }

            QuestionDto question = ToQuestion(QuestionRepository.GetById(id));

            if (!active)
            {
                List<string> notices = new List<string>();

                if (question.setId.HasValue)
                {
                    string practiceNotice = DeactivatedNotice(question.setId.Value);
                    if (practiceNotice != null) { notices.Add(practiceNotice); }
                }

                // An Active Mock Exam Set must keep a complete paper of active questions.
                List<string> mockSets = MockExamSetRepository.DeactivateSetsUsingQuestion(id);
                if (mockSets.Count > 0)
                {
                    notices.Add(string.Join(", ", mockSets) + (mockSets.Count == 1 ? " is" : " are") + " now Inactive because it uses this question.");
                }

                question.notice = notices.Count > 0 ? string.Join(" ", notices) : null;
            }

            return question;
        }

        /// <summary>Returns "Deleted" (optionally followed by a notice) or a message explaining why it was not deleted.</summary>
        [WebMethod(EnableSession = true)]
        public static string Delete(int id)
        {
            PracticeSets.RequireAdmin();

            DataRow existing = QuestionRepository.GetById(id);
            string imagePath = existing == null || existing.IsNull("ImagePath") ? null : Convert.ToString(existing["ImagePath"]);
            int? setId = existing == null || existing.IsNull("PracticeSetId") ? (int?)null : Convert.ToInt32(existing["PracticeSetId"]);

            switch (QuestionRepository.Delete(id))
            {
                case QuestionDeleteResult.Deleted:
                    QuestionImageStorage.DeleteIfUnreferenced(imagePath);
                    string notice = setId.HasValue ? DeactivatedNotice(setId.Value) : null;
                    return notice == null ? "Deleted" : "Deleted " + notice;
                case QuestionDeleteResult.HasAttempts:
                    return "This question is protected because it appears in completed learner attempts.";
                case QuestionDeleteResult.UsedByMockExam:
                    return "Remove this question from its Mock Exam Sets before deleting it.";
                default:
                    return "Question not found.";
            }
        }

        /// <summary>Deactivates the set if it has no active question left, and says so; otherwise null.</summary>
        private static string DeactivatedNotice(int setId)
        {
            if (!PracticeSetRepository.DeactivateIfNoActiveQuestions(setId))
            {
                return null;
            }

            DataRow set = PracticeSetRepository.GetById(setId);
            string name = set == null ? "Its Practice Set" : "Practice Set " + set["SetNumber"] + " (" + set["CategoryTitle"] + ")";
            return name + " is now Inactive because it has no active questions.";
        }

        private static QuestionDto ToQuestion(DataRow row)
        {
            DataRow[] options = new DataRow[3];

            foreach (DataRow option in QuestionRepository.GetOptions(Convert.ToInt32(row["QuestionId"])).Rows)
            {
                int index = Convert.ToInt32(option["OptionOrder"]) - 1;

                if (index >= 0 && index < 3)
                {
                    options[index] = option;
                }
            }

            return ToQuestion(row, options);
        }

        private static QuestionDto ToQuestion(DataRow row, DataRow[] options)
        {
            string[] texts = new string[3];
            int correct = -1;

            for (int i = 0; i < 3; i++)
            {
                if (options[i] == null)
                {
                    continue;
                }

                texts[i] = Convert.ToString(options[i]["OptionText"]);

                if (Convert.ToBoolean(options[i]["IsCorrect"]))
                {
                    correct = i;
                }
            }

            return new QuestionDto
            {
                id = Convert.ToInt32(row["QuestionId"]),
                categoryId = Convert.ToInt32(row["QuestionCategoryId"]),
                setId = row.IsNull("PracticeSetId") ? (int?)null : Convert.ToInt32(row["PracticeSetId"]),
                text = Convert.ToString(row["QuestionText"]),
                explanation = Convert.ToString(row["Explanation"]),
                image = row.IsNull("ImagePath") ? null : Convert.ToString(row["ImagePath"]),
                imageAlt = row.IsNull("ImageAltText") ? null : Convert.ToString(row["ImageAltText"]),
                status = Convert.ToBoolean(row["IsActive"]) ? "Active" : "Inactive",
                options = texts,
                correctIndex = correct,
                attemptCount = Convert.ToInt32(row["AttemptCount"]),
                mockExamUseCount = Convert.ToInt32(row["MockExamUseCount"])
            };
        }
    }
}
