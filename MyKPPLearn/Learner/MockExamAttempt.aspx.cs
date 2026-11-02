using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;
using MyKPPLearn.Data;
using MyKPPLearn.Security;

namespace MyKPPLearn.Learner
{
    /// <summary>
    /// Learner Mock Exam attempt. Scripts/mock-exam.js runs the screens (instructions, exam, result, review); this page:
    ///   - lets only a set the learner may start through (Active and complete), otherwise back to the set list;
    ///   - Start: hands out the paper WITHOUT the correct answers and remembers, in Session, which set and questions
    ///     were started and when (the timer is the server's, not the browser's);
    ///   - Submit: marks the answers on the server, saves the attempt with a snapshot of every question, and only then
    ///     returns the result with the correct answers and explanations for the review.
    /// </summary>
    public partial class MockExamAttempt : System.Web.UI.Page
    {
        private const string SessionKey = "MockExamInProgress";

        // A late submit (slow network, laptop asleep) within this margin still counts as on time.
        private const int GraceSeconds = 30;

        [Serializable]
        private sealed class ExamInProgress
        {
            public int SetId;
            public string TypeCode;
            public List<int> QuestionIds;
            public DateTime StartedUtc;
        }

        public string TypeId { get; private set; }

        public string SetNumber { get; private set; }

        /// <summary>window.MockExamDatabaseData for mock-exam.js: the startable sets of this type (for "Next Set").</summary>
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

            string requestedType = Request.QueryString["type"];

            if (MockExamRules.Find(requestedType) == null)
            {
                Response.Redirect("~/Learner/MockExam.aspx", true);
                return;
            }

            int setNumber;
            List<int> startable = MockExamSetRepository.GetStartableSetNumbers(requestedType);

            if (!int.TryParse(Request.QueryString["set"], NumberStyles.None, CultureInfo.InvariantCulture, out setNumber)
                || !startable.Contains(setNumber))
            {
                Response.Redirect("~/Learner/MockExamSets.aspx?type=" + Uri.EscapeDataString(requestedType), true);
                return;
            }

            TypeId = requestedType;
            SetNumber = setNumber.ToString(CultureInfo.InvariantCulture);
            DataJson = MockExamSets.BuildDataJson(requestedType, startable);
        }

        public sealed class QuestionDto
        {
            public int id;
            public int number;
            public string section;
            public string sectionLabel;
            public string text;
            public string image;
            public string[] options;
        }

        public sealed class StartDto
        {
            public string error;
            public int seconds;
            public List<QuestionDto> questions;
        }

        public sealed class SectionScoreDto { public string id; public string label; public int score; public int total; }

        public sealed class ReviewDto { public int correctIndex; public string explanation; }

        public sealed class ResultDto
        {
            public string error;
            public int colourBlindScore;
            public int theoryScore;
            public bool colourBlindPass;
            public bool theoryPass;
            public bool overallPass;
            public int answered;
            public int unanswered;
            public int timeTakenSeconds;
            public bool autoSubmitted;
            public List<SectionScoreDto> sections;
            public List<ReviewDto> review;
        }

        /// <summary>Starts (or restarts) the exam for the signed-in learner and returns the paper without answers.</summary>
        [WebMethod(EnableSession = true)]
        public static StartDto Start(string type, int set)
        {
            DemandLearner();

            MockExamType examType = MockExamRules.Find(type);
            System.Data.DataRow setRow = examType == null ? null : MockExamSetRepository.GetByTypeAndNumber(examType.Code, set);

            if (setRow == null || !MockExamSetRepository.GetStartableSetNumbers(examType.Code).Contains(set))
            {
                return new StartDto { error = "This Mock Exam Set is not available any more. Please choose another set." };
            }

            int setId = Convert.ToInt32(setRow["MockExamSetId"]);
            List<MockExamPaperQuestion> paper = MockExamAttemptRepository.GetPaper(setId, examType);

            HttpContext.Current.Session[SessionKey] = new ExamInProgress
            {
                SetId = setId,
                TypeCode = examType.Code,
                QuestionIds = paper.Select(q => q.QuestionId).ToList(),
                StartedUtc = DateTime.UtcNow
            };

            return new StartDto
            {
                seconds = examType.Minutes * 60,
                questions = paper.Select((q, i) => new QuestionDto
                {
                    id = q.QuestionId,
                    number = i + 1,
                    section = q.Section.Id,
                    sectionLabel = q.Section.Label,
                    text = q.Text,
                    image = q.ImagePath,
                    options = q.Options
                }).ToList()
            };
        }

