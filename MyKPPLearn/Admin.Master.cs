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
            litAvatarInitials.Text = AvatarInitials.From(fullName);

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

            bool isPracticeSets = IsPage(currentPage, "PracticeSets.aspx");
            bool isHiddenRoadSign = IsPage(currentPage, "HiddenRoadSigns.aspx");
            bool isRoadSignAssets = IsPage(currentPage, "RoadSignAssets.aspx");
            bool isPracticeManagementOverview = IsPage(currentPage, "PracticeManagement.aspx");
            bool isPracticeManagementChild = isPracticeSets || isHiddenRoadSign || isRoadSignAssets || isPracticeManagementOverview;

            ApplyActiveState(navDashboard, IsPage(currentPage, "Dashboard.aspx"));
            ApplyActiveState(navLearnerRecords, IsPage(currentPage, "LearnerRecords.aspx"));
            ApplyActiveState(navLearning, IsPage(currentPage, "Learning.aspx"));
            ApplyActiveState(navQuestionBank, IsPage(currentPage, "QuestionBank.aspx"));
            ApplyActiveState(navMockExam, IsPage(currentPage, "MockExam.aspx"));
            ApplyActiveState(navProfile, IsPage(currentPage, "Profile.aspx"));

            ApplySubActiveState(navPracticeSets, isPracticeSets);
            ApplySubActiveState(navHiddenRoadSign, isHiddenRoadSign);
            ApplySubActiveState(navRoadSignAssets, isRoadSignAssets);

            string toggleClass = isPracticeManagementChild
                ? "app-nav-link app-nav-toggle is-active"
                : "app-nav-link app-nav-toggle";

            practiceManagementToggle.Attributes["class"] = toggleClass;
            practiceManagementToggle.Attributes["aria-expanded"] = isPracticeManagementChild ? "true" : "false";

            practiceManagementSubmenu.Attributes["class"] = isPracticeManagementChild
                ? "app-submenu is-open"
                : "app-submenu";
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
