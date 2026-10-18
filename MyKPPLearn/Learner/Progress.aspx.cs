using System;
using System.Data;
using System.Globalization;
using MyKPPLearn.Common;
using MyKPPLearn.Data;
using MyKPPLearn.Security;

namespace MyKPPLearn.Learner
{
    /// <summary>
    /// Learner progress, always for the signed-in learner (taken from Session, never from the request).
    ///   Learning: real LearningProgress against the currently active lessons and chapters.
    ///   Hidden Road Sign Game: real completed attempts (historical: kept even if the Set is later deactivated).
    ///   Practice and Mock Exam: attempts are not stored yet, so they show a dash / an honest placeholder, never made-up data.
    /// </summary>
    public partial class Progress : System.Web.UI.Page
    {
        // The 3 most recent games are always shown; any older ones are hidden until "View All" is clicked (client-side, no reload).
        protected const int RecentGames = 3;

        protected int OverallCompleted;
        protected int OverallTotal;
        protected int OverallPercent;
        protected int GamesCompleted;

        protected void Page_Load(object sender, EventArgs e)
        {
            SessionHelper.RequireLearner(this);
            ((LearnerMaster)Master).PageHeading = "Progress";

            int userId = SessionHelper.CurrentUserId(Session).Value;

            try
            {
                BindLearning(userId);
                BindGames(userId);
            }
            catch (Exception)
            {
                lblError.Text = "Your progress could not be loaded. Please try again.";
                lblError.Visible = true;
            }
        }

        // One grouped query: every active chapter with its active lesson count and how many the learner completed.
        // Overall progress is the sum of those, so it always agrees with the chapter rows.
        private void BindLearning(int userId)
        {
            DataTable chapters = LearningChapterRepository.GetActiveChaptersForLearner(userId);

            foreach (DataRow row in chapters.Rows)
            {
                OverallTotal += Convert.ToInt32(row["LessonCount"]);
                OverallCompleted += Convert.ToInt32(row["CompletedCount"]);
            }

            OverallPercent = LearningDisplay.Percent(OverallCompleted, OverallTotal);

            rptChapters.DataSource = chapters;
            rptChapters.DataBind();
            pnlNoChapters.Visible = chapters.Rows.Count == 0;

            DataRow next = LessonRepository.GetFirstIncompleteLesson(userId);

            if (next != null)
            {
                lnkContinue.HRef = ResolveUrl("~/Learner/Lesson.aspx?id=" + Convert.ToInt32(next["LessonId"]).ToString(CultureInfo.InvariantCulture));
                lnkContinue.InnerText = OverallCompleted == 0 ? "Start Learning" : "Continue Learning";
                lnkContinue.Visible = true;
            }
        }

        private void BindGames(int userId)
        {
            GamesCompleted = HiddenRoadSignAttemptRepository.GetCompletedCount(userId);

            // All of the learner's games are in the page (just score and date, a few rows); the browser shows only the latest 3 until expanded.
            DataTable history = HiddenRoadSignAttemptRepository.GetHistoryForUser(userId, int.MaxValue);
            rptGames.DataSource = history;
            rptGames.DataBind();

            if (history.Rows.Count > 0)
            {
                int bestCorrect = 0;
                int bestTotal = 0;
                double bestRatio = -1;
                double ratioSum = 0;

                foreach (DataRow row in history.Rows)
                {
                    int correct = Convert.ToInt32(row["CorrectCount"]);
                    int totalItems = Convert.ToInt32(row["TotalItems"]);
                    double ratio = totalItems <= 0 ? 0 : (double)correct / totalItems;

                    ratioSum += ratio;

                    if (ratio > bestRatio)
                    {
                        bestRatio = ratio;
                        bestCorrect = correct;
                        bestTotal = totalItems;
                    }
                }

                int games = history.Rows.Count;
                int average = (int)Math.Round(ratioSum / games * 100, MidpointRounding.AwayFromZero);

                litGameSummary.Text = games.ToString(CultureInfo.InvariantCulture) + (games == 1 ? " game played" : " games played") +
                    " &middot; Best: " + bestCorrect.ToString(CultureInfo.InvariantCulture) + " / " + bestTotal.ToString(CultureInfo.InvariantCulture) +
                    " &middot; Average: " + average.ToString(CultureInfo.InvariantCulture) + "%";
                pnlGameSummary.Visible = true;
            }

            pnlNoGames.Visible = GamesCompleted == 0;
            pnlGameToggle.Visible = GamesCompleted > RecentGames;
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

        protected int ChapterPercent(object completed, object total)
        {
            return LearningDisplay.Percent(Convert.ToInt32(completed), Convert.ToInt32(total));
        }

        protected string FormatDate(object completedAtUtc)
        {
            return DateTime.SpecifyKind(Convert.ToDateTime(completedAtUtc), DateTimeKind.Utc)
                .ToLocalTime().ToString("d MMMM yyyy", CultureInfo.InvariantCulture);
        }
    }
}
