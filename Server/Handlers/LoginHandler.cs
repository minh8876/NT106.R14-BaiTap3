using Server.Security;
using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Text;
using SharedLibrary;

namespace Server.Handlers
{
    public class LoginHandler
    {
        private readonly string _connectionString = "CHUỖI_KẾT_NỐI_SQL_CỦA_NHÓM";

        public LoginResponse Handle(LoginRequest request)
        {
            var response = new LoginResponse();
            string commonErrorMsg = "Sai tên đăng nhập hoặc mật khẩu";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT UserId, Username, PasswordHash, PasswordSalt, FullName, Email FROM NguoiDung WHERE Username = @Username";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Username", request.Username);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string savedHash = reader["PasswordHash"].ToString();
                        string savedSalt = reader["PasswordSalt"].ToString();

                        bool isValid = PasswordHasher.VerifyPassword(request.Password, savedHash, savedSalt);

                        if (isValid)
                        {
                            response.IsSuccess = true;
                            response.UserInfo = new UserDto
                            {
                                UserId = (int)reader["UserId"],
                                Username = reader["Username"].ToString(),
                                FullName = reader["FullName"].ToString(),
                                Email = reader["Email"].ToString()
                            };
                        }
                        else
                        {
                            response.IsSuccess = false;
                            response.ErrorMessage = commonErrorMsg;
                        }
                    }
                    else
                    {
                        response.IsSuccess = false;
                        response.ErrorMessage = commonErrorMsg;
                    }
                }
            }
            return response;
        }
    }
}
