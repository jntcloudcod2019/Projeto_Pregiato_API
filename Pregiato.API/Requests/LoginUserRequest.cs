using Pregiato.API.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Pregiato.API.Requests
{
    public class LoginUserRequest
    {
        [Required]
        public string? NickName { get; set; }

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
