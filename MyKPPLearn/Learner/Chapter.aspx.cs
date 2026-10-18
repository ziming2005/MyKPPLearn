using System;
using System.Data;
using System.Globalization;
using System.Web;
using MyKPPLearn.Data;
using MyKPPLearn.Security;

namespace MyKPPLearn.Learner
{
    public partial class Chapter : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SessionHelper.RequireLearner(this);
            ((LearnerMaster)Master).PageHeading = "Learn";

            int userId = SessionHelper.CurrentUserId(Session).Value;
            int chapterId;

            if (!int.TryParse(Request.QueryString["id"], NumberStyles.None, CultureInfo.InvariantCulture, out chapterId))
            {
                RedirectToLearn();
                return;
            }

            DataRow chapter;
            DataTable lessons;

            try
            {
                chapter = LearningChapterRepository.GetChapterById(chapterId);

                // Missing or inactive chapters are never shown, even via direct URL.
                if (chapter == null || !(bool)chapter["IsActive"])
                {
                    RedirectToLearn();
                    return;
                }

                lessons = LessonRepository.GetActiveLessonsByChapter(chapterId, userId);
            }
            catch (Exception)
            {
                RedirectToLearn();
                return;
            }

            string chapterNumber = Convert.ToInt32(chapter["DisplayOrder"]).ToString(CultureInfo.InvariantCulture);
            litCrumbChapter.Text = chapterNumber;
            litChapterNumber.Text = chapterNumber;
            litTitle.Text = HttpUtility.HtmlEncode(Convert.ToString(chapter["Title"]));
            litDescription.Text = HttpUtility.HtmlEncode(Convert.ToString(chapter["Description"]));

            rptLessons.DataSource = lessons;
            rptLessons.DataBind();
            pnlEmpty.Visible = lessons.Rows.Count == 0;
        }

        private void RedirectToLearn()
        {
            Response.Redirect("~/Learner/Learn.aspx", true);
        }
    }
}
