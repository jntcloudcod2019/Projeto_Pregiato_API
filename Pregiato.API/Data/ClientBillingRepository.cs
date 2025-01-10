using Microsoft.EntityFrameworkCore;
using Pregiato.API.Interface;
using Pregiato.API.Models;

namespace Pregiato.API.Data
{
    public class ClientBillingRepository : IClientBillingRepository
    {
        private readonly ModelAgencyContext _context;

        public ClientBillingRepository(ModelAgencyContext context)
        {
            _context = context;
        }

        public async Task AddClientBillingAsync(ClientBilling clientBilling)
        {
            _context.ClientsBilling.Add(clientBilling);
            await _context.SaveChangesAsync();
            await Task.CompletedTask;

        }

        public async Task DeleteClientBillingAsync(Guid id)
        {
            var idclientBilling = await _context.Clients.FindAsync(id);
            if (idclientBilling != null)
            {
                _context.Clients.Remove(idclientBilling);
                await _context.SaveChangesAsync();
                await Task.CompletedTask;
            }
        }

        public async Task<IEnumerable<ClientBilling>> GetAllClientBillingAsync()
        {
           return await _context.ClientsBilling.ToListAsync();
            await Task.CompletedTask;
        }

        public async Task<ClientBilling> GetByIdClientBillingAsync(Guid id)
        {
            return await _context.ClientsBilling.FindAsync(id);
            await Task.CompletedTask;
        }

        public async Task UpdateClientBillingAsync(ClientBilling clientBilling)
        {
            _context.ClientsBilling.Update(clientBilling);
            await _context.SaveChangesAsync();
            await Task.CompletedTask;
        }
    } 
}
