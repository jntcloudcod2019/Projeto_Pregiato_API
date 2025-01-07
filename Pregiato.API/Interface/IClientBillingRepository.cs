using Pregiato.API.Models;

namespace Pregiato.API.Interface
{
    public interface IClientBillingRepository
    {
        Task<IEnumerable<ClientBilling>> GetAllClientBillingAsync();
        Task<ClientBilling> GetByIdClientBillingAsync(Guid id);
        Task AddClientBillingAsync(ClientBilling clientBilling);
        Task UpdateClientBillingAsync(ClientBilling clientBilling);
        Task DeleteClientBillingAsync(Guid id);
    }
}
