using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.Services;
using MyKPPLearn.Common;
using MyKPPLearn.Data;
using MyKPPLearn.Security;

namespace MyKPPLearn.Admin
{
    public partial class QuestionBank : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SessionHelper.RequireAdministrator(this);
            ((AdminMaster)Master).PageHeading = "Question Bank";
        }

        public sealed class QuestionDto { public int id; public int categoryId; public int? setId; public string text; public string explanation; public string image; public string imageAlt; public string status; public string[] options; public int correctIndex; public int attemptCount; public int mockExamUseCount; public string error; }
        public sealed class QuestionDataDto { public List<PracticeSets.CategoryDto> categories; public List<PracticeSets.SetDto> sets; public List<QuestionDto> questions; }
        [WebMethod(EnableSession = true)] public static QuestionDataDto LoadData()
        { RequireAdmin(); QuestionDataDto output=new QuestionDataDto { categories=new List<PracticeSets.CategoryDto>(),sets=new List<PracticeSets.SetDto>(),questions=new List<QuestionDto>()}; foreach(DataRow r in QuestionCategoryRepository.GetAll().Rows)output.categories.Add(new PracticeSets.CategoryDto{id=Convert.ToInt32(r["QuestionCategoryId"]),code=Convert.ToString(r["Code"]),title=Convert.ToString(r["Title"])}); foreach(DataRow r in PracticeSetRepository.GetAllForAdmin().Rows)output.sets.Add(new PracticeSets.SetDto{id=Convert.ToInt32(r["PracticeSetId"]),categoryId=Convert.ToInt32(r["QuestionCategoryId"]),number=Convert.ToInt32(r["SetNumber"]),status=(bool)r["IsActive"]?"Active":"Inactive",questionCount=Convert.ToInt32(r["QuestionCount"]),activeQuestionCount=Convert.ToInt32(r["ActiveQuestionCount"]),attemptCount=Convert.ToInt32(r["AttemptCount"])}); foreach(DataRow r in QuestionRepository.GetAllForAdmin().Rows)output.questions.Add(ToQuestion(r)); return output; }
        [WebMethod(EnableSession = true)] public static QuestionDto Get(int id) { RequireAdmin(); DataRow row=QuestionRepository.GetById(id);if(row==null)throw new ArgumentException("Question not found.");return ToQuestion(row); }
        [WebMethod(EnableSession = true)] public static QuestionDto Save(QuestionDto question)
        { RequireAdmin(); string newPath=null; string oldPath=null; try { if(question==null)throw new ArgumentException("Question data is required."); DataRow previous=question.id==0?null:QuestionRepository.GetById(question.id); oldPath=previous==null?null:(previous.IsNull("ImagePath")?null:Convert.ToString(previous["ImagePath"])); newPath=question.image; if(!string.IsNullOrEmpty(newPath)&&newPath.StartsWith("data:image/",StringComparison.Ordinal))newPath=QuestionImageStorage.SaveDataUrl(newPath); QuestionInput input=new QuestionInput{QuestionId=question.id,CategoryId=question.categoryId,PracticeSetId=question.setId,QuestionText=question.text,Explanation=question.explanation,ImagePath=newPath,ImageAltText=question.imageAlt,Options=question.options,CorrectOption=question.correctIndex+1}; int id=QuestionRepository.Save(input);if(oldPath!=null&&!string.Equals(oldPath,newPath,StringComparison.Ordinal))QuestionImageStorage.DeleteIfUnreferenced(oldPath);return ToQuestion(QuestionRepository.GetById(id)); } catch(Exception ex) { if(newPath!=null&&!string.Equals(newPath,oldPath,StringComparison.Ordinal))QuestionImageStorage.DeleteIfUnreferenced(newPath); return new QuestionDto { error=ex.GetBaseException().Message }; } }
        [WebMethod(EnableSession = true)] public static QuestionDto Toggle(int id,bool active) { RequireAdmin();if(!QuestionRepository.SetActive(id,active))throw new ArgumentException("Question not found.");return ToQuestion(QuestionRepository.GetById(id)); }
        [WebMethod(EnableSession = true)] public static string Delete(int id) { RequireAdmin();QuestionDeleteResult r=QuestionRepository.Delete(id);if(r==QuestionDeleteResult.Deleted)return "Deleted";if(r==QuestionDeleteResult.HasAttempts)return "This question is protected because it appears in completed learner attempts.";if(r==QuestionDeleteResult.UsedByMockExam)return "Remove this question from Mock Exam Sets before deleting it.";return "Question not found."; }
        private static QuestionDto ToQuestion(DataRow r) { int id=Convert.ToInt32(r["QuestionId"]);DataTable opts=QuestionRepository.GetOptions(id);string[] texts=new string[3];int correct=-1;foreach(DataRow o in opts.Rows){int n=Convert.ToInt32(o["OptionOrder"])-1;texts[n]=Convert.ToString(o["OptionText"]);if((bool)o["IsCorrect"])correct=n;}return new QuestionDto{id=id,categoryId=Convert.ToInt32(r["QuestionCategoryId"]),setId=r.IsNull("PracticeSetId")?(int?)null:Convert.ToInt32(r["PracticeSetId"]),text=Convert.ToString(r["QuestionText"]),explanation=Convert.ToString(r["Explanation"]),image=r.IsNull("ImagePath")?null:Convert.ToString(r["ImagePath"]),imageAlt=r.IsNull("ImageAltText")?null:Convert.ToString(r["ImageAltText"]),status=(bool)r["IsActive"]?"Active":"Inactive",options=texts,correctIndex=correct,attemptCount=Convert.ToInt32(r["AttemptCount"]),mockExamUseCount=Convert.ToInt32(r["MockExamUseCount"])}; }
        private static void RequireAdmin() { if(!SessionHelper.IsAuthenticated(HttpContext.Current.Session)||!string.Equals(SessionHelper.CurrentRole(HttpContext.Current.Session),SessionHelper.RoleAdministrator,StringComparison.Ordinal))throw new HttpException(403,"Administrator access is required."); }
    }
}
