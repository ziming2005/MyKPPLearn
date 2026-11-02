using System;
using System.Data;
using System.Data.SqlClient;

namespace MyKPPLearn.Data
{
    public static class QuestionCategoryRepository
    {
        public static DataTable GetAll()
        {
            return DatabaseHelper.ExecuteQuery(@"SELECT QuestionCategoryId, Code, Title, Description, DisplayOrder, IsActive FROM QuestionCategories ORDER BY DisplayOrder, Title");
        }

        public static bool Exists(int categoryId)
        {
            return Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT COUNT(1) FROM QuestionCategories WHERE QuestionCategoryId=@Id", new SqlParameter("@Id", categoryId))) == 1;
        }
    }
}
