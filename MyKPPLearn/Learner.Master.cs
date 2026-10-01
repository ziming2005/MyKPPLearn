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

            ApplyActiveState(navDashboard, IsPage(currentPage, "Dashboard.aspx"));
            ApplyActiveState(navLearn, IsPage(currentPage, "Learn.aspx"));
            ApplyActiveState(navPractice, IsPage(currentPage, "Practice.aspx"));
            ApplyActiveState(navMockExam, IsPage(currentPage, "MockExam.aspx"));
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
