using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web;
using MyKPPLearn.Common;
using MyKPPLearn.Data;
using MyKPPLearn.Security;

namespace MyKPPLearn.Learner
{
    /// <summary>
    /// Learner progress, always for the signed-in learner (taken from Session, never from the request).
    ///   Learning: real LearningProgress against the currently active lessons and chapters.
    ///   Hidden Road Sign Game: real completed attempts (historical: kept even if the Set is later deactivated).
    ///   Practice and Mock Exam: every finished practice set and mock exam, newest first.
    /// </summary>
    public partial class Progress : System.Web.UI.Page
    {
        // The 3 most recent entries of each history are always shown; older ones stay hidden until "View All" (client-side, no reload).
        protected const int RecentGames = 3;

        protected int OverallCompleted;
        protected int OverallTotal;
        protected int OverallPercent;
        protected int GamesCompleted;
        protected int PracticeCount;
        protected int MockCount;

        protected void Page_Load(object sender, EventArgs e)
        {
            SessionHelper.RequireLearner(this);
            ((LearnerMaster)Master).PageHeading = "Progress";

            int userId = SessionHelper.CurrentUserId(Session).Value;

            try
            {
                BindLearning(userId);
                BindGames(userId);
                BindPractice(userId);
                BindMockExams(userId);
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
            pnlChapterToggle.Visible = chapters.Rows.Count > RecentGames;

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
                double ratioSum = 0;

                foreach (DataRow row in history.Rows)
                {
                    int totalItems = Convert.ToInt32(row["TotalItems"]);
                    ratioSum += totalItems <= 0 ? 0 : (double)Convert.ToInt32(row["CorrectCount"]) / totalItems;
                }

                int games = history.Rows.Count;
                int average = (int)Math.Round(ratioSum / games * 100, MidpointRounding.AwayFromZero);

                litGameSummary.Text = Stat(games.ToString(CultureInfo.InvariantCulture), games == 1 ? "Game Played" : "Games Played")                    + Stat(average.ToString(CultureInfo.InvariantCulture) + "%", "Average");
                pnlGameSummary.Visible = true;
            }

            pnlNoGames.Visible = GamesCompleted == 0;
            pnlGameToggle.Visible = GamesCompleted > RecentGames;
        }

        private void BindPractice(int userId)
        {
            DataTable history = LearnerRecordRepository.GetPracticeHistory(userId);
            rptPractice.DataSource = history;
            rptPractice.DataBind();

            int count = history.Rows.Count;
            PracticeCount = count;

            if (count > 0)
            {
                double ratioSum = 0;

                foreach (DataRow row in history.Rows)
                {
                    int total = Convert.ToInt32(row["TotalQuestions"]);
                    ratioSum += total <= 0 ? 0 : (double)Convert.ToInt32(row["CorrectCount"]) / total;
                }

                int average = (int)Math.Round(ratioSum / count * 100, MidpointRounding.AwayFromZero);
                litPracticeSummary.Text = Stat(count.ToString(CultureInfo.InvariantCulture), count == 1 ? "Set Completed" : "Sets Completed")
                    + Stat(average.ToString(CultureInfo.InvariantCulture) + "%", "Average");
                pnlPracticeSummary.Visible = true;
            }

            pnlNoPractice.Visible = count == 0;
            pnlPracticeToggle.Visible = count > RecentGames;
        }

        private void BindMockExams(int userId)
        {
            DataTable history = LearnerRecordRepository.GetMockExamHistory(userId);
            rptMock.DataSource = history;
            rptMock.DataBind();

            int count = history.Rows.Count;
            MockCount = count;

            if (count > 0)
            {
                int passed = history.Rows.Cast<DataRow>().Count(r => Convert.ToBoolean(r["IsPassed"]));
                litMockSummary.Text = Stat(count.ToString(CultureInfo.InvariantCulture), count == 1 ? "Exam Taken" : "Exams Taken")
                    + Stat(passed.ToString(CultureInfo.InvariantCulture), "Passed");
                pnlMockSummary.Visible = true;
            }

            pnlNoMock.Visible = count == 0;
            pnlMockToggle.Visible = count > RecentGames;
        }

        /// <summary>"KPP Standard — Mock Exam Set 1", HTML-encoded.</summary>
        protected string MockTitle(object typeCode, object setNumber)
        {
            MockExamType type = MockExamRules.Find(Convert.ToString(typeCode));
            string name = type == null ? Convert.ToString(typeCode) : type.Name;
            return HttpUtility.HtmlEncode(name + " \u2014 Mock Exam Set " + Convert.ToInt32(setNumber).ToString(CultureInfo.InvariantCulture));
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

        /// <summary>One summary figure above a history list: a big value with a small label under it.</summary>
        private static string Stat(string value, string label)
        {
            return "<div class=\"history-stat\"><span class=\"history-stat__value\">" + HttpUtility.HtmlEncode(value)
                + "</span><span class=\"history-stat__label\">" + HttpUtility.HtmlEncode(label) + "</span></div>";
        }

        /// <summary>The round percentage badge at the start of a history row: green from 80%, amber from 50%, red below.</summary>
        protected string ScoreBadge(object correct, object total)
        {
            int count = Convert.ToInt32(total);
            int percent = count <= 0 ? 0 : (int)Math.Round(Convert.ToInt32(correct) * 100.0 / count, MidpointRounding.AwayFromZero);
            string tier = percent >= 80 ? "good" : percent >= 50 ? "fair" : "low";
            return "<span class=\"history-score history-score--" + tier + "\" aria-label=\"Score " + percent.ToString(CultureInfo.InvariantCulture)
                + " percent\">" + percent.ToString(CultureInfo.InvariantCulture) + "%</span>";
        }

        protected string FormatDate(object completedAtUtc)
        {
            return DateTime.SpecifyKind(Convert.ToDateTime(completedAtUtc), DateTimeKind.Utc)
                .ToLocalTime().ToString("d MMMM yyyy", CultureInfo.InvariantCulture);
        }
    }
}
