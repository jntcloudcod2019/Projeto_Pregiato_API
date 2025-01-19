using Microsoft.AspNetCore.Identity;
using System.Security;
using System.Text.Json.Serialization;

namespace Pregiato.API.Models
{
    public class User
    {
        public Guid UserId { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
        public string PasswordHash { get; set; }
        public string  UserType { get; set; }  
       
    }
}
