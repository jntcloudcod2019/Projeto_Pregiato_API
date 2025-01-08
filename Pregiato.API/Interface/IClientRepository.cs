using Pregiato.API.Models;

namespace Pregiato.API.Interface
{
    public interface IClientRepository
    {
        Task<IEnumerable<Client>> GetAllClientsAsync();
        Task<Client> GetByClientIdAsync(Guid id);
        Task AddClientAsync(Client client);
        Task UpdateClientAsync(Client client);
        Task DeleteClientAsync(Guid id);
    }
}
