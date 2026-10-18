using System;
using System.Globalization;
using System.IO;
using MyKPPLearn.Security;

namespace MyKPPLearn.Admin
{
    /// <summary>
    /// Phase 6 UI prototype. Every row, filter and action on this page is driven entirely
    /// by frontend-only mock data (Scripts/mock-exam-demo.js + Scripts/mock-exam-admin-demo.js).
    /// There is no Mock Exam database table or repository: this code-behind only performs
    /// the page's authorization check and sets the page heading.
    /// </summary>
    public partial class MockExam : System.Web.UI.Page
    {
        public string MockExamScriptVersion
        {
            get { return ScriptVersion("~/Scripts/mock-exam-demo.js"); }
        }

        public string MockExamAdminScriptVersion
        {
            get { return ScriptVersion("~/Scripts/mock-exam-admin-demo.js"); }
        }

        private string ScriptVersion(string virtualPath)
        {
            return File.GetLastWriteTimeUtc(Server.MapPath(virtualPath)).Ticks.ToString(CultureInfo.InvariantCulture);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            SessionHelper.RequireAdministrator(this);
            ((AdminMaster)Master).PageHeading = "Mock Exam";
        }
    }
}
