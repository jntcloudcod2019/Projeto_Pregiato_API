using Microsoft.EntityFrameworkCore;
using Pregiato.API.Interface;
using Pregiato.API.Models;

namespace Pregiato.API.Data
{
    public class ClientRepository : IClientRepository
    {
        private readonly ModelAgencyContext _context;

        public ClientRepository(ModelAgencyContext context) 
        {
            _context = context ?? throw new ArgumentNullException(nameof(context)) ;
        }
        public async Task AddClientAsync(Client client)
        {
            _context.Clients.Add(client);   
            await _context.SaveChangesAsync();  
        }

        public async Task DeleteClientAsync(Guid id)
        {
            var idclient = await _context.Clients.FindAsync(id);
            if (idclient != null)
            {
               
                _context.Clients.Remove(idclient);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Client>> GetAllClientsAsync()
        {
           return await _context.Clients.ToListAsync(); 
        }

        public async Task<Client> GetByClientIdAsync(Guid id)
        {
            return await _context.Clients.FindAsync(id);
        }

        public async Task UpdateClientAsync(Client client)
        {
            _context.Clients.Update(client);    
            await _context.SaveChangesAsync();
        }

        public async Task<Client> GetByEmailOrDocumentAsync(string email, string clientDocument)
        {
            return await _context.Clients
                .FirstOrDefaultAsync(c => c.Email == email || c.ClientDocument == clientDocument);
        }

    }
}
