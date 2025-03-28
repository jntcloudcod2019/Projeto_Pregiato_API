using System.Diagnostics.Contracts;
using Microsoft.EntityFrameworkCore;
using Pregiato.API.Interfaces;
using Pregiato.API.Models;

namespace Pregiato.API.Data
{
    public class ProducersRepository : IProducersRepository
    {
        private readonly IDbContextFactory<ModelAgencyContext> _contextFactory;
        public ProducersRepository(IDbContextFactory<ModelAgencyContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }
        public async Task SaveProducersAsync(Producers proceducers)
        {
            using var context = _contextFactory.CreateDbContext();
            context.Producers.Add(proceducers);
            await context.SaveChangesAsync();
        }

    }
}
