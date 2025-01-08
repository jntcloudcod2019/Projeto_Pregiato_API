using System.ComponentModel.DataAnnotations;

namespace Pregiato.API.Requests
{
    public class CreateModelRequest
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string CPF { get; set; }
        [Required]
        public string RG { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string PostalCode { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string BankAccount { get; set; }
        [Required]
        public string PasswordHash { get; set; }
    }
}
