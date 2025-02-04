using Pregiato.API.Requests;
using System.ComponentModel.DataAnnotations;

namespace Pregiato.API.Models
{
    public class JwtHookRequest 
    {
        [EmailAddress]
        public string Email { get; set; }   
        public string? Username { get; set; }
        public string? UserType { get; set; }
    }
}
