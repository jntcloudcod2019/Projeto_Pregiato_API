using Pregiato.API.Models;

namespace Pregiato.API.Interfaces
{
    public interface IRabbitMQProducer
    {
        Task<string> SendMensage(List<ContractBase> contracts, string modelDocument);
    }
}
