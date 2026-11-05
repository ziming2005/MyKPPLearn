using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
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
            public int learnerCount;
            public List<int> selected;
        }

        public sealed class BankQuestionDto
        {
            public int id;
            public string sectionId;   // colourBlind, roadSigns, rulesOfTheRoad or kejaraSafety
            public string text;
            public string image;       // site-absolute URL of the question's picture, or null
            public string status;
        }

        public sealed class DataDto
        {
            public List<SetDto> sets;
            public List<BankQuestionDto> bank;
        }

        /// <summary>A problem with the exam type (field "type"), a general error, or the saved set with its notice.</summary>
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
                    image = row.IsNull("ImagePath") ? null : VirtualPathUtility.ToAbsolute("~/" + Convert.ToString(row["ImagePath"]).TrimStart('~', '/')),
                    status = Convert.ToBoolean(row["IsActive"]) ? "Active" : "Inactive"
                });
            }

            return data;
        }

        /// <summary>
        /// Add or Edit Mock Exam Set: saves the exam type and the chosen questions together (id 0 adds a new set, which
        /// gets the next free number). The selection may be incomplete; such a set is saved as Inactive.
        /// </summary>
        [WebMethod(EnableSession = true)]
        public static ResultDto Save(int id, string typeId, List<int> questionIds)
        {
            SessionHelper.DemandAdministrator();

            bool isNew = id == 0;
            bool complete;
            bool deactivated;
            string error = MockExamSetRepository.Save(ref id, typeId, questionIds, out complete, out deactivated);

            if (error != null)
            {
                return new ResultDto { field = error.StartsWith("Please select an exam type") || error.Contains("exam type can't") ? "type" : null, error = error };
            }

            SetDto set = LoadSet(id);
            string name = "Mock Exam Set " + set.number;
            string notice;

            if (isNew)
            {
                notice = complete
                    ? name + " created and Active."
                    : name + " saved as Inactive because its selection is incomplete. Edit it to finish choosing questions, then activate it.";
            }
            else if (deactivated)
            {
                notice = name + " saved. It is now Inactive because its selection is incomplete.";
            }
            else if (complete && set.status != "Active")
            {
                notice = name + " saved. Its selection is complete, so you can activate it from the list.";
            }
            else
            {
                notice = name + " saved.";
            }

            return new ResultDto { set = set, notice = notice };
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
                        ? "This set can't be activated until every section has the required number of active questions. Edit the set to finish the selection."
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
                case MockExamSetDeleteResult.HasResults:
                    return "Learners have taken this Mock Exam Set, so it can't be deleted. Deactivate it instead to hide it from learners.";
                default:
                    return "Mock Exam Set not found.";
            }
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
                learnerCount = Convert.ToInt32(row["LearnerCount"]),
                selected = selected
            };
        }
    }
}
