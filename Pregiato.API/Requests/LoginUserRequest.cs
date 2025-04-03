using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Pregiato.API.Requests
{
    public class LoginUserRequest
    {
        [Required]
        [JsonPropertyName("FULLNAME")]
        public string? NickNAme { get; set; }

        [Required]
        public string? Password { get; set; }

        [JsonIgnore]
        public string? UserType { get; set; }

        [JsonIgnore]
        [EmailAddress]
        public string? Email { get; set; }

        [JsonIgnore]
        public Guid? IdUser { get; set; }

    }
}
