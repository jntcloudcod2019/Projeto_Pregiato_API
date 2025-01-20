using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Core.Types;
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

        public async Task SaveContractAsync(ContractBase contract, Stream pdfStream)
        {
            using var memoryStream = new MemoryStream();
            await pdfStream.CopyToAsync(memoryStream);
            byte[] pdfBytes = memoryStream.ToArray();

            // Salvar o PDF diretamente no banco (supondo que o ContractFile seja um campo binário)
            contract.ContractFilePath = Convert.ToBase64String(pdfBytes); // Ou use outro método de serialização
            await _context.AddAsync(contract);
            await _context.SaveChangesAsync();

        }

        public async Task<ContractsModels> GetByIdContractAsync(Guid id)
        {
            return await _context.ContractsModels.FindAsync(id);
        }
    }
 }
