using Pregiato.API.Enums;
using Pregiato.API.Models;

namespace Pregiato.API.Interfaces
{
    public interface IRabbitMQProducer
    {
        Task<string> SendMensage(List<ContractBase>? contracts, string modelDocument);
        Task<string> SendMensageCreateContract(Guid? idContract, string modelDocument);
        Task<string> SendCommitmentTerm (ContractCommitmentTerm contractCommitment);
        Task<Task> SendMessageWhatsAppAsync(string queueName, object message);
        Task<RegistrationResult> SendMessageDeleteContractAsync(DocumentsAutentique documentsAutentique, object message);


    }
}
