using Pregiato.API.Models;

namespace Pregiato.API.Interface
{
    public interface IContractRepository
    {
        Task<IEnumerable<Contract>> GetAllContractAsync();
        Task<Contract> GetByIdContractAsync(Guid id);
        Task AddContractAsync(Contract contract);
        Task UpdateContractAsync(Contract contract);
        Task DeleteContractAsync(Guid id);
    }
}
