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

            QuestionDataDto output = new QuestionDataDto
            {
                categories = PracticeSets.LoadCategories(),
                sets = PracticeSets.LoadSets(),
                questions = new List<QuestionDto>()
            };

            foreach (DataRow row in QuestionRepository.GetAllForAdmin().Rows)
            {
                output.questions.Add(ToQuestion(row));
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

                return ToQuestion(QuestionRepository.GetById(id));
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

            return ToQuestion(QuestionRepository.GetById(id));
        }

        [WebMethod(EnableSession = true)]
        public static string Delete(int id)
        {
            PracticeSets.RequireAdmin();

            switch (QuestionRepository.Delete(id))
            {
                case QuestionDeleteResult.Deleted:
                    return "Deleted";
                case QuestionDeleteResult.HasAttempts:
                    return "This question is protected because it appears in completed learner attempts.";
                case QuestionDeleteResult.UsedByMockExam:
                    return "Remove this question from its Mock Exam Sets before deleting it.";
                default:
                    return "Question not found.";
            }
        }

        private static QuestionDto ToQuestion(DataRow row)
        {
            int id = Convert.ToInt32(row["QuestionId"]);
            string[] options = new string[3];
            int correct = -1;

            foreach (DataRow option in QuestionRepository.GetOptions(id).Rows)
            {
                int index = Convert.ToInt32(option["OptionOrder"]) - 1;
                options[index] = Convert.ToString(option["OptionText"]);

                if (Convert.ToBoolean(option["IsCorrect"]))
                {
                    correct = index;
                }
            }

            return new QuestionDto
            {
                id = id,
                categoryId = Convert.ToInt32(row["QuestionCategoryId"]),
                setId = row.IsNull("PracticeSetId") ? (int?)null : Convert.ToInt32(row["PracticeSetId"]),
                text = Convert.ToString(row["QuestionText"]),
                explanation = Convert.ToString(row["Explanation"]),
                image = row.IsNull("ImagePath") ? null : Convert.ToString(row["ImagePath"]),
                imageAlt = row.IsNull("ImageAltText") ? null : Convert.ToString(row["ImageAltText"]),
                status = Convert.ToBoolean(row["IsActive"]) ? "Active" : "Inactive",
                options = options,
                correctIndex = correct,
                attemptCount = Convert.ToInt32(row["AttemptCount"]),
                mockExamUseCount = Convert.ToInt32(row["MockExamUseCount"])
            };
        }
    }
}
