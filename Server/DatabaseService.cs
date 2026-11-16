using Server.Security;
using SharedLibrary;
using System.Data;
using System.Data.SqlClient;

namespace Server
{
    public sealed class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(string connectionString)
        {
            _connectionString = string.IsNullOrWhiteSpace(connectionString)
                ? throw new ArgumentException("The NT106_CONNECTION_STRING environment variable is required.", nameof(connectionString))
                : connectionString;
        }

        public async Task<ServerResponse> RegisterAsync(ClientRequest request, CancellationToken cancellationToken)
        {
            using SqlConnection connection = new(_connectionString);
            await connection.OpenAsync(cancellationToken);

            const string existsSql = "SELECT COUNT(1) FROM NguoiDung WHERE Username = @Username";
            using (SqlCommand existsCommand = new(existsSql, connection))
            {
                existsCommand.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = request.Username;
                int count = Convert.ToInt32(await existsCommand.ExecuteScalarAsync(cancellationToken));
                if (count > 0)
                {
                    return Failure("DUPLICATE_USERNAME", "Tên ??ng nh?p ?ã t?n t?i.");
                }
            }

            PasswordHasher.HashPassword(request.Password, out string hash, out string salt);
            const string insertSql = """
                INSERT INTO NguoiDung (Username, PasswordHash, PasswordSalt, FullName, Email)
                VALUES (@Username, @PasswordHash, @PasswordSalt, @FullName, @Email)
                """;

            using SqlCommand insertCommand = new(insertSql, connection);
            AddAccountParameters(insertCommand, request, hash, salt);
            await insertCommand.ExecuteNonQueryAsync(cancellationToken);
            return new ServerResponse { Success = true };
        }

        public async Task<ServerResponse> AuthenticateAsync(ClientRequest request, CancellationToken cancellationToken)
        {
            const string sql = "SELECT UserId, Username, PasswordHash, PasswordSalt, FullName, Email FROM NguoiDung WHERE Username = @Username";
            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(sql, connection);
            command.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = request.Username;
            await connection.OpenAsync(cancellationToken);

            using SqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                return Failure("INVALID_CREDENTIALS", "Sai tên ??ng nh?p ho?c m?t kh?u.");
            }

            string savedHash = reader["PasswordHash"].ToString() ?? string.Empty;
            string savedSalt = reader["PasswordSalt"].ToString() ?? string.Empty;
            if (!PasswordHasher.VerifyPassword(request.Password, savedHash, savedSalt))
            {
                return Failure("INVALID_CREDENTIALS", "Sai tên ??ng nh?p ho?c m?t kh?u.");
            }

            return new ServerResponse
            {
                Success = true,
                UserInfo = ReadUser(reader)
            };
        }

        public async Task<ServerResponse> GetUserInfoAsync(int userId, CancellationToken cancellationToken)
        {
            const string sql = "SELECT UserId, Username, FullName, Email FROM NguoiDung WHERE UserId = @UserId";
            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new(sql, connection);
            command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
            await connection.OpenAsync(cancellationToken);

            using SqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                return Failure("USER_NOT_FOUND", "Không tìm th?y thông tin ng??i dùng.");
            }

            return new ServerResponse { Success = true, UserInfo = ReadUser(reader) };
        }

        private static void AddAccountParameters(SqlCommand command, ClientRequest request, string hash, string salt)
        {
            command.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = request.Username;
            command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 200).Value = hash;
            command.Parameters.Add("@PasswordSalt", SqlDbType.NVarChar, 200).Value = salt;
            command.Parameters.Add("@FullName", SqlDbType.NVarChar, 100).Value = request.FullName;
            command.Parameters.Add("@Email", SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(request.Email) ? DBNull.Value : request.Email;
        }

        private static UserDto ReadUser(SqlDataReader reader)
        {
            return new UserDto
            {
                UserId = Convert.ToInt32(reader["UserId"]),
                Username = reader["Username"].ToString() ?? string.Empty,
                FullName = reader["FullName"].ToString() ?? string.Empty,
                Email = reader["Email"] == DBNull.Value ? string.Empty : reader["Email"].ToString() ?? string.Empty
            };
        }

        private static ServerResponse Failure(string code, string message) => new()
        {
            ErrorCode = code,
            ErrorMessage = message
        };
    }
}
