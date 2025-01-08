using System.Diagnostics.Contracts;

namespace Pregiato.API.Models
{
    public class Client
    {
        public Guid IdClient { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string ClientDocument { get; set; }
        public string Contact { get; set; }    
    }
}
