using System;
using System.Web.UI.HtmlControls;
using MyKPPLearn.Common;
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

        protected string PracticeDataJson() {
            string page = System.IO.Path.GetFileName(Request.Path);
            if (!IsPage(page, "Practice.aspx") && !IsPage(page, "PracticeSets.aspx") && !IsPage(page, "PracticeAttempt.aspx")) return "null";
            SessionHelper.RequireLearner(Page);
            var questions = new System.Collections.Generic.List<object>();
            var sets = new System.Collections.Generic.List<object>();
            foreach (System.Data.DataRow r in MyKPPLearn.Data.DatabaseHelper.ExecuteQuery(@"SELECT p.PracticeSetId,p.SetNumber,p.IsActive,c.Code AS CategoryCode,(SELECT COUNT(*) FROM Questions q WHERE q.PracticeSetId=p.PracticeSetId AND q.IsActive=1) AS ActiveQuestionCount FROM PracticeSets p JOIN QuestionCategories c ON c.QuestionCategoryId=p.QuestionCategoryId WHERE c.IsActive=1 ORDER BY c.DisplayOrder,p.SetNumber").Rows) {
                if (Convert.ToBoolean(r["IsActive"]) && Convert.ToInt32(r["ActiveQuestionCount"]) > 0)
                    sets.Add(new { id=Convert.ToInt32(r["PracticeSetId"]), categoryId=Convert.ToString(r["CategoryCode"]), number=Convert.ToInt32(r["SetNumber"]), questionCount=Convert.ToInt32(r["ActiveQuestionCount"]), status="Active" });
            }
            foreach (System.Data.DataRow q in MyKPPLearn.Data.DatabaseHelper.ExecuteQuery(@"SELECT q.*, c.Code FROM Questions q JOIN PracticeSets p ON p.PracticeSetId=q.PracticeSetId JOIN QuestionCategories c ON c.QuestionCategoryId=p.QuestionCategoryId WHERE q.IsActive=1 AND p.IsActive=1 AND c.IsActive=1 ORDER BY q.QuestionId").Rows) {
                var options = new System.Collections.Generic.List<string>();
                int correct = -1;
                foreach (System.Data.DataRow o in MyKPPLearn.Data.QuestionRepository.GetOptions(Convert.ToInt32(q["QuestionId"])).Rows) {
                    if (Convert.ToBoolean(o["IsCorrect"])) correct=options.Count;
                    options.Add(Convert.ToString(o["OptionText"]));
                }
                string image=Convert.ToString(q["ImagePath"]);
                questions.Add(new { id=Convert.ToInt32(q["QuestionId"]), setId=Convert.ToInt32(q["PracticeSetId"]), categoryId=Convert.ToString(q["Code"]), text=Convert.ToString(q["QuestionText"]), options=options, correctIndex=correct, explanation=Convert.ToString(q["Explanation"]), image=string.IsNullOrEmpty(image) ? "" : ResolveUrl(image), imageAlt=Convert.ToString(q["ImageAltText"]), status="Active" });
            }
            return new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = int.MaxValue }.Serialize(new { sets=sets, questions=questions }).Replace("<", "\\u003c");
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



