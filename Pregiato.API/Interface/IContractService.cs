using Pregiato.API.Data;
using Pregiato.API.Models;

namespace Pregiato.API.Interface
{
    public interface IContractService
{
     Task<ContractBase> GenerateContractAsync(Guid modelId, Guid jobId, string contractType, Dictionary<string, string> parameters);
     Task<List<ContractBase>> GenerateAllContractsAsync(string? idModel = null, string? cpf = null, string? rg = null, Guid? jobId = null);
     Task SaveContractAsync(ContractBase contract, Stream pdfStream);
     Task<string> GenerateContractPdf(int? codProposta, Guid? contractId);
    }
}
