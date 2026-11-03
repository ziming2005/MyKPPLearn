using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.Services;
using MyKPPLearn.Data;
using MyKPPLearn.Security;

namespace MyKPPLearn.Learner
{
    /// <summary>
    /// Learner practice attempt. The questions come from the database through window.PracticeDatabaseData
    /// (written by Learner.Master); answering, the result and the review run in Scripts/practice.js. When the learner
    /// submits, Save stores the attempt (marked again on the server) for Progress and the Learner Records.
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

        /// <summary>
        /// Saves the finished practice of the signed-in learner. answers: 0-2 per question in questionIds order, -1 when
        /// unanswered; seconds: time spent (only used for the start time). Returns false when nothing could be saved.
        /// </summary>
        [WebMethod(EnableSession = true)]
        public static bool Save(string category, int set, List<int> questionIds, List<int> answers, int seconds)
        {
            HttpContext context = HttpContext.Current;

            if (!SessionHelper.IsInRole(context.Session, SessionHelper.RoleLearner))
            {
                throw new HttpException(403, "Learner access is required.");
            }

            int userId = SessionHelper.CurrentUserId(context.Session).Value;
            DateTime startedUtc = DateTime.UtcNow.AddSeconds(-Math.Max(0, seconds));

            return PracticeAttemptRepository.Save(userId, category, set, startedUtc, questionIds, answers) > 0;
        }
    }
}
