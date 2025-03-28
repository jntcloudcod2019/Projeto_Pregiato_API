namespace Pregiato.API.Response
{
    public class LoginResponse
    {
        public string Token { get; set; }
        public UserInfo User { get; set; }
    }

    public class UserInfo
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string UserType { get; set; }
    }

    public class ErrorResponse
    {
        public string Message { get; set; }
        public string Details { get; set; }
    }
}
