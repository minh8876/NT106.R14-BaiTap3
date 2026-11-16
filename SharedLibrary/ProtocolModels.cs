using System.Text.Json.Serialization;

namespace SharedLibrary
{
    public static class ProtocolOperations
    {
        public const string Register = "DANGKY";
        public const string Login = "DANGNHAP";
        public const string GetUserInfo = "LAYTHONGTIN";
        public const string Logout = "DANGXUAT";
    }

    public sealed class ClientRequest
    {
        public string Operation { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int? UserId { get; set; }
    }

    public sealed class ServerResponse
    {
        public bool Success { get; set; }
        public string ErrorCode { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public UserDto UserInfo { get; set; }
    }
}
