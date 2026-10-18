using System;
using System.Data;
using System.Globalization;
using System.Web;
using MyKPPLearn.Data;
using MyKPPLearn.Security;

namespace MyKPPLearn.Learner
{
    public partial class Dashboard : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SessionHelper.RequireLearner(this);
            ((LearnerMaster)Master).PageHeading = "Dashboard";

            litFullName.Text = System.Web.HttpUtility.HtmlEncode(SessionHelper.CurrentFullName(Session));

            if (Session["RegistrationSuccess"] != null)
            {
                pnlSuccess.Visible = true;
                Session.Remove("RegistrationSuccess");
            }

            BindLearning(SessionHelper.CurrentUserId(Session).Value);
            BindGameSets();
        }

        private void BindGameSets()
        {
            try
            {
                int count = HiddenRoadSignPracticeRepository.GetPlayableForLearner().Rows.Count;

                if (count > 0)
                {
                    litGameCount.Text = count.ToString(CultureInfo.InvariantCulture) + (count == 1 ? " game set" : " game sets");
                    pnlGameCount.Visible = true;
                }
            }
            catch (Exception)
            {
                // The count is a nicety; the banner works without it.
            }
        }

        private void BindLearning(int userId)
        {
            try
            {
                int completed = LearningProgressRepository.GetCompletedLessonCount(userId);
                int percent = LearningProgressRepository.GetLearningProgressPercentage(userId);
                string percentText = percent.ToString(CultureInfo.InvariantCulture) + "%";

                litLessonsCompleted.Text = completed.ToString(CultureInfo.InvariantCulture);
                litLearningProgress.Text = percentText;

                DataRow next = LessonRepository.GetFirstIncompleteLesson(userId);

                if (next != null)
                {
                    litContinueNumber.Text = LessonRepository.FormatLessonNumber(next["ChapterOrder"], next["DisplayOrder"]);
                    litContinueTitle.Text = HttpUtility.HtmlEncode(Convert.ToString(next["Title"]));
                    lnkContinue.HRef = ResolveUrl("~/Learner/Lesson.aspx?id=" + Convert.ToInt32(next["LessonId"]).ToString(CultureInfo.InvariantCulture));
                    pnlContinueLesson.Visible = true;
                }
                else if (LessonRepository.GetTotalActiveLessonCount() > 0)
                {
                    pnlContinueAllDone.Visible = true;
                }
                else
                {
                    pnlContinueNone.Visible = true;
                }
            }
            catch (Exception)
            {
                litLessonsCompleted.Text = "\u2014";
                litLearningProgress.Text = "\u2014";
                pnlContinueNone.Visible = true;
            }
        }
    }
}
