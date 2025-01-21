using iText.Kernel.Pdf;
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
             _context.Add(contract); // Adiciona o contrato à tabela correspondente
            await _context.SaveChangesAsync(); // Salva as alterações no banco
        }
    }
 }
