using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Web;
using MyKPPLearn.Security;

namespace MyKPPLearn.Learner
{
    /// <summary>
    /// Phase 5 UI prototype. The actual attempt — questions, options, progress, scoring,
    /// result and review — is entirely frontend state driven by the mock data in
    /// Scripts/practice.js. Nothing here touches a database, Session or
    /// LearningProgress; a page refresh resets the in-progress attempt, which is expected
    /// for this prototype. This code-behind only validates the "category"/"set" query
    /// string, the same validate-and-redirect pattern used by the Learning module.
    /// </summary>
    public partial class PracticeAttempt : System.Web.UI.Page
    {
        // Mirrors the four category ids defined in Scripts/practice.js, for
        // query-string validation only.
        private static readonly Dictionary<string, string> KnownCategories = new Dictionary<string, string>
        {
            { "colour-recognition", "Colour Recognition" },
            { "road-signs", "Road Signs" },
            { "rules-of-the-road", "Rules of the Road" },
            { "kejara-safety", "KEJARA & Safety" }
        };

        public string CategoryId { get; private set; }
        public int SetNumber { get; private set; }

        public string PracticeDemoScriptVersion
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
