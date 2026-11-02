using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Services;
using MyKPPLearn.Data;
using MyKPPLearn.Security;

namespace MyKPPLearn.Admin
{
    /// <summary>
    /// Admin Mock Exam. The page markup is a shell; Scripts/mock-exam-admin.js calls the page methods below (JSON over
    /// POST) to list, add, edit, activate/deactivate and delete Mock Exam Sets and to choose each set's questions from the
    /// Question Bank. The exam rules (sections, counts, time, pass marks) are MockExamRules. Every method re-checks the
    /// Administrator role because page methods bypass Page_Load.
    /// </summary>
    public partial class MockExam : System.Web.UI.Page
    {
        public string MockExamScriptVersion
        {
            get { return ScriptVersion("~/Scripts/mock-exam.js"); }
        }

        public string MockExamAdminScriptVersion
        {
            get { return ScriptVersion("~/Scripts/mock-exam-admin.js"); }
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

        public sealed class SetDto
        {
            public int id;
            public string typeId;
            public int number;
            public string status;
            public int attemptCount;
            public List<int> selected;
        }

        public sealed class BankQuestionDto
        {
            public int id;
            public string sectionId;   // colourBlind, roadSigns, rulesOfTheRoad or kejaraSafety
            public string text;
            public bool hasImage;
            public string status;
        }

        public sealed class DataDto
        {
            public List<SetDto> sets;
            public List<BankQuestionDto> bank;
        }

        /// <summary>A problem for one field ("type" or "number"), a general error, or the saved set with an optional notice.</summary>
        public sealed class ResultDto
        {
            public string field;
            public string error;
            public string notice;
            public SetDto set;
        }

        [WebMethod(EnableSession = true)]
        public static DataDto LoadData()
        {
            SessionHelper.DemandAdministrator();

            ILookup<int, int> selections = MockExamSetRepository.GetAllSelections().Rows.Cast<DataRow>()
                .ToLookup(r => Convert.ToInt32(r["MockExamSetId"]), r => Convert.ToInt32(r["QuestionId"]));

            // A category's section id is the same in both exam types.
            Dictionary<string, string> sectionByCategory = MockExamRules.Types[0].Sections.ToDictionary(s => s.CategoryCode, s => s.Id);

            DataDto data = new DataDto { sets = new List<SetDto>(), bank = new List<BankQuestionDto>() };

            foreach (DataRow row in MockExamSetRepository.GetAll().Rows)
            {
                data.sets.Add(ToSet(row, selections[Convert.ToInt32(row["MockExamSetId"])].ToList()));
            }

            foreach (DataRow row in MockExamSetRepository.GetQuestionChoices().Rows)
            {
                string sectionId;

                if (!sectionByCategory.TryGetValue(Convert.ToString(row["CategoryCode"]), out sectionId))
                {
                    continue;
                }

                data.bank.Add(new BankQuestionDto
                {
                    id = Convert.ToInt32(row["QuestionId"]),
                    sectionId = sectionId,
                    text = Convert.ToString(row["QuestionText"]),
                    hasImage = !row.IsNull("ImagePath"),
                    status = Convert.ToBoolean(row["IsActive"]) ? "Active" : "Inactive"
                });
            }

            return data;
        }

        [WebMethod(EnableSession = true)]
        public static ResultDto Save(int id, string typeId, int number)
        {
            SessionHelper.DemandAdministrator();

            MockExamType type = MockExamRules.Find(typeId);

            if (type == null)
            {
                return new ResultDto { field = "type", error = "Please select an exam type." };
            }

            if (number < 1)
            {
                return new ResultDto { field = "number", error = "Mock Exam Set number must be a whole number greater than 0 (for example 1, 2, 3)." };
            }

            string notice = null;
            MockExamSetSaveResult result;

            if (id == 0)
            {
                result = MockExamSetRepository.Create(type.Code, number, out id);
                notice = "Mock exam set created as Inactive. Use Questions to select its questions, then activate it.";
            }
            else
            {
                bool deactivated;
                result = MockExamSetRepository.Update(id, type.Code, number, out deactivated);

                if (deactivated)
                {
                    notice = "Exam type changed. Questions that no longer fit were removed and the set is now Inactive. Use Questions to complete the selection.";
                }
            }

            switch (result)
            {
                case MockExamSetSaveResult.DuplicateNumber:
                    return new ResultDto { field = "number", error = type.Title + " already has Mock Exam Set " + number + ". Please choose a different number." };
                case MockExamSetSaveResult.TypeLocked:
                    return new ResultDto { field = "type", error = "The exam type can't be changed because learners have already taken this set." };
                case MockExamSetSaveResult.NotFound:
                    return new ResultDto { error = "Mock Exam Set not found." };
            }

            return new ResultDto { set = LoadSet(id), notice = notice };
        }

        [WebMethod(EnableSession = true)]
        public static ResultDto Toggle(int id, bool active)
        {
            SessionHelper.DemandAdministrator();

            if (!MockExamSetRepository.SetActive(id, active))
            {
                return new ResultDto
                {
                    error = active
                        ? "This set can't be activated until every section has the required number of active questions. Use Questions to finish the selection."
                        : "Mock Exam Set not found."
                };
            }

            return new ResultDto { set = LoadSet(id) };
        }

        [WebMethod(EnableSession = true)]
        public static string Delete(int id)
        {
            SessionHelper.DemandAdministrator();

            switch (MockExamSetRepository.Delete(id))
            {
                case MockExamSetDeleteResult.Deleted:
                    return "Deleted";
                case MockExamSetDeleteResult.HasAttempts:
                    return "This set is protected because learners have taken it. Deactivate it instead.";
                default:
                    return "Mock Exam Set not found.";
            }
        }

        [WebMethod(EnableSession = true)]
        public static ResultDto SaveQuestions(int id, List<int> questionIds)
        {
            SessionHelper.DemandAdministrator();

            string error = MockExamSetRepository.SaveQuestions(id, questionIds);
            return error != null ? new ResultDto { error = error } : new ResultDto { set = LoadSet(id) };
        }

        private static SetDto LoadSet(int id)
        {
            DataRow row = MockExamSetRepository.GetById(id);

            if (row == null)
            {
                return null;
            }

            List<int> selected = MockExamSetRepository.GetAllSelections().Rows.Cast<DataRow>()
                .Where(r => Convert.ToInt32(r["MockExamSetId"]) == id)
                .Select(r => Convert.ToInt32(r["QuestionId"]))
                .ToList();

            return ToSet(row, selected);
        }

        private static SetDto ToSet(DataRow row, List<int> selected)
        {
            return new SetDto
            {
                id = Convert.ToInt32(row["MockExamSetId"]),
                typeId = Convert.ToString(row["ExamTypeCode"]),
                number = Convert.ToInt32(row["SetNumber"]),
                status = Convert.ToBoolean(row["IsActive"]) ? "Active" : "Inactive",
                attemptCount = Convert.ToInt32(row["AttemptCount"]),
                selected = selected
            };
        }
    }
}
