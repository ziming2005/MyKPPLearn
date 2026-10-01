using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace MyKPPLearn.Data
{
    /// <summary>
    /// Thin ADO.NET helper around the MyKPPLearnConnectionString connection string.
    /// All SQL text passed here must use parameters for any user-supplied value.
    /// </summary>
    public static class DatabaseHelper
    {
        private static string ConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings["MyKPPLearnConnectionString"].ConnectionString; }
        }

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        public static int ExecuteNonQuery(string commandText, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = GetConnection())
            using (SqlCommand command = new SqlCommand(commandText, connection))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                connection.Open();
                return command.ExecuteNonQuery();
            }
        }

        public static object ExecuteScalar(string commandText, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = GetConnection())
            using (SqlCommand command = new SqlCommand(commandText, connection))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                connection.Open();
                return command.ExecuteScalar();
            }
        }

        public static DataTable ExecuteQuery(string commandText, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = GetConnection())
            using (SqlCommand command = new SqlCommand(commandText, connection))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                DataTable table = new DataTable();
                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    adapter.Fill(table);
                }

                return table;
            }
        }
    }
}
