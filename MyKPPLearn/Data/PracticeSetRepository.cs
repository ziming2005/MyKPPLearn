using System;
using System.Data;
using System.Data.SqlClient;

namespace MyKPPLearn.Data
{
    public enum PracticeSetDeleteResult { Deleted, NotFound, HasQuestions, HasAttempts }

    public static class PracticeSetRepository
    {
        private const string SelectSql = @"SELECT p.PracticeSetId, p.QuestionCategoryId, p.SetNumber, p.IsActive, c.Code AS CategoryCode, c.Title AS CategoryTitle,
            (SELECT COUNT(*) FROM Questions q WHERE q.PracticeSetId=p.PracticeSetId) AS QuestionCount,
            (SELECT COUNT(*) FROM Questions q WHERE q.PracticeSetId=p.PracticeSetId AND q.IsActive=1) AS ActiveQuestionCount,
            (SELECT COUNT(*) FROM PracticeAttempts a WHERE a.PracticeSetId=p.PracticeSetId) AS AttemptCount
            FROM PracticeSets p INNER JOIN QuestionCategories c ON c.QuestionCategoryId=p.QuestionCategoryId";

        public static DataTable GetAllForAdmin() { return DatabaseHelper.ExecuteQuery(SelectSql + " ORDER BY c.DisplayOrder,p.SetNumber"); }
        public static DataTable GetByCategory(int categoryId) { return DatabaseHelper.ExecuteQuery(SelectSql + " WHERE p.QuestionCategoryId=@Id ORDER BY p.SetNumber", new SqlParameter("@Id", categoryId)); }
        public static DataRow GetById(int id) { DataTable t=DatabaseHelper.ExecuteQuery(SelectSql+" WHERE p.PracticeSetId=@Id",new SqlParameter("@Id",id)); return t.Rows.Count==0?null:t.Rows[0]; }
        public static int Create(int categoryId, int setNumber) { return Convert.ToInt32(DatabaseHelper.ExecuteScalar(@"INSERT INTO PracticeSets(QuestionCategoryId,SetNumber,IsActive,CreatedAt,UpdatedAt) OUTPUT INSERTED.PracticeSetId VALUES(@CategoryId,@SetNumber,0,SYSUTCDATETIME(),SYSUTCDATETIME())",new SqlParameter("@CategoryId",categoryId),new SqlParameter("@SetNumber",setNumber))); }
        public static bool Update(int id,int categoryId,int setNumber) { return DatabaseHelper.ExecuteNonQuery(@"UPDATE PracticeSets SET QuestionCategoryId=@CategoryId,SetNumber=@SetNumber,UpdatedAt=SYSUTCDATETIME() WHERE PracticeSetId=@Id",new SqlParameter("@Id",id),new SqlParameter("@CategoryId",categoryId),new SqlParameter("@SetNumber",setNumber))==1; }
        public static bool SetActive(int id,bool active) { if(active && Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Questions WHERE PracticeSetId=@Id AND IsActive=1",new SqlParameter("@Id",id)))==0)return false; return DatabaseHelper.ExecuteNonQuery("UPDATE PracticeSets SET IsActive=@Active,UpdatedAt=SYSUTCDATETIME() WHERE PracticeSetId=@Id",new SqlParameter("@Id",id),new SqlParameter("@Active",active))==1; }
        public static PracticeSetDeleteResult Delete(int id)
        {
            using(SqlConnection c=DatabaseHelper.GetConnection()) { c.Open(); using(SqlTransaction tx=c.BeginTransaction()) { try {
                if(Scalar(c,tx,"SELECT COUNT(*) FROM PracticeSets WHERE PracticeSetId=@Id",id)==0){tx.Rollback();return PracticeSetDeleteResult.NotFound;}
                if(Scalar(c,tx,"SELECT COUNT(*) FROM PracticeAttempts WITH (UPDLOCK,HOLDLOCK) WHERE PracticeSetId=@Id",id)>0){tx.Rollback();return PracticeSetDeleteResult.HasAttempts;}
                if(Scalar(c,tx,"SELECT COUNT(*) FROM Questions WITH (UPDLOCK,HOLDLOCK) WHERE PracticeSetId=@Id",id)>0){tx.Rollback();return PracticeSetDeleteResult.HasQuestions;}
                using(SqlCommand cmd=new SqlCommand("DELETE FROM PracticeSets WHERE PracticeSetId=@Id",c,tx)){cmd.Parameters.AddWithValue("@Id",id);cmd.ExecuteNonQuery();} tx.Commit(); return PracticeSetDeleteResult.Deleted;
            } catch {tx.Rollback();throw;} } }
        }
        private static int Scalar(SqlConnection c,SqlTransaction tx,string sql,int id) { using(SqlCommand cmd=new SqlCommand(sql,c,tx)){cmd.Parameters.AddWithValue("@Id",id);return Convert.ToInt32(cmd.ExecuteScalar());} }
    }
}
