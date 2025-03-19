using Pregiato.API.DTO;
using Pregiato.API.Models;
using Pregiato.API.Requests;
using System.Diagnostics.Contracts;

namespace Pregiato.API.Interface
{
    public interface IContractRepository
    {
        Task AddAsync(ContractsModels contract);
        Task<ContractsModels> GetByIdContractAsync(Guid id);
        Task UpdateAsync(ContractsModels contract);
        Task DeleteAsync(ContractsModels contract);
        Task SaveContractAsync(ContractBase contract);
        Task<ContractBase?> GetContractByIdAsync( int? codProposta, Guid? contractId);
        Task<List<ContractBase>> GetContractsByModelId(Guid modelId);
        Task<ContractBase> GetContractByCriteriaAsync(string? contractId, string? modelId, int? codProposta);
        Task<ContractDTO?> DownloadContractAsync(int proposalCode);
    }
}
