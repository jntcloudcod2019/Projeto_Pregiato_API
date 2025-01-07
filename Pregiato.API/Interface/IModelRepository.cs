using Pregiato.API.Models;

namespace Pregiato.API.Interface
{
    public interface IModelRepository
    {
        Task<IEnumerable<Moddels>> GetAllModelAsync();
        Task<Moddels> GetByIdModelAsync(Guid id);
        Task AddModelAsync(Moddels model);
        Task UpdateModelAsync(Moddels model);
        Task DeleteModelAsync(Guid id);
    }
}
