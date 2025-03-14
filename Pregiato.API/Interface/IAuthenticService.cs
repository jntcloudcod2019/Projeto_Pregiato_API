using Pregiato.API.Models;

namespace Pregiato.API.Interface
{
    public interface IAuthenticService
    {
        Task<List<ContractsModels>> UploadContractAsync(List<ContractBase> contracts, Guid modelId);
        Task SendContractsForSignatureAsync(List<string> documentosIds);

    }
}
