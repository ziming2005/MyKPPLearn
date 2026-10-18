using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Web;
using MyKPPLearn.Security;

namespace MyKPPLearn.Learner
{
    /// <summary>
    /// Phase 6 UI prototype. The Mock Exam Sets listed here come from the frontend-only mock
    /// data in Scripts/mock-exam.js (only Active sets are rendered). Nothing here touches
    /// a database. This code-behind only validates the "type" query string and fills the
    /// heading, the same validate-and-redirect pattern used by PracticeSets.aspx.
    /// </summary>
    public partial class MockExamSets : System.Web.UI.Page
    {
        private sealed class ExamTypeInfo
        {
            public string Name;
            public string Description;
        }

        // Mirrors the exam type ids defined in Scripts/mock-exam.js, for
        // query-string validation and headings only.
        private static readonly Dictionary<string, ExamTypeInfo> KnownTypes = new Dictionary<string, ExamTypeInfo>
        {
            { "combined", new ExamTypeInfo { Name = "KPP Combined", Description = "Car & Motorcycle" } },
            { "kpp-standard", new ExamTypeInfo { Name = "KPP Standard", Description = "Car or Motorcycle Only" } }
        };

        public string TypeId { get; private set; }

        public string MockExamScriptVersion
        {
            get
            {
                string scriptPath = Server.MapPath("~/Scripts/mock-exam.js");
                return File.GetLastWriteTimeUtc(scriptPath).Ticks.ToString(CultureInfo.InvariantCulture);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            SessionHelper.RequireLearner(this);
            ((LearnerMaster)Master).PageHeading = "Mock Exam";

            string requestedType = Request.QueryString["type"];
            ExamTypeInfo info;

            if (string.IsNullOrEmpty(requestedType) || !KnownTypes.TryGetValue(requestedType, out info))
            {
                Response.Redirect("~/Learner/MockExam.aspx", true);
                return;
            }

            TypeId = requestedType;
            litTypeName.Text = HttpUtility.HtmlEncode(info.Name);
            litHeading.Text = HttpUtility.HtmlEncode(info.Name + " \u2014 Mock Exam Sets");
            litSubtitle.Text = HttpUtility.HtmlEncode(info.Description);
        }
    }
}
