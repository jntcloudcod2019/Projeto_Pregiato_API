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
            using ModelAgencyContext context = await _contextFactory.CreateDbContextAsync().ConfigureAwait(false);
            context.Producers.Add(proceducers);
            await context.SaveChangesAsync().ConfigureAwait(true);
        }

        public async Task<List<Producers>> GetDailyBillingByProducers(User user)
        {
            using ModelAgencyContext context = await _contextFactory.CreateDbContextAsync().ConfigureAwait(false);
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

        public async Task<User> GetProducersAsync(string codProducers)
        { 
            using ModelAgencyContext context = await _contextFactory.CreateDbContextAsync().ConfigureAwait(false);
           
            User producer = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.CodProducers == codProducers);
            return producer;
        }

        public async Task<List<Producers>> GetBillingDayProducers()
        {
            await using ModelAgencyContext context = await _contextFactory.CreateDbContextAsync().ConfigureAwait(false);
            var startOfDay = new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero);
            var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

            var producersList = new List<Producers>();
            if (producersList == null) throw new ArgumentNullException(nameof(producersList));

            producersList = await context.Producers
                    .AsNoTracking()
                    .Where(p => p.CreatedAt >= startOfDay && p.CreatedAt <= endOfDay)
                    .OrderBy(p => p.NameProducer)
                    .ToListAsync().ConfigureAwait(true);
           
            return producersList;
        }
    }
}
