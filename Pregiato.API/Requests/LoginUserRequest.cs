using Pregiato.API.Models;
using System.ComponentModel.DataAnnotations;

namespace Pregiato.API.Requests
{
    public class LoginUserRequest 
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string ?UserType { get; set; }

    }
}
