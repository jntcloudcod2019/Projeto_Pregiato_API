using Pregiato.API.Data;
using Pregiato.API.Models;

namespace Pregiato.API.Interface
{
    public interface IContractService
    {
        Task<ContractBase> GenerateContractAsync(Guid modelId, Guid jobId, string contractType, Dictionary<string, string> parameters);
        Task SaveContractAsync(ContractBase contract, Stream pdfStream);
        Task<ContractBase?> GetContractByIdAsync(Guid contractId);
    }
}
