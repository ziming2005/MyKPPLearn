using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using MyKPPLearn.Security;

namespace MyKPPLearn.Learner
{
    /// <summary>
    /// Phase 6 UI prototype. The actual exam — timer, questions, answers, flags, scoring,
    /// result and review — is entirely frontend state driven by Scripts/mock-exam.js.
    /// Nothing here touches a database or Session; a page refresh resets the attempt.
    /// This code-behind only validates the "type" and "set" query strings.
    /// </summary>
    public partial class MockExamAttempt : System.Web.UI.Page
    {
        // Mirrors the exam type ids defined in Scripts/mock-exam.js, for
        // query-string validation only.
        private static readonly Dictionary<string, string> KnownTypes = new Dictionary<string, string>
        {
            { "combined", "KPP Combined (Car & Motorcycle)" },
            { "kpp-standard", "KPP Standard (Car or Motorcycle Only)" }
        };

        public string TypeId { get; private set; }

        public string SetNumber { get; private set; }

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

            if (string.IsNullOrEmpty(requestedType) || !KnownTypes.ContainsKey(requestedType))
            {
                Response.Redirect("~/Learner/MockExam.aspx", true);
                return;
            }

            // Only the shape of "set" is checked here (a positive integer). Whether that set
            // exists, belongs to the type and is Active lives in the frontend mock data, so
            // Scripts/mock-exam.js redirects to the Exam Sets page for anything else.
            int setNumber;

            if (!int.TryParse(Request.QueryString["set"], NumberStyles.None, CultureInfo.InvariantCulture, out setNumber) || setNumber < 1)
            {
                Response.Redirect("~/Learner/MockExamSets.aspx?type=" + Uri.EscapeDataString(requestedType), true);
                return;
            }

            TypeId = requestedType;
            SetNumber = setNumber.ToString(CultureInfo.InvariantCulture);
        }
    }
}
