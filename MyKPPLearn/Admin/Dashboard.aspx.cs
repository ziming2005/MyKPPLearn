using System;
using System.Data;
using System.Globalization;
using System.Web;
using MyKPPLearn.Data;
using MyKPPLearn.Security;

namespace MyKPPLearn.Admin
{
    public partial class Dashboard : System.Web.UI.Page
    {
        // Set when any figure could not be loaded, so the page can say so instead of only showing a dash.
        private bool _partial;

        protected void Page_Load(object sender, EventArgs e)
        {
            SessionHelper.RequireAdministrator(this);
            ((AdminMaster)Master).PageHeading = "Dashboard";

            litFullName.Text = HttpUtility.HtmlEncode(SessionHelper.CurrentFullName(Session));

            try
            {
                int learners = UserRepository.GetLearnerCount();
                litTotalLearners.Text = learners.ToString(CultureInfo.InvariantCulture);
                litLearnersMeta.Text = learners + (learners == 1 ? " learner" : " learners");
            }
            catch (Exception)
            {
                litTotalLearners.Text = "—";
                _partial = true;
            }

            try
            {
                // The learner-visible count (active lessons in active chapters), so it matches what learners see.
                int lessons = LessonRepository.GetTotalActiveLessonCount();
                litTotalLessons.Text = lessons.ToString(CultureInfo.InvariantCulture);

                int chapters = AdminDashboardRepository.GetActiveChapterCount();
                litLearningMeta.Text = lessons + (lessons == 1 ? " lesson" : " lessons") + " in " + chapters + (chapters == 1 ? " chapter" : " chapters");
            }
            catch (Exception)
            {
                litTotalLessons.Text = "—";
                _partial = true;
            }

            try
            {
                // Same idea as Active Lessons: the game sets a learner can actually play (Active, with a configured road sign).
                int playable = HiddenRoadSignPracticeRepository.GetPlayableForLearner().Rows.Count;
                int games = HiddenRoadSignPracticeRepository.GetTotalCount();
                int assets = AdminDashboardRepository.GetRoadSignAssetCount();

                litOverviewHiddenRoadSigns.Text = playable.ToString(CultureInfo.InvariantCulture);
                litGamesMeta.Text = games + (games == 1 ? " game set" : " game sets") + " · " + assets + (assets == 1 ? " road sign" : " road signs");
            }
            catch (Exception)
            {
                litOverviewHiddenRoadSigns.Text = "—";
                _partial = true;
            }

            try
            {
                litNewLearners.Text = AdminDashboardRepository.GetNewLearnerCount(7).ToString(CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                litNewLearners.Text = "—";
                _partial = true;
            }

            BindActivity();

            pnlPartial.Visible = _partial;
        }

        private void BindActivity()
        {
            try
            {
                DataTable rows = AdminDashboardRepository.GetRecentActivity(10);

                if (rows.Rows.Count == 0)
                {
                    pnlNoActivity.Visible = true;
                    return;
                }

                rptActivity.DataSource = rows;
                rptActivity.DataBind();
                pnlActivity.Visible = true;
            }
            catch (Exception)
            {
                pnlNoActivity.Visible = true;
                _partial = true;
            }
        }

        /// <summary>One feed line as safe HTML: learner name in bold, then what they did.</summary>
        protected string ActivityText(object kind, object who, object detail)
        {
            string name = "<strong>" + HttpUtility.HtmlEncode(Convert.ToString(who)) + "</strong>";
            string text = Convert.ToString(detail);

            switch (Convert.ToString(kind))
            {
                case "register":
                    return name + " registered a learner account";

                case "lesson":
                    return name + " completed <em>" + HttpUtility.HtmlEncode(text) + "</em>";

                default:
                    string[] parts = text.Split('|');

                    if (parts.Length == 3)
                    {
                        return name + " finished " + HttpUtility.HtmlEncode(HiddenRoadSignPracticeRepository.FormatSetName(parts[0]))
                            + " (" + HttpUtility.HtmlEncode(parts[1]) + " / " + HttpUtility.HtmlEncode(parts[2]) + ")";
                    }

                    return name + " finished a Road Sign Game";
            }
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
    }
}
