using Microsoft.Data.SqlClient;
using standcalcwaspnet.Models;
using System.Data;

namespace standcalcwaspnet.Data
{
    public class ServerData
    {
        SqlConnection _connection = null;
        SqlCommand _command = null;
        public static IConfiguration Configuration { get; set; }
        private string GetConnectionString()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            Configuration = builder.Build();
            return Configuration.GetConnectionString("DefaultConnection");
        }
        public List<LoginModel> GetAllUsers()
        {
            List<LoginModel> userList = new List<LoginModel>();
            using (_connection = new SqlConnection(GetConnectionString()))
            {
                _command = _connection.CreateCommand();
                _command.CommandType = CommandType.StoredProcedure;
                _command.CommandText = "usp_GetAllUsers";
                _connection.Open();
                SqlDataReader reader = _command.ExecuteReader();
                while (reader.Read())
                {
                    LoginModel user = new LoginModel();
                    user.UserName = reader["UserName"].ToString();
                    user.Password = reader["Password"].ToString();
                    user.UserID = Convert.ToInt32(reader["UserID"]);
                    userList.Add(user);
                }
                _connection.Close();
            }
            return userList;
        }
        public bool AddUser(LoginModel model)
        {
            int id = 0;
            using (_connection = new SqlConnection(GetConnectionString()))
            {
                _command = _connection.CreateCommand();
                _command.CommandType = CommandType.StoredProcedure;
                _command.CommandText = "usp_InsertUser";
                _command.Parameters.AddWithValue("Username", model.UserName);
                _command.Parameters.AddWithValue("Password", model.Password);
                _command.Parameters.AddWithValue("UserCreationDate", model.UserCreationDate);
                _command.Parameters.AddWithValue("AuthType", model.AuthType);
                _connection.Open();
                id = _command.ExecuteNonQuery();
                _connection.Close();
            }
            return id > 0 ? true : false;
        }
        public bool DeleteUser(int uid)
        {
            int id = 0;
            using (_connection = new SqlConnection(GetConnectionString()))
            {
                _command = _connection.CreateCommand();
                _command.CommandType = CommandType.StoredProcedure;
                _command.CommandText = "usp_DeleteUser";
                _command.Parameters.AddWithValue("Id", uid);
                _connection.Open();
                id = _command.ExecuteNonQuery();
                _connection.Close();
            }
            return id > 0 ? true : false;
        }
    }
}
