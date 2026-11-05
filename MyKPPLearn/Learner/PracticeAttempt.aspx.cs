using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.Script.Serialization;
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

        private bool _adminView;

        protected override void OnPreInit(EventArgs e)
        {
            _adminView = AttemptViewer.UseAdminLayout(this);
            base.OnPreInit(e);
        }

        public string CategoryId { get; private set; }

        /// <summary>For an administrator: the learner's record page, where "Back" leads. Empty for the learner.</summary>
        public string BackUrl { get; private set; }

        /// <summary>window.PracticeSavedAttempt: a finished attempt reopened from Progress ("?attempt="), or "null".</summary>
        public string SavedJson { get; private set; }
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
            AttemptViewer.SetUp(this, _adminView, "Practice");
            SavedJson = "null";
            BackUrl = string.Empty;

            if (Request.QueryString["attempt"] != null)
            {
                LoadSaved();
                return;
            }

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

        /// <summary>Reopens one of the learner's own saved attempts on its result screen; anything else goes back to Progress.</summary>
        private void LoadSaved()
        {
            int attemptId;
            DataTable rows = null;

            if (int.TryParse(Request.QueryString["attempt"], NumberStyles.None, CultureInfo.InvariantCulture, out attemptId))
            {
                rows = PracticeAttemptRepository.GetSavedForUser(attemptId, AttemptViewer.OwnerFilter(this, _adminView));
            }

            if (rows == null || rows.Rows.Count == 0)
            {
                Response.Redirect(_adminView ? "~/Admin/LearnerRecords.aspx" : "~/Learner/Progress.aspx", true);
                return;
            }

            if (_adminView) { BackUrl = AttemptViewer.LearnerRecordUrl(this, rows.Rows[0]["UserId"]); }

            CategoryId = Convert.ToString(rows.Rows[0]["CategoryCode"]);
            SetNumber = Convert.ToInt32(rows.Rows[0]["SetNumber"]);

            var questions = new List<object>();
            var answers = new List<int>();

            foreach (DataRow row in rows.Rows)
            {
                string image = row.IsNull("ImagePathSnapshot") ? null : Convert.ToString(row["ImagePathSnapshot"]);

                questions.Add(new
                {
                    id = Convert.ToInt32(row["QuestionId"]),
                    text = Convert.ToString(row["QuestionTextSnapshot"]),
                    options = new[] { Convert.ToString(row["OptionATextSnapshot"]), Convert.ToString(row["OptionBTextSnapshot"]), Convert.ToString(row["OptionCTextSnapshot"]) },
                    correctIndex = LetterIndex(row["CorrectOptionSnapshot"]),
                    explanation = Convert.ToString(row["ExplanationSnapshot"]),
                    image = string.IsNullOrEmpty(image) ? "" : ResolveUrl("~/" + image)
                });
                answers.Add(LetterIndex(row["SelectedOptionSnapshot"]));
            }

            // "<" is escaped so question text can never close the inline <script> block.
            SavedJson = new JavaScriptSerializer { MaxJsonLength = int.MaxValue }
                .Serialize(new { questions = questions, answers = answers })
                .Replace("<", "\\u003c");
        }

        /// <summary>"A"-"C" to 0-2; anything else (no answer) is -1.</summary>
        internal static int LetterIndex(object letter)
        {
            string text = letter == null || letter == DBNull.Value ? "" : Convert.ToString(letter).Trim();
            return text.Length == 1 && text[0] >= 'A' && text[0] <= 'C' ? text[0] - 'A' : -1;
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
