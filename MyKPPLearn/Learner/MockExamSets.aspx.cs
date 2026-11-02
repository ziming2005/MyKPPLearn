using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using MyKPPLearn.Data;
using MyKPPLearn.Security;

namespace MyKPPLearn.Learner
{
    /// <summary>
    /// Learner Mock Exam Sets for one exam type. Only sets a learner can start now are listed: Active, with a complete
    /// paper of active questions (MockExamSetRepository.GetStartableSetNumbers). The list is written into
    /// window.MockExamDatabaseData and drawn by Scripts/mock-exam.js. An unknown "type" goes back to the Mock Exam page.
    /// </summary>
    public partial class MockExamSets : System.Web.UI.Page
    {
        public string TypeId { get; private set; }

        /// <summary>window.MockExamDatabaseData for mock-exam.js.</summary>
        public string DataJson { get; private set; }

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

            MockExamType type = MockExamRules.Find(Request.QueryString["type"]);

            if (type == null)
            {
                Response.Redirect("~/Learner/MockExam.aspx", true);
                return;
            }

            TypeId = type.Code;
            DataJson = BuildDataJson(type.Code, MockExamSetRepository.GetStartableSetNumbers(type.Code));

            string description = type.Title.Substring(type.Name.Length).Trim().Trim('(', ')');
            litTypeName.Text = HttpUtility.HtmlEncode(type.Name);
            litHeading.Text = HttpUtility.HtmlEncode(type.Name + " — Mock Exam Sets");
            litSubtitle.Text = HttpUtility.HtmlEncode(description);
        }

        /// <summary>{ "sets": { "&lt;type&gt;": [ { setNumber, status: "Active" } ] } }, safe to place inside a script block.</summary>
        internal static string BuildDataJson(string typeCode, IEnumerable<int> setNumbers)
        {
            var sets = new Dictionary<string, object>
            {
                { typeCode, setNumbers.Select(n => new { setNumber = n, status = "Active" }).ToList() }
            };

            return new JavaScriptSerializer().Serialize(new { sets = sets }).Replace("<", "\\u003c");
        }
    }
}
