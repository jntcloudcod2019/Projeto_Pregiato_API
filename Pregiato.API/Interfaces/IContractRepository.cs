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
        Task DeleteAsync(Guid idContract, int? proposalCode, Guid? IdModel);
        Task SaveContractAsync(ContractBase contract);
        Task<List<ContractBase>> GetContractsByModelId(Guid modelId);
        Task<ContractBase> GetContractByCriteriaAsync(Guid idContract, int? proposalCode, Guid? IdModel);
        Task<ContractDTO?> DownloadContractAsync(int proposalCode);
        Task<List<ContractSummaryDTO>> GetAllContractsAsync();
        Task<List<ContractSummaryDTO>> GetAllContractsForProducersAsync(string codPrducers);
        Task<List<ContractBase>> ExistsContractForTodayAsync(Guid idmodel);

    }
}
