using Pregiato.API.Models;

namespace Pregiato.API.Interface
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
