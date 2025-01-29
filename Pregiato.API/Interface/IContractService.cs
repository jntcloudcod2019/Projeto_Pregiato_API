using Pregiato.API.Data;
using Pregiato.API.Models;
using Pregiato.API.Requests;

namespace Pregiato.API.Interface
{
    public interface IContractService
{
     Task<ContractBase> GenerateContractAsync(PaymentRequest paymentRequest, Guid modelId, string contractType, Dictionary<string, string> parameters);
     Task<List<ContractBase>> GenerateAllContractsAsync(PaymentRequest paymentRequest, string? idModel = null, string? cpf = null, string? rg = null);
     Task SaveContractAsync(ContractBase contract, Stream pdfStream);
     Task<string> GenerateContractPdf(int? codProposta, Guid? contractId);
     Task<ContractBase> GenerateContractCommitmentTerm(CreateRequestContractImageRights createRequestContractImageRights,  string querymodel);

    }
}
