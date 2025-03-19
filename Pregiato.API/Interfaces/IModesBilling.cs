using Pregiato.API.Models;

namespace Pregiato.API.Interface
{
    public interface IModesBilling
    {
        Task<IEnumerable<ModelsBilling>> GetAllModelsBillingAsync();
        Task<ModelsBilling> GetByIdModelsBillingAsync(Guid id);
        Task AddModelsBillingAsync(ModelsBilling modelsBilling);
        Task UpdateModelsBillingAsync(ModelsBilling modelsBilling);
        Task DeleteModelsBillingAsync(Guid id);
    }
}
