using Microsoft.AspNetCore.Mvc;
using Pregiato.API.Data;
using Pregiato.API.Models;
using Pregiato.API.Requests;

namespace Pregiato.API.Interface
{
    public interface IContractService
{
     Task<ContractBase> GenerateContractAsync(CreateContractModelRequest createContractModelRequest, Guid modelId, string contractType, Dictionary<string, string> parameters);
     Task<List<ContractBase>> GenerateAllContractsAsync(CreateContractModelRequest createContractModelRequest);
     Task SaveContractAsync(ContractBase contract, Stream pdfStream, string cpfModelo);
     Task<string> GenerateContractPdf(int? codProposta, Guid? contractId);
     Task<ContractBase> GenerateContractCommitmentTerm(CreateRequestCommitmentTerm createRequestContractImageRights, string querymodel);
     Task<ContractBase> GenerateContractPhotographyProduction(PaymentRequest paymentRequest, string querymodel);
     Task<ContractBase> GenetayeContractImageRightsTerm(string querymodel);
     Task<ContractBase> GenerateContractAgency(string querymodel);
     Task<IActionResult> GetMyContracts(string type = "files");
     Task<List<ContractsModels>> GetContractsByModelIdAsync(Guid modelId);
     Task<byte[]> ExtractBytesFromString(string content);
     Task<string> ConvertBytesToString(byte[] bytes);

    }
}
