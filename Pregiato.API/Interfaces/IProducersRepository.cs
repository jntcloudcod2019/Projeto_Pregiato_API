using Pregiato.API.Models;

namespace Pregiato.API.Interfaces
{
    public interface IProducersRepository
    {
        Task SaveProducersAsync(Producers proceducers);

    }
}
