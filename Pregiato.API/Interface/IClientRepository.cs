using Pregiato.API.Models;

namespace Pregiato.API.Interface
{
    public interface IClientRepository
    {
        Task<IEnumerable<Client>> GetAllClientsAsync();
        Task<Client> GetByClientIdAsync(int id);
        Task AddClientAsync(Client client);
        Task UpdateClientAsync(Client client);
        Task DeleteAsync(Guid id);
    }
}
