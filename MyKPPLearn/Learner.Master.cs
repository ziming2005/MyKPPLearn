using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Script.Serialization;
using System.Web.UI.HtmlControls;
using MyKPPLearn.Common;
using MyKPPLearn.Data;
using MyKPPLearn.Security;

namespace MyKPPLearn
{
    public partial class LearnerMaster : System.Web.UI.MasterPage
    {
        public string PageHeading { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            SessionHelper.RequireLearner(this.Page);

            string fullName = SessionHelper.CurrentFullName(Session);

            litFullName.Text = fullName;
            litRole.Text = SessionHelper.CurrentRole(Session);
            ProfileAvatar.Bind(Session, fullName, litAvatarInitials, imgAvatar);

            SetActiveNav();
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(PageHeading))
            {
                litPageTitle.Text = PageHeading;
            }
        }

        /// <summary>
        /// The Standard Practice data the learner practice pages (Scripts/practice.js) need, as JSON for
        /// window.PracticeDatabaseData: active sets and their active questions with options. Other pages get "null".
        /// </summary>
        protected string PracticeDataJson()
        {
            string page = System.IO.Path.GetFileName(Request.Path);

            if (!IsPage(page, "Practice.aspx") && !IsPage(page, "PracticeSets.aspx") && !IsPage(page, "PracticeAttempt.aspx"))
            {
                return "null";
            }

            var sets = new List<object>();

            foreach (DataRow row in PracticeSetRepository.GetActiveForLearner().Rows)
            {
                sets.Add(new
                {
                    id = Convert.ToInt32(row["PracticeSetId"]),
                    categoryId = Convert.ToString(row["CategoryCode"]),
                    number = Convert.ToInt32(row["SetNumber"]),
                    questionCount = Convert.ToInt32(row["ActiveQuestionCount"]),
                    status = "Active"
                });
            }

            // One row per option (three per question), already ordered by question then option.
            var questions = new List<object>();
            int currentId = -1;
            List<string> options = null;
            int correctIndex = -1;
            DataRow first = null;

            foreach (DataRow row in QuestionRepository.GetActiveForLearnerPractice().Rows)
            {
                int questionId = Convert.ToInt32(row["QuestionId"]);

                if (questionId != currentId)
                {
                    if (first != null)
                    {
                        questions.Add(ToPracticeQuestion(first, options, correctIndex));
                    }

                    currentId = questionId;
                    first = row;
                    options = new List<string>();
                    correctIndex = -1;
                }

                if (Convert.ToBoolean(row["IsCorrect"]))
                {
                    correctIndex = options.Count;
                }

                options.Add(Convert.ToString(row["OptionText"]));
            }

            if (first != null)
            {
                questions.Add(ToPracticeQuestion(first, options, correctIndex));
            }

            // "<" is escaped so question text can never close the inline <script> block.
            return new JavaScriptSerializer { MaxJsonLength = int.MaxValue }
                .Serialize(new { sets = sets, questions = questions })
                .Replace("<", "\\u003c");
        }

        private object ToPracticeQuestion(DataRow row, List<string> options, int correctIndex)
        {
            string image = row.IsNull("ImagePath") ? null : Convert.ToString(row["ImagePath"]);

            return new
            {
                id = Convert.ToInt32(row["QuestionId"]),
                setId = Convert.ToInt32(row["PracticeSetId"]),
                categoryId = Convert.ToString(row["CategoryCode"]),
                text = Convert.ToString(row["QuestionText"]),
                options = options,
                correctIndex = correctIndex,
                explanation = Convert.ToString(row["Explanation"]),
                image = string.IsNullOrEmpty(image) ? "" : ResolveUrl("~/" + image),
                status = "Active"
            };
        }

        private void SetActiveNav()
        {
            string currentPage = System.IO.Path.GetFileName(Request.Path);

            ApplyActiveState(navDashboard, IsPage(currentPage, "Dashboard.aspx"));
            ApplyActiveState(navLearn, IsPage(currentPage, "Learn.aspx") || IsPage(currentPage, "Chapter.aspx") || IsPage(currentPage, "Lesson.aspx"));
            ApplyActiveState(navPractice, IsPage(currentPage, "Practice.aspx") || IsPage(currentPage, "PracticeSets.aspx") || IsPage(currentPage, "PracticeAttempt.aspx"));
            ApplyActiveState(navGame, IsPage(currentPage, "Game.aspx") || IsPage(currentPage, "HiddenRoadSignGame.aspx") || IsPage(currentPage, "HiddenRoadSignResult.aspx") || IsPage(currentPage, "HiddenRoadSignReview.aspx"));
            ApplyActiveState(navMockExam, IsPage(currentPage, "MockExam.aspx") || IsPage(currentPage, "MockExamSets.aspx") || IsPage(currentPage, "MockExamAttempt.aspx"));
            ApplyActiveState(navProgress, IsPage(currentPage, "Progress.aspx"));
            ApplyActiveState(navProfile, IsPage(currentPage, "Profile.aspx"));
        }

        private static bool IsPage(string currentPage, string pageName)
        {
            return string.Equals(currentPage, pageName, StringComparison.OrdinalIgnoreCase);
        }

        private static void ApplyActiveState(HtmlAnchor link, bool isActive)
        {
            link.Attributes["class"] = isActive ? "app-nav-link is-active" : "app-nav-link";

            if (isActive)
            {
                link.Attributes["aria-current"] = "page";
            }
            else
            {
                link.Attributes.Remove("aria-current");
            }
        }
    }
}
