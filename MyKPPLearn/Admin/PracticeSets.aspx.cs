using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.Services;
using MyKPPLearn.Data;
using MyKPPLearn.Security;

namespace MyKPPLearn.Admin
{
    public partial class PracticeSets : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SessionHelper.RequireAdministrator(this);
            ((AdminMaster)Master).PageHeading = "Practice Sets";
        }

        public sealed class CategoryDto { public int id; public string code; public string title; }
        public sealed class SetDto { public int id; public int categoryId; public int number; public string status; public int questionCount; public int activeQuestionCount; public int attemptCount; public string error; }
        public sealed class PracticeDataDto { public List<CategoryDto> categories; public List<SetDto> sets; }

        [WebMethod(EnableSession = true)] public static PracticeDataDto LoadData()
        {
            RequireAdmin(); PracticeDataDto output=new PracticeDataDto { categories=new List<CategoryDto>(), sets=new List<SetDto>() };
            foreach(DataRow r in QuestionCategoryRepository.GetAll().Rows) output.categories.Add(new CategoryDto { id=Convert.ToInt32(r["QuestionCategoryId"]),code=Convert.ToString(r["Code"]),title=Convert.ToString(r["Title"]) });
            foreach(DataRow r in PracticeSetRepository.GetAllForAdmin().Rows) output.sets.Add(ToSet(r)); return output;
        }
        [WebMethod(EnableSession = true)] public static SetDto Save(int id,int categoryId,int number)
        {
            RequireAdmin(); if(number<1)throw new ArgumentException("Practice Set number must be greater than zero."); if(!QuestionCategoryRepository.Exists(categoryId))throw new ArgumentException("Select a valid category.");
            try { if(id==0)id=PracticeSetRepository.Create(categoryId,number); else if(!PracticeSetRepository.Update(id,categoryId,number))throw new ArgumentException("Practice Set not found."); }
            catch(System.Data.SqlClient.SqlException ex) when(ex.Number==2601||ex.Number==2627){throw new ArgumentException("That category already has this Practice Set number.");}
            DataRow row=PracticeSetRepository.GetById(id); return ToSet(row);
        }
        [WebMethod(EnableSession = true)] public static SetDto Toggle(int id,bool active)
        { RequireAdmin(); if(!PracticeSetRepository.SetActive(id,active))return new SetDto { error=active?"Add and save at least one active question before activating this Practice Set.":"Practice Set not found." }; DataRow row=PracticeSetRepository.GetById(id);return ToSet(row); }
        [WebMethod(EnableSession = true)] public static string Delete(int id)
        { RequireAdmin(); PracticeSetDeleteResult result=PracticeSetRepository.Delete(id); if(result==PracticeSetDeleteResult.Deleted)return "Deleted"; if(result==PracticeSetDeleteResult.HasQuestions)return "Remove or reassign this set's questions first."; if(result==PracticeSetDeleteResult.HasAttempts)return "This Practice Set is protected because it has learner attempts."; return "Practice Set not found."; }
        private static SetDto ToSet(DataRow r) { return new SetDto { id=Convert.ToInt32(r["PracticeSetId"]),categoryId=Convert.ToInt32(r["QuestionCategoryId"]),number=Convert.ToInt32(r["SetNumber"]),status=(bool)r["IsActive"]?"Active":"Inactive",questionCount=Convert.ToInt32(r["QuestionCount"]),activeQuestionCount=Convert.ToInt32(r["ActiveQuestionCount"]),attemptCount=Convert.ToInt32(r["AttemptCount"]) }; }
        private static void RequireAdmin() { if(!SessionHelper.IsAuthenticated(HttpContext.Current.Session)||!string.Equals(SessionHelper.CurrentRole(HttpContext.Current.Session),SessionHelper.RoleAdministrator,StringComparison.Ordinal))throw new HttpException(403,"Administrator access is required."); }
    }
}
