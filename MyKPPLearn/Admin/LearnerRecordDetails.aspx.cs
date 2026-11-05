using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Services;
using MyKPPLearn.Data;
using MyKPPLearn.Security;

namespace MyKPPLearn.Admin
{
    /// <summary>
    /// Admin Learner Record Details (read-only). Scripts/learner-records-admin.js calls GetDetails with the id from the
    /// address and draws the learner's account information, learning progress and Practice, Mock Exam and
    /// Road Sign Game history. An unknown id (or an administrator's id) shows "Learner not found".
    /// </summary>
    public partial class LearnerRecordDetails : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SessionHelper.RequireAdministrator(this);
            ((AdminMaster)Master).PageHeading = "Learner Record Details";
        }

        public sealed class ChapterDto { public int number; public string title; public int total; public int completed; }

        public sealed class PracticeDto { public int attemptId; public string category; public int setNumber; public int total; public int correct; public string date; }

        public sealed class MockDto
        {
            public int attemptId;
            public string examType;
            public int setNumber;
            public int colourBlind;
            public int colourBlindTotal;
            public int theory;
            public int theoryTotal;
            public bool passed;
            public string date;
        }

        public sealed class GameDto { public int attemptId; public int setNumber; public int total; public int correct; public string date; }

        public sealed class DetailsDto
        {
            public int id;
            public string name;
            public string email;
            public string status;
            public string joined;
            public List<ChapterDto> chapters;
            public List<PracticeDto> practice;
            public List<MockDto> mock;
            public List<GameDto> games;
        }

        /// <summary>The learner's record, or null when there is no learner with this id.</summary>
        [WebMethod(EnableSession = true)]
        public static DetailsDto GetDetails(int id)
        {
            SessionHelper.DemandAdministrator();

            DataRow learner = LearnerRecordRepository.GetById(id);

            if (learner == null)
            {
                return null;
            }

            DetailsDto details = new DetailsDto
            {
                id = id,
                name = Convert.ToString(learner["FullName"]),
                email = Convert.ToString(learner["Email"]),
                status = Convert.ToString(learner["Status"]),
                joined = LearnerRecords.LocalDate(learner["CreatedAt"]),
                chapters = new List<ChapterDto>(),
                practice = new List<PracticeDto>(),
                mock = new List<MockDto>(),
                games = new List<GameDto>()
            };

            // Same figures as the learner's own Progress page: active chapters and their active lessons.
            foreach (DataRow row in LearningChapterRepository.GetActiveChaptersForLearner(id).Rows)
            {
                details.chapters.Add(new ChapterDto
                {
                    number = Convert.ToInt32(row["DisplayOrder"]),
                    title = Convert.ToString(row["Title"]),
                    total = Convert.ToInt32(row["LessonCount"]),
                    completed = Convert.ToInt32(row["CompletedCount"])
                });
            }

            foreach (DataRow row in LearnerRecordRepository.GetPracticeHistory(id).Rows)
            {
                details.practice.Add(new PracticeDto
                {
                    attemptId = Convert.ToInt32(row["PracticeAttemptId"]),
                    category = Convert.ToString(row["CategoryTitle"]),
                    setNumber = Convert.ToInt32(row["SetNumber"]),
                    total = Convert.ToInt32(row["TotalQuestions"]),
                    correct = Convert.ToInt32(row["CorrectCount"]),
                    date = LearnerRecords.LocalDate(row["CompletedAt"])
                });
            }

            foreach (DataRow row in LearnerRecordRepository.GetMockExamHistory(id).Rows)
            {
                details.mock.Add(new MockDto
                {
                    attemptId = Convert.ToInt32(row["MockExamAttemptId"]),
                    examType = ExamTypeTitle(Convert.ToString(row["ExamTypeCode"])),
                    setNumber = Convert.ToInt32(row["SetNumber"]),
                    colourBlind = Convert.ToInt32(row["ColourBlindCorrect"]),
                    colourBlindTotal = Convert.ToInt32(row["ColourBlindTotal"]),
                    theory = Convert.ToInt32(row["TheoryCorrect"]),
                    theoryTotal = Convert.ToInt32(row["TheoryTotal"]),
                    passed = Convert.ToBoolean(row["IsPassed"]),
                    date = LearnerRecords.LocalDate(row["CompletedAt"])
                });
            }

            foreach (DataRow row in HiddenRoadSignAttemptRepository.GetHistoryForUser(id, int.MaxValue).Rows)
            {
                details.games.Add(new GameDto
                {
                    attemptId = Convert.ToInt32(row["HiddenRoadSignAttemptId"]),
                    setNumber = Convert.ToInt32(row["SetNumber"]),
                    total = Convert.ToInt32(row["TotalItems"]),
                    correct = Convert.ToInt32(row["CorrectCount"]),
                    date = LearnerRecords.LocalDate(row["CompletedAt"])
                });
            }

            return details;
        }

        private static string ExamTypeTitle(string code)
        {
            return code == "combined" ? "KPP Combined" : code == "kpp-standard" ? "KPP Standard" : code;
        }
    }
}
