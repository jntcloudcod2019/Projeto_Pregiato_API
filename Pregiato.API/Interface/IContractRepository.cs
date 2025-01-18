using Pregiato.API.Models;
using System.Diagnostics.Contracts;

namespace Pregiato.API.Interface
{
    public interface IContractRepository
    {
        Task AddAsync(ContractsModels contract);
        Task<ContractsModels> GetByIdAsync(Guid id);
        Task UpdateAsync(ContractsModels contract);
        Task DeleteAsync(ContractsModels contract);
    }
}
