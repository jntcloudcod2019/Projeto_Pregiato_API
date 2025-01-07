using System.Diagnostics.Contracts;

namespace Pregiato.API.Models
{
    public class Client
    {
        public int ClientId { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string ClientDocument { get; set; }
        public int? ContractId { get; set; }
        public string Contact { get; set; }    
    }
}
