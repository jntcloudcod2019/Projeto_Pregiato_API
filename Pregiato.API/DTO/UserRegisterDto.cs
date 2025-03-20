using System.Text.Json.Serialization;

namespace Pregiato.API.DTO
{
    public class UserRegisterDto
    {
        public string? Username { get; set; }
        public string? Email { get; set; }

        [JsonIgnore]
        public string? Password { get; set; }
    }
}
