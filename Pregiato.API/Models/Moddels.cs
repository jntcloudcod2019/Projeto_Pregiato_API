using System.Security;

namespace Pregiato.API.Models
{
    public class Moddels
    {
        public Guid IdModel { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string CPF { get; set; }
        public string RG { get; set; }
        public string Email { get; set; }
        public string PostalCode { get; set; }
        public string Address { get; set; }
        public string BankAccount { get; set; }
        public bool Status { get; set; } = true;
        public string PasswordHash { get; set; }
    }
}
