using Microsoft.EntityFrameworkCore;
using Pregiato.API.Interface;
using Pregiato.API.Models;

namespace Pregiato.API.Data
{
    public class ContractRepository : IContractRepository
    {
        private readonly ModelAgencyContext _context;

        public ContractRepository(ModelAgencyContext context) 
        {
              _context = context; 
        }

        public async Task AddContractAsync(Contract contract)
        {
            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteContractAsync(Guid id)
        {
            var idContract = await _context.Contracts.FindAsync(id);
            if (idContract != null) 
            {
             _context.Remove(idContract);
             await _context.SaveChangesAsync();

            }
        }

        public async Task<IEnumerable<Contract>> GetAllContractAsync()
        {
            return await _context.Contracts.ToListAsync();
        }

        public async Task<Contract> GetByIdContractAsync(Guid id)
        {
            return await _context.Contracts.FindAsync(id);
        }

        public async Task UpdateContractAsync(Contract contract)
        {
           _context.Contracts.Update(contract);
           await _context.SaveChangesAsync();
        }
    }
}
