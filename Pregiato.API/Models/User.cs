using Microsoft.AspNetCore.Identity;
using System.Security;

namespace Pregiato.API.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string PasswordHash { get; set; }
    }
}
