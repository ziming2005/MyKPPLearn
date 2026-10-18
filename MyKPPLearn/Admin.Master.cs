using System;
using System.Web.UI.HtmlControls;
using MyKPPLearn.Common;
using MyKPPLearn.Security;

namespace MyKPPLearn
{
    public partial class AdminMaster : System.Web.UI.MasterPage
    {
        public string PageHeading { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            SessionHelper.RequireAdministrator(this.Page);

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

        private void SetActiveNav()
        {
            string currentPage = System.IO.Path.GetFileName(Request.Path);

            ApplyActiveState(navDashboard, IsPage(currentPage, "Dashboard.aspx"));
            ApplyActiveState(navLearnerRecords, IsPage(currentPage, "LearnerRecords.aspx") || IsPage(currentPage, "LearnerRecordDetails.aspx"));
            ApplyActiveState(navProfile, IsPage(currentPage, "Profile.aspx"));

            // Learning / Assessment / Game are plain labels: only the page link itself is highlighted.
            ApplySubActiveState(navChapters, IsPage(currentPage, "LearningChapters.aspx"));
            ApplySubActiveState(navLessons, IsPage(currentPage, "Lessons.aspx"));

            ApplySubActiveState(navPracticeSets, IsPage(currentPage, "PracticeSets.aspx"));
            ApplySubActiveState(navQuestionBank, IsPage(currentPage, "QuestionBank.aspx"));
            ApplySubActiveState(navMockExam, IsPage(currentPage, "MockExam.aspx"));

            ApplySubActiveState(navHiddenRoadSign, IsPage(currentPage, "HiddenRoadSigns.aspx"));
            ApplySubActiveState(navRoadSignAssets, IsPage(currentPage, "RoadSignAssets.aspx"));
        }

        private static bool IsPage(string currentPage, string pageName)
        {
            return string.Equals(currentPage, pageName, StringComparison.OrdinalIgnoreCase);
        }

        private static void ApplyActiveState(HtmlAnchor link, bool isActive)
        {
            link.Attributes["class"] = isActive ? "app-nav-link is-active" : "app-nav-link";
            SetAriaCurrent(link, isActive);
        }

        private static void ApplySubActiveState(HtmlAnchor link, bool isActive)
        {
            link.Attributes["class"] = isActive
                ? "app-nav-link app-nav-link--sub is-active"
                : "app-nav-link app-nav-link--sub";
            SetAriaCurrent(link, isActive);
        }

        private static void SetAriaCurrent(HtmlAnchor link, bool isActive)
        {
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
