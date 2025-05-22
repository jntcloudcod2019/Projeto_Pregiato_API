using Microsoft.EntityFrameworkCore;
using Pregiato.API.Models;
using Pregiato.API.DTO;
using Pregiato.API.Helper;
using Pregiato.API.Interfaces;
using Pregiato.API.Services.ServiceModels;

namespace Pregiato.API.Data
{
    public class ContractRepository : IContractRepository
    {
        private readonly IDbContextFactory<ModelAgencyContext> _contextFactory;

        public ContractRepository(IDbContextFactory<ModelAgencyContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }
        public async Task AddAsync(ContractsModels contract)
        {
            using ModelAgencyContext context = _contextFactory.CreateDbContext();
            context.ContractsModels.Add(contract);
            await context.SaveChangesAsync();
        }
        public async Task UpdateAsync(ContractsModels contract)
        {
            using ModelAgencyContext context = _contextFactory.CreateDbContext();
            context.ContractsModels.Update(contract);
            await context.SaveChangesAsync();
        }
        public async Task DeleteAsync(Guid idContract, int? proposalCode, Guid? IdModel)
        {
            using ModelAgencyContext context = _contextFactory.CreateDbContext();

            var contract = await GetContractByCriteriaAsync(idContract, proposalCode, IdModel);
            context.Contracts.Remove(contract);


            await context.SaveChangesAsync();
        }
        public async Task<ContractsModels> GetByIdContractAsync(Guid id)
        {
            using ModelAgencyContext context = _contextFactory.CreateDbContext();
            return await context.ContractsModels.FindAsync(id);
        }
        public async Task SaveContractAsync(ContractBase contract)
        {
            using ModelAgencyContext context = _contextFactory.CreateDbContext();
            context.Add(contract);
            await context.SaveChangesAsync();
        }
        public async Task<List<ContractBase>> GetContractsByModelId(Guid modelId)
        {
            using ModelAgencyContext context = _contextFactory.CreateDbContext();
            return await context.Contracts
                .AsNoTracking()
                .Where(c => c.IdModel == modelId)
                .ToListAsync();
        }
        public async Task<ContractBase> GetContractByCriteriaAsync(Guid idContract, int? proposalCode, Guid? IdModel)
        {
            using ModelAgencyContext context = _contextFactory.CreateDbContext();

            ContractBase? contract = await context.Contracts.FirstOrDefaultAsync
                (m => IdModel != null || m.IdModel == IdModel ||
                idContract  != null || m.ContractId == idContract ||
                proposalCode != null || m.CodProposta == proposalCode);

            if (contract == null)
            {
                throw new InvalidOperationException("Contrato não encontrado.");
            }

            return contract;
        }
        public async Task<ContractDTO?> DownloadContractAsync(int proposalCode)
        {
            using ModelAgencyContext context = _contextFactory.CreateDbContext();
            ContractDTO? contract = await context.Contracts
                .AsNoTracking()
                .Where(c => c.CodProposta == proposalCode)
                .Select(c => new ContractDTO
                {
                    ContractId = c.ContractId,
                    ModelId = c.IdModel,
                    ProposalCode = c.CodProposta,
                    ContractFilePath = c.ContractFilePath,
                    Content = c.Content
                })
                .FirstOrDefaultAsync().ConfigureAwait(true);

            return contract;
        }
       public async Task<List<ContractSummaryDTO>> GetAllContractsAsync()
        {
            using ModelAgencyContext context = _contextFactory.CreateDbContext();
            return await context.Contracts
                .AsNoTracking()
                .OrderBy(c => c.ContractId)
                .Select(c => new ContractSummaryDTO
                {
                    ContractId = c.ContractId,
                    ModelId = c.IdModel ?? Guid.Empty,
                    DataContrato = c.DataContrato,
                    VigenciaContrato = c.VigenciaContrato,
                    ValorContrato = c.ValorContrato,
                    FormaPagamento = c.FormaPagamento ?? "Não informado",
                    StatusPagamento = c.StatusPagamento,
                    ContractFilePath = c.ContractFilePath,
                    CodProposta = c.CodProposta
                })
                .ToListAsync().ConfigureAwait(true);
        }

        public async Task<List<ContractSummaryDTO>> GetAllContractsForProducersAsync(string codPrducers)
        {
            using ModelAgencyContext context = _contextFactory.CreateDbContext();
            return await context.Contracts
                .AsNoTracking()
                .Where(c => c.CodProducers == codPrducers && c.CodProposta == c.CodProposta)
                .OrderBy(c => c.ContractId)
                .Select(c => new ContractSummaryDTO
                {
                    ContractId = c.ContractId,
                    ModelId = c.IdModel ?? Guid.Empty,
                    DataContrato = c.DataContrato,
                    VigenciaContrato = c.VigenciaContrato,
                    ValorContrato = c.ValorContrato,
                    FormaPagamento = c.FormaPagamento ?? "Não informado",
                    StatusPagamento = c.StatusPagamento,
                    ContractFilePath = c.ContractFilePath,
                    CodProposta = c.CodProposta,
                    CodProduces = c.CodProducers
                })
                .ToListAsync()
                .ConfigureAwait(true);
        }

        public async Task<List<ContractBase>> ExistsContractForTodayAsync(Guid idModel)
        {
            using ModelAgencyContext context = _contextFactory.CreateDbContext();
            var contracts =  await context.Contracts
            .Where(c => c.IdModel == idModel)
            .WhereDateEquals(c => c.CreatedAt, DateTime.UtcNow.Date)
            .ToListAsync();

            return contracts;
        }
    }
}