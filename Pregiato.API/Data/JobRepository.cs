using Microsoft.EntityFrameworkCore;
using Pregiato.API.Interfaces;
using Pregiato.API.Models;

namespace Pregiato.API.Data
{
    public class JobRepository : IJobRepository
    {
        private readonly ModelAgencyContext _context;
        public JobRepository(ModelAgencyContext context)
        { 
            _context = context; 
        }

        public async Task AddAJobsync(Job job)
        {
            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteJobAsync(Guid id)
        {
            Job? idJob = await _context.Jobs.FindAsync(id);
            if (idJob != null) 
            {
                _context.Jobs.Remove(idJob);    
                await _context.SaveChangesAsync();  
            }
        }

        public async Task<IEnumerable<Job>> GetAllJobAsync()
        {
            return await _context.Jobs.ToListAsync();
        }

        public async Task<Job> GetByIdJobAsync(Guid id)
        {
            return await _context.Jobs.FindAsync(id);
        }

        public async Task UpdateJobAsync(Job job)
        {
            _context.Jobs.Update(job);  
            await _context.SaveChangesAsync();
        }
    }
}
