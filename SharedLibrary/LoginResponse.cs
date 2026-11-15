using System;
using System.Collections.Generic;
using System.Text;

namespace SharedLibrary
{
    public class LoginResponse
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public UserDto UserInfo { get; set; }
    }
}
