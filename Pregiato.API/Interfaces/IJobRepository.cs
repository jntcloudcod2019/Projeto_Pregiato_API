using Pregiato.API.Models;

namespace Pregiato.API.Interfaces
{
    public interface IJobRepository
    {
        Task<IEnumerable<Job>> GetAllJobAsync();
        Task<Job> GetByIdJobAsync(Guid id);
        Task AddAJobsync(Job job);
        Task UpdateJobAsync(Job job);
        Task DeleteJobAsync(Guid id);
    }
}
