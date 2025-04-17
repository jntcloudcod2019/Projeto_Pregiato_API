using Microsoft.AspNetCore.Mvc;
using Pregiato.API.Models;
using Pregiato.API.Requests;
using Pregiato.API.Services.ServiceModels;

namespace Pregiato.API.Interfaces
{
    public interface IContractService
    {
     Task<ContractBase> GenerateContractAsync(CreateContractModelRequest createContractModelRequest, Model model, string contractType, Dictionary<string, string> parameters);
     Task<List<ContractBase>> GenerateAllContractsAsync(CreateContractModelRequest createContractModelRequest, Model model);
     Task SaveContractAsync(ContractBase contract, Stream pdfStream, string cpfModelo);
     Task<ContractBase> GenerateContractCommitmentTerm(CreateRequestCommitmentTerm createRequestContractImageRights, string querymodel);
     Task<ContractBase> GenetayeContractImageRightsTerm(string querymodel);
     Task<string> GenerateProducerCodeContractAsync();
     Task<IActionResult> GetMyContracts(string type = "files");
     Task<List<ContractsModels>> GetContractsByModelIdAsync(Guid modelId);
     Task<byte[]> ExtractBytesFromString(string content);
     Task<string> ConvertBytesToString(byte[] bytes);
     Task<string> PopulateTemplate(string template, Dictionary<string, string> parameters);
     Task<byte[]> ConvertHtmlToPdf(string htmlTemplate, Dictionary<string, string> parameters);
     Task<Dictionary<string, string>> AddSignatureToParameters(Dictionary<string, string> parameters, string contractType);
     Task AddMinorModelInfo(Model model, Dictionary<string, string> parameters);
     Task<Producers> ProcessProducersAsync(ContractBase contract,  Model model);
     Task<ContractWithProducers> DefineContractAsync(ContractBase contract,
         CreateContractModelRequest contractModelRequest, Model model, string? contractType);
     Task<ContractBase> DefineContractAgencyAsync(ContractBase contract,
         CreateContractModelRequest contractModelRequest, Model model, string? contractType);



    }
}
