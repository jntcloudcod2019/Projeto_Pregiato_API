using Pregiato.API.Models;

namespace Pregiato.API.Services.ServiceModels
{
    public class ContractWithProducers
    {
        public ContractBase Contract { get; set; }
        public Producers Producers { get; set; }
    }
}
