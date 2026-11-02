using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Web;
using MyKPPLearn.Security;

namespace MyKPPLearn.Learner
{
    /// <summary>
    /// Learner practice attempt. The questions come from the database through window.PracticeDatabaseData
    /// (written by Learner.Master); answering, scoring, the result and the review run in Scripts/practice.js.
    /// Attempts are not saved yet: PracticeAttemptRepository is still to be implemented, so a page refresh
    /// starts the set again. This code-behind only validates the "category"/"set" query string.
    /// </summary>
    public partial class PracticeAttempt : System.Web.UI.Page
    {
        // The four fixed QuestionCategories codes, for query-string validation only.
        private static readonly Dictionary<string, string> KnownCategories = new Dictionary<string, string>
        {
            { "colour-recognition", "Colour Recognition" },
            { "road-signs", "Road Signs" },
            { "rules-of-the-road", "Rules of the Road" },
            { "kejara-safety", "KEJARA & Safety" }
        };

        public string CategoryId { get; private set; }
        public int SetNumber { get; private set; }

        public string PracticeScriptVersion
        {
            get
            {
                string scriptPath = Server.MapPath("~/Scripts/practice.js");
                return File.GetLastWriteTimeUtc(scriptPath).Ticks.ToString(CultureInfo.InvariantCulture);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            SessionHelper.RequireLearner(this);
            ((LearnerMaster)Master).PageHeading = "Practice";

            string requestedCategory = Request.QueryString["category"];
            string categoryTitle;
            int setNumber;

            bool categoryValid = !string.IsNullOrEmpty(requestedCategory) && KnownCategories.TryGetValue(requestedCategory, out categoryTitle);

            if (!categoryValid)
            {
                Response.Redirect("~/Learner/Practice.aspx", true);
                return;
            }

            if (!int.TryParse(Request.QueryString["set"], NumberStyles.None, CultureInfo.InvariantCulture, out setNumber) || setNumber < 1)
            {
                Response.Redirect("~/Learner/PracticeSets.aspx?category=" + requestedCategory, true);
                return;
            }

            CategoryId = requestedCategory;
            SetNumber = setNumber;
        }
    }
}
