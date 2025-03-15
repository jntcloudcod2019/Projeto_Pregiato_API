using Pregiato.API.Models;

namespace Pregiato.API.Interface
{
    public interface IRabbitMQProducer
    {
        Task<string> SendMensage(List<ContractBase> contracts, string modelDocument);
    }
}
