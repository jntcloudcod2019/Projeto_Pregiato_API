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
            using ModelAgencyContext context = _contextFactory.CreateDbContext();
            context.Producers.Add(proceducers);
            await context.SaveChangesAsync();
        }

        public async Task<List<Producers>> GetDailyBillingByProducers(User user)
        {
            using ModelAgencyContext context = _contextFactory.CreateDbContext();
           
            DateTimeOffset startOfDay = new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero);
            DateTimeOffset endOfDay = startOfDay.AddDays(1).AddTicks(-1);

            List<Producers> producers = await context.Producers
                .AsNoTracking()
                .Where(p => p != null &&
                            p.CreatedAt >= startOfDay &&
                            p.CreatedAt <= endOfDay &&
                            (user.CodProducers != null && p.CodProducers == user.CodProducers ||
                             user.Name != null && p.NameProducer == user.Name))
                .ToListAsync();
            return producers.ToList();

        }

        public async Task<List<Producers>> GetBillingDayProducers()
        {
            using ModelAgencyContext context = _contextFactory.CreateDbContext();
            DateTimeOffset startOfDay = new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero);
            DateTimeOffset endOfDay = startOfDay.AddDays(1).AddTicks(-1);

            List<Producers> producersList = new List<Producers>();

              producersList = await context.Producers
                    .AsNoTracking()
                    .Where(p => p.CreatedAt >= startOfDay && p.CreatedAt <= endOfDay)
                    .OrderBy(p => p.NameProducer)
                    .ToListAsync();
           
            return producersList;
        }
    }
}
