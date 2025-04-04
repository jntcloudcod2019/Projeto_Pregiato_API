using Pregiato.API.DTO;
using Pregiato.API.Models;
using Pregiato.API.Services.ServiceModels;

namespace Pregiato.API.Interfaces
{
    public interface IContractRepository
    {
        Task AddAsync(ContractsModels contract);
        Task<ContractsModels> GetByIdContractAsync(Guid id);
        Task UpdateAsync(ContractsModels contract);
        Task DeleteAsync(ContractsModels contract);
        Task SaveContractAsync(ContractBase contract);
        Task<List<ContractBase>> GetContractsByModelId(Guid modelId);
        Task<ContractBase> GetContractByCriteriaAsync(string? contractId, string? modelId, int? codProposta);
        Task<ContractDTO?> DownloadContractAsync(int proposalCode);
        Task<List<ContractSummaryDTO>> GetAllContractsAsync();
        Task<List<ContractSummaryDTO>> GetAllContractsForProducersAsync(string codPrducers);

    }
}
