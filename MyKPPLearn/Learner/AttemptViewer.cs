using System;
using System.Globalization;
using System.IO;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using MyKPPLearn.Security;

namespace MyKPPLearn.Learner
{
    /// <summary>
    /// Lets an administrator open a learner's saved result (Practice, Mock Exam, Road Sign Game) on the same pages the
    /// learner uses, from Admin Learner Record Details. The admin sees the page in the Admin layout, read-only: no Try
    /// Again or Next Set, and "Back to Learner Record" instead of the learner's way back.
    /// </summary>
    internal static class AttemptViewer
    {
        /// <summary>Call from OnPreInit. True (and the Admin layout) when an administrator opens a saved attempt.</summary>
        public static bool UseAdminLayout(Page page)
        {
            if (page.Request.QueryString["attempt"] == null || !SessionHelper.IsInRole(page.Session, SessionHelper.RoleAdministrator))
            {
                return false;
            }

            page.MasterPageFile = "~/Admin.Master";
            return true;
        }

        /// <summary>Signs in check and page heading for either viewer; the admin layout also gets the learner page styles.</summary>
        public static void SetUp(Page page, bool admin, string heading)
        {
            if (!admin)
            {
                SessionHelper.RequireLearner(page);
                ((LearnerMaster)page.Master).PageHeading = heading;
                return;
            }

            SessionHelper.RequireAdministrator(page);
            ((MyKPPLearn.AdminMaster)page.Master).PageHeading = heading;

            string version = File.GetLastWriteTimeUtc(page.Server.MapPath("~/Content/css/learner.css")).Ticks.ToString(CultureInfo.InvariantCulture);
            HtmlLink css = new HtmlLink { Href = page.ResolveUrl("~/Content/css/learner.css") + "?v=" + version };
            css.Attributes["rel"] = "stylesheet";
            page.Header.Controls.Add(css);
        }

        /// <summary>Whose attempts may be opened: the signed-in learner's own, or (null) any learner's for an administrator.</summary>
        public static int? OwnerFilter(Page page, bool admin)
        {
            return admin ? (int?)null : SessionHelper.CurrentUserId(page.Session).Value;
        }

        public static string LearnerRecordUrl(Page page, object learnerId)
        {
            return page.ResolveUrl("~/Admin/LearnerRecordDetails.aspx?id=" + Convert.ToInt32(learnerId).ToString(CultureInfo.InvariantCulture));
        }
    }
}
