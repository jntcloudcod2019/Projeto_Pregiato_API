using System.ComponentModel.DataAnnotations;

namespace Pregiato.API.Requests
{
    public class CreateClientRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]  
        public string ClientDocument { get; set; }
        [Required]  
        public string Contact { get; set; }
    }
}
