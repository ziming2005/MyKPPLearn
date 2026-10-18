using System;
using MyKPPLearn.Security;

namespace MyKPPLearn
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string dashboard = SessionHelper.DashboardUrl(Session);

            if (dashboard != null)
            {
                string url = ResolveUrl(dashboard);

                phHeroGuest.Visible = false;
                lnkHeroDashboard.Visible = true;
                lnkHeroDashboard.HRef = url;
                phCtaGuest.Visible = false;
                lnkCtaDashboard.Visible = true;
                lnkCtaDashboard.HRef = url;
                ctaHeading.InnerText = "Welcome Back";
                ctaText.InnerText = "Head to your dashboard to pick up where you left off.";

                if (dashboard.StartsWith("~/Learner", StringComparison.OrdinalIgnoreCase))
                {
                    lnkMockExam.HRef = ResolveUrl("~/Learner/MockExam.aspx");
                    lnkMockExam.InnerText = "Go to Mock Exam";
                    lnkProgress.HRef = ResolveUrl("~/Learner/Progress.aspx");
                    lnkProgress.InnerText = "Go to Progress";
                }
            }
        }
    }
}
