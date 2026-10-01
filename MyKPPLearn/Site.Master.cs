using System;

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
            SetSectionLink(NavPractice, homeBaseUrl, "practice");
            SetSectionLink(NavMockExam, homeBaseUrl, "mock-exam");
            SetSectionLink(NavProgress, homeBaseUrl, "progress");

            FooterNavHome.NavigateUrl = homeUrl;
            SetSectionLink(FooterNavLearn, homeBaseUrl, "learn");
            SetSectionLink(FooterNavPractice, homeBaseUrl, "practice");
            SetSectionLink(FooterNavMockExam, homeBaseUrl, "mock-exam");
            SetSectionLink(FooterNavProgress, homeBaseUrl, "progress");

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