        /// <summary>
        /// Marks and saves the exam started with Start. answers: 0-2 per question in paper order, -1 when unanswered.
        /// auto: the browser's timer ran out. The time used is measured on the server.
        /// </summary>
        [WebMethod(EnableSession = true)]
        public static ResultDto Submit(List<int> answers, List<bool> flags, bool auto)
        {
            DemandLearner();

            HttpContext context = HttpContext.Current;
            ExamInProgress exam = context.Session[SessionKey] as ExamInProgress;

            if (exam == null)
            {
                return new ResultDto { error = "This exam is no longer in progress (it may have been submitted already or started in another tab). Please start it again." };
            }

            MockExamType type = MockExamRules.Find(exam.TypeCode);
            List<MockExamPaperQuestion> paper = MockExamAttemptRepository.GetPaper(exam.SetId, type);

            if (!paper.Select(q => q.QuestionId).SequenceEqual(exam.QuestionIds))
            {
                context.Session.Remove(SessionKey);
                return new ResultDto { error = "This Mock Exam Set was changed while you were taking it. Please start it again." };
            }

            answers = answers ?? new List<int>();

            if (answers.Count != paper.Count)
            {
                return new ResultDto { error = "The answers could not be read. Please submit again." };
            }

            List<int> cleanAnswers = answers.Select(a => a >= 0 && a <= 2 ? a : -1).ToList();

            DateTime completedUtc = DateTime.UtcNow;
            int limit = type.Minutes * 60;
            int elapsed = (int)Math.Max(0, (completedUtc - exam.StartedUtc).TotalSeconds);
            bool timedOut = auto || elapsed > limit + GraceSeconds;
            int timeTaken = Math.Min(elapsed, limit);

            MockExamMarking marking = MockExamAttemptRepository.Mark(type, paper, cleanAnswers);
            int userId = SessionHelper.CurrentUserId(context.Session).Value;

            MockExamAttemptRepository.Save(userId, exam.SetId, exam.StartedUtc, completedUtc, timeTaken, timedOut,
                paper, cleanAnswers, flags ?? new List<bool>(), marking);

            context.Session.Remove(SessionKey);

            int answered = cleanAnswers.Count(a => a >= 0);

            return new ResultDto
            {
                colourBlindScore = marking.ColourBlindCorrect,
                theoryScore = marking.TheoryCorrect,
                colourBlindPass = marking.ColourBlindPassed,
                theoryPass = marking.TheoryPassed,
                overallPass = marking.Passed,
                answered = answered,
                unanswered = paper.Count - answered,
                timeTakenSeconds = timeTaken,
                autoSubmitted = timedOut,
                sections = type.Sections.Select(s => new SectionScoreDto
                {
                    id = s.Id,
                    label = s.Label,
                    score = marking.CorrectBySection[s.Id],
                    total = s.Count
                }).ToList(),
                review = paper.Select(q => new ReviewDto { correctIndex = q.CorrectIndex, explanation = q.Explanation }).ToList()
            };
        }

        private static void DemandLearner()
        {
            if (!SessionHelper.IsInRole(HttpContext.Current.Session, SessionHelper.RoleLearner))
            {
                throw new HttpException(403, "Learner access is required.");
            }
        }
    }
}
