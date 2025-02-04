using Pregiato.API.Models;
using System.ComponentModel.DataAnnotations;

namespace Pregiato.API.Requests
{
    public class LoginUserRequest 
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        public string ?UserType { get; set; }

    }
}
