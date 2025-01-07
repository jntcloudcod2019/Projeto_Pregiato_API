using System.Security;

namespace Pregiato.API.Models
{
    public class Moddels
    {
        public int ModelId { get; set; }
        public string Name { get; set; }
        public string CPF { get; set; }
        public string RG { get; set; }
        public string Ethnicity { get; set; }
        public int Age { get; set; }
        public string Nationality { get; set; }
        public double Weight { get; set; }
        public string Email { get; set; }
        public char Gender { get; set; }
        public string City { get; set; }
        public string ZIP { get; set; }
        public string Address { get; set; }
        public double Height { get; set; }
        public string BankAccount { get; set; }
        public int? ContractId { get; set; }
        public Contract Contract { get; set; }
        public int? JobId { get; set; }
        public Job Job { get; set; }
        public bool Status { get; set; }
        public string Password { get; set; }
    }
}
