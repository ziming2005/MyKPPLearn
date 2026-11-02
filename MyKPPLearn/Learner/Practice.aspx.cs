using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.Services;
using MyKPPLearn.Data;
using MyKPPLearn.Security;

namespace MyKPPLearn.Learner
{
    public partial class Practice : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SessionHelper.RequireLearner(this);
            ((LearnerMaster)Master).PageHeading = "Practice";
        }

        public sealed class LearnerSetDto { public int id; public string categoryCode; public int number; public int questionCount; }
        public sealed class LearnerCategoryDto { public string code; public string title; public string description; }
        public sealed class LearnerPracticeDataDto { public List<LearnerCategoryDto> categories; public List<LearnerSetDto> sets; }

        [WebMethod(EnableSession = true)]
        public static LearnerPracticeDataDto LoadActive()
        {
            if (!SessionHelper.IsAuthenticated(HttpContext.Current.Session) || !string.Equals(SessionHelper.CurrentRole(HttpContext.Current.Session), SessionHelper.RoleLearner, StringComparison.Ordinal)) throw new HttpException(403, "Learner access is required.");
            LearnerPracticeDataDto result = new LearnerPracticeDataDto { categories = new List<LearnerCategoryDto>(), sets = new List<LearnerSetDto>() };
            foreach (DataRow row in QuestionCategoryRepository.GetAll().Rows) result.categories.Add(new LearnerCategoryDto { code = Convert.ToString(row["Code"]), title = Convert.ToString(row["Title"]), description = Convert.ToString(row["Description"]) });
            foreach (DataRow row in PracticeSetRepository.GetAllForAdmin().Rows)
            {
                if ((bool)row["IsActive"] && Convert.ToInt32(row["ActiveQuestionCount"]) > 0) result.sets.Add(new LearnerSetDto { id = Convert.ToInt32(row["PracticeSetId"]), categoryCode = Convert.ToString(row["CategoryCode"]), number = Convert.ToInt32(row["SetNumber"]), questionCount = Convert.ToInt32(row["ActiveQuestionCount"]) });
            }
            return result;
        }
    }
}
