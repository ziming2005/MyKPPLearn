using System;
using MyKPPLearn.Security;

namespace MyKPPLearn
{
    public partial class SiteMaster : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var currentPage = System.IO.Path.GetFileName(Request.Path);
            var isHomePage = string.Equals(currentPage, "Default.aspx", StringComparison.OrdinalIgnoreCase);

            var homeUrl = ResolveUrl("~/Default.aspx");
            var homeBaseUrl = isHomePage ? string.Empty : homeUrl;

            NavHome.NavigateUrl = homeUrl;
            SetSectionLink(NavLearn, homeBaseUrl, "learn");
            SetSectionLink(NavGame, homeBaseUrl, "road-sign-game");
            SetSectionLink(NavMockExam, homeBaseUrl, "mock-exam");
            SetSectionLink(NavFaq, homeBaseUrl, "faq");

            FooterNavHome.NavigateUrl = homeUrl;
            SetSectionLink(FooterNavLearn, homeBaseUrl, "learn");
            SetSectionLink(FooterNavGame, homeBaseUrl, "road-sign-game");
            SetSectionLink(FooterNavMockExam, homeBaseUrl, "mock-exam");
            SetSectionLink(FooterNavFaq, homeBaseUrl, "faq");

            // On the sign-in pages the footer shrinks to the note.
            bool isLogin = string.Equals(currentPage, "Login.aspx", StringComparison.OrdinalIgnoreCase);
            bool isRegister = string.Equals(currentPage, "Register.aspx", StringComparison.OrdinalIgnoreCase);

            footerTop.Visible = !(isLogin || isRegister);

            string dashboard = SessionHelper.DashboardUrl(Session);

            if (dashboard != null)
            {
                phHeaderGuest.Visible = false;
                lnkHeaderDashboard.Visible = true;
                lnkHeaderDashboard.HRef = ResolveUrl(dashboard);
                phFooterGuest.Visible = false;
                liFooterDashboard.Visible = true;
                lnkFooterDashboard.HRef = ResolveUrl(dashboard);
            }

            if (isHomePage)
            {
                NavHome.Attributes["aria-current"] = "page";
            }
        }

        private static void SetSectionLink(System.Web.UI.WebControls.HyperLink link, string homeBaseUrl, string sectionId)
        {
            link.NavigateUrl = homeBaseUrl + "#" + sectionId;
        }
    }
}
