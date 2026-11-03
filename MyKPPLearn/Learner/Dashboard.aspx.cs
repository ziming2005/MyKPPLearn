using System;
using System.Data;
using System.Globalization;
using System.Linq;
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

            int userId = SessionHelper.CurrentUserId(Session).Value;
            BindLearning(userId);
            BindScores(userId);
            BindActivity(userId);
            BindGameSets();
        }

        /// <summary>
        /// Practice Average: the mean score of every finished practice set. Mock Exam Best Score: the best overall score
        /// (Colour Blind + Theory) of any mock exam. Both stay a dash until there is something to show.
        /// </summary>
        private void BindScores(int userId)
        {
            try
            {
                DataRow[] practices = LearnerRecordRepository.GetPracticeHistory(userId).Rows.Cast<DataRow>().ToArray();

                if (practices.Length > 0)
                {
                    litPracticeAverage.Text = Percent(practices.Average(r => Ratio(r["CorrectCount"], r["TotalQuestions"])));
                }

                DataRow[] mocks = LearnerRecordRepository.GetMockExamHistory(userId).Rows.Cast<DataRow>().ToArray();

                if (mocks.Length > 0)
                {
                    litMockBest.Text = Percent(mocks.Max(r => Ratio(
                        Convert.ToInt32(r["ColourBlindCorrect"]) + Convert.ToInt32(r["TheoryCorrect"]),
                        Convert.ToInt32(r["ColourBlindTotal"]) + Convert.ToInt32(r["TheoryTotal"]))));
                }
            }
            catch (Exception)
            {
                // The boxes keep their dash.
            }
        }

        private static double Ratio(object correct, object total)
        {
            int count = Convert.ToInt32(total);
            return count <= 0 ? 0 : (double)Convert.ToInt32(correct) / count;
        }

        private static string Percent(double ratio)
        {
            return ((int)Math.Round(ratio * 100, MidpointRounding.AwayFromZero)).ToString(CultureInfo.InvariantCulture) + "%";
        }

        private void BindActivity(int userId)
        {
            try
            {
                DataTable rows = ActivityRepository.GetRecent(6, userId);

                if (rows.Rows.Count > 0)
                {
                    rptActivity.DataSource = rows;
                    rptActivity.DataBind();
                    pnlActivity.Visible = true;
                    pnlNoActivity.Visible = false;
                }
            }
            catch (Exception)
            {
                // The empty state stays.
            }
        }

        /// <summary>One feed line as safe HTML: "You completed <em>Lesson title</em>".</summary>
        protected string ActivityText(object kind, object detail)
        {
            return "You " + (Convert.ToString(kind) == "register" ? "created your account" : ActivityRepository.Describe(kind, detail));
        }

        /// <summary>The exact date and time, shown when hovering over "3 hours ago".</summary>
        protected string ExactTime(object utcValue)
        {
            return DateTime.SpecifyKind(Convert.ToDateTime(utcValue), DateTimeKind.Utc)
                .ToLocalTime().ToString("d MMM yyyy, h:mm tt", CultureInfo.InvariantCulture);
        }

        protected string TimeAgo(object utcValue)
        {
            DateTime when = DateTime.SpecifyKind(Convert.ToDateTime(utcValue), DateTimeKind.Utc);
            TimeSpan span = DateTime.UtcNow - when;

            if (span.TotalMinutes < 1) { return "just now"; }
            if (span.TotalMinutes < 60) { return (int)span.TotalMinutes + (span.TotalMinutes < 2 ? " minute ago" : " minutes ago"); }
            if (span.TotalHours < 24) { return (int)span.TotalHours + (span.TotalHours < 2 ? " hour ago" : " hours ago"); }
            if (span.TotalDays < 7) { return (int)span.TotalDays + (span.TotalDays < 2 ? " day ago" : " days ago"); }

            return when.ToLocalTime().ToString("d MMM yyyy", CultureInfo.InvariantCulture);
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
