using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Services;
using MyKPPLearn.Data;
using MyKPPLearn.Security;

namespace MyKPPLearn.Admin
{
    /// <summary>
    /// Admin Practice Sets. The page markup is a shell; Scripts/practice-admin-db.js calls the page methods below
    /// (JSON over POST) to list, add, edit, activate/deactivate and delete sets. Every method re-checks the
    /// Administrator role because page methods bypass Page_Load.
    /// </summary>
    public partial class PracticeSets : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SessionHelper.RequireAdministrator(this);
            ((AdminMaster)Master).PageHeading = "Practice Sets";
        }

        public sealed class CategoryDto
        {
            public int id;
            public string code;
            public string title;
        }

        public sealed class SetDto
        {
            public int id;
            public int categoryId;
            public int number;
            public string status;
            public int questionCount;
            public int activeQuestionCount;
            public int attemptCount;
            public string error;
        }

        public sealed class PracticeDataDto
        {
            public List<CategoryDto> categories;
            public List<SetDto> sets;
        }

        [WebMethod(EnableSession = true)]
        public static PracticeDataDto LoadData()
        {
            RequireAdmin();

            return new PracticeDataDto
            {
                categories = LoadCategories(),
                sets = LoadSets()
            };
        }

        [WebMethod(EnableSession = true)]
        public static SetDto Save(int id, int categoryId, int number)
        {
            RequireAdmin();

            if (number < 1)
            {
                return new SetDto { error = "Practice Set number must be greater than zero." };
            }

            if (!QuestionCategoryRepository.Exists(categoryId))
            {
                return new SetDto { error = "Select a valid category." };
            }

            try
            {
                if (id == 0)
                {
                    id = PracticeSetRepository.Create(categoryId, number);
                }
                else
                {
                    PracticeSetUpdateResult result = PracticeSetRepository.Update(id, categoryId, number);

                    if (result == PracticeSetUpdateResult.NotFound)
                    {
                        return new SetDto { error = "Practice Set not found." };
                    }

                    if (result == PracticeSetUpdateResult.CategoryLocked)
                    {
                        return new SetDto { error = "Move or delete this set's questions before changing its category." };
                    }
                }
            }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
            {
                return new SetDto { error = "That category already has this Practice Set number." };
            }

            return ToSet(PracticeSetRepository.GetById(id));
        }

        [WebMethod(EnableSession = true)]
        public static SetDto Toggle(int id, bool active)
        {
            RequireAdmin();

            if (!PracticeSetRepository.SetActive(id, active))
            {
                return new SetDto
                {
                    error = active ? "Add at least one active question before activating this Practice Set." : "Practice Set not found."
                };
            }

            return ToSet(PracticeSetRepository.GetById(id));
        }

        [WebMethod(EnableSession = true)]
        public static string Delete(int id)
        {
            RequireAdmin();

            switch (PracticeSetRepository.Delete(id))
            {
                case PracticeSetDeleteResult.Deleted:
                    return "Deleted";
                case PracticeSetDeleteResult.HasQuestions:
                    return "Move or delete this set's questions first.";
                case PracticeSetDeleteResult.HasAttempts:
                    return "This Practice Set is protected because learners have attempted it.";
                default:
                    return "Practice Set not found.";
            }
        }

        /// <summary>Shared with the Question Bank page, which offers the same categories and sets in its form.</summary>
        internal static List<CategoryDto> LoadCategories()
        {
            List<CategoryDto> categories = new List<CategoryDto>();

            foreach (DataRow row in QuestionCategoryRepository.GetAll().Rows)
            {
                categories.Add(new CategoryDto
                {
                    id = Convert.ToInt32(row["QuestionCategoryId"]),
                    code = Convert.ToString(row["Code"]),
                    title = Convert.ToString(row["Title"])
                });
            }

            return categories;
        }

        internal static List<SetDto> LoadSets()
        {
            List<SetDto> sets = new List<SetDto>();

            foreach (DataRow row in PracticeSetRepository.GetAllForAdmin().Rows)
            {
                sets.Add(ToSet(row));
            }

            return sets;
        }

        internal static void RequireAdmin()
        {
            if (!SessionHelper.IsInRole(HttpContext.Current.Session, SessionHelper.RoleAdministrator))
            {
                throw new HttpException(403, "Administrator access is required.");
            }
        }

        private static SetDto ToSet(DataRow row)
        {
            return new SetDto
            {
                id = Convert.ToInt32(row["PracticeSetId"]),
                categoryId = Convert.ToInt32(row["QuestionCategoryId"]),
                number = Convert.ToInt32(row["SetNumber"]),
                status = Convert.ToBoolean(row["IsActive"]) ? "Active" : "Inactive",
                questionCount = Convert.ToInt32(row["QuestionCount"]),
                activeQuestionCount = Convert.ToInt32(row["ActiveQuestionCount"]),
                attemptCount = Convert.ToInt32(row["AttemptCount"])
            };
        }
    }
}
