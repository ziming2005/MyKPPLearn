using System;
using System.Data;
using System.Globalization;
using MyKPPLearn.Common;
using MyKPPLearn.Data;
using MyKPPLearn.Security;

namespace MyKPPLearn.Learner
{
    public partial class Learn : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SessionHelper.RequireLearner(this);
            ((LearnerMaster)Master).PageHeading = "Learning";

            int userId = SessionHelper.CurrentUserId(Session).Value;

            try
            {
                DataTable chapters = LearningChapterRepository.GetActiveChaptersForLearner(userId);
                rptChapters.DataSource = chapters;
                rptChapters.DataBind();
                pnlEmpty.Visible = chapters.Rows.Count == 0;
                BindSummary(userId, chapters);
            }
            catch (Exception)
            {
                pnlEmpty.Visible = false;
                lblError.Text = "Learning content could not be loaded. Please try again.";
                lblError.Visible = true;
            }
        }

        private void BindSummary(int userId, DataTable chapters)
        {
            int total = 0;
            int completed = 0;

            foreach (DataRow row in chapters.Rows)
            {
                total += Convert.ToInt32(row["LessonCount"]);
                completed += Convert.ToInt32(row["CompletedCount"]);
            }

            if (total == 0)
            {
                return;
            }

            int percent = LearningDisplay.Percent(completed, total);

            litSummaryCompleted.Text = completed.ToString(CultureInfo.InvariantCulture);
            litSummaryTotal.Text = total.ToString(CultureInfo.InvariantCulture);
            litSummaryPercent.Text = percent.ToString(CultureInfo.InvariantCulture);
            summaryFill.Style["width"] = percent.ToString(CultureInfo.InvariantCulture) + "%";
            pnlSummary.Visible = true;

            DataRow next = LessonRepository.GetFirstIncompleteLesson(userId);

            if (next != null)
            {
                lnkContinue.HRef = ResolveUrl("~/Learner/Lesson.aspx?id=" + Convert.ToInt32(next["LessonId"]).ToString(CultureInfo.InvariantCulture));
                lnkContinue.Visible = true;
            }
            else
            {
                pnlAllDone.Visible = true;
            }
        }

        /// <summary>"empty" (no lessons), "new", "progress" or "done".</summary>
        protected string ChapterState(object completed, object total)
        {
            int done = Convert.ToInt32(completed);
            int all = Convert.ToInt32(total);

            if (all <= 0) { return "empty"; }
            if (done >= all) { return "done"; }
            return done > 0 ? "progress" : "new";
        }

        protected string ChapterButtonText(object completed, object total)
        {
            switch (ChapterState(completed, total))
            {
                case "done": return "Review Chapter";
                case "progress": return "Continue";
                default: return "Start Chapter";
            }
        }

        protected string ChapterProgressText(object completed, object total)
        {
            int done = Convert.ToInt32(completed);
            int all = Convert.ToInt32(total);

            if (all <= 0) { return "No lessons yet"; }

            return done + " of " + all + (all == 1 ? " lesson" : " lessons") + " \u00B7 " + LearningDisplay.Percent(done, all) + "%";
        }

        protected int ChapterPercent(object completed, object total)
        {
            return LearningDisplay.Percent(Convert.ToInt32(completed), Convert.ToInt32(total));
        }
    }
}
