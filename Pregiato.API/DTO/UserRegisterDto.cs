using System.Text.Json.Serialization;

namespace Pregiato.API.DTO
{
    public class UserRegisterDto
    {
        [JsonPropertyName("FullName")]
        public string? Username { get; set; }

        public string? Email { get; set; }

        public string? WhatsApp { get; set; }

        public string? Cpf { get; set; }

        [JsonIgnore]
        public string? Password { get; set; }
    }
}
