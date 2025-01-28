using iText.Kernel.Pdf;
using Microsoft.EntityFrameworkCore;
using Pregiato.API.Interface;
using Pregiato.API.Models;
using System.Diagnostics.Contracts;

namespace Pregiato.API.Data
{
    public class ContractRepository : IContractRepository
    {
        private readonly ModelAgencyContext _context;

        public ContractRepository(ModelAgencyContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ContractsModels contract)
        {
            _context.ContractsModels.Add(contract);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ContractsModels contract)
        {
            _context.ContractsModels.Update(contract);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(ContractsModels contract)
        {
            _context.ContractsModels.Remove(contract);
            await _context.SaveChangesAsync();
        }


        public async Task<ContractsModels> GetByIdContractAsync(Guid id)
        {
            return await _context.ContractsModels.FindAsync(id);
        }

        public async Task SaveContractAsync(ContractBase contract)
        {
          
            _context.Add(contract); 
            await _context.SaveChangesAsync();
        }

        public async Task<ContractBase?> GetContractByIdAsync(int? codProposta, Guid? contractId)
        {
            return await _context.Contracts
                .FirstOrDefaultAsync(c => c.CodProposta == codProposta || c.ContractId == contractId);
        }
    }
 }
