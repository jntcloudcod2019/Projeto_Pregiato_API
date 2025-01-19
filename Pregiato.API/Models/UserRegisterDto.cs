using System.Text.Json.Serialization;

namespace Pregiato.API.Models
{
    public class UserRegisterDto
    {
        public string Username { get; set; }
        public string? Email { get; set; }
        public string Password { get; set; }
    }
}
