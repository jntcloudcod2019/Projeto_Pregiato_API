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
          await  _context.SaveChangesAsync();
        }

        Task<ContractBase?> IContractRepository.GetContractByIdAsync(int? codProposta, Guid? contractId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<ContractBase>> GetContractsByModelId(Guid modelId)
        {
            return await _context.Contracts
             .Where(c => c.ModelId == modelId)
             .ToListAsync();
        }

        public  async Task<ContractBase> GetContractByCriteriaAsync(string ? contractId, string? modelId, int? codProposta)
        {
           return await _context.Contracts.FirstOrDefaultAsync( m => 
           ( modelId != null && m.ModelId.ToString() == modelId) ||
           ( contractId != null && m.ContractId.ToString() == contractId) ||
           (codProposta != null && m.CodProposta == codProposta));

        }
    }
 }
