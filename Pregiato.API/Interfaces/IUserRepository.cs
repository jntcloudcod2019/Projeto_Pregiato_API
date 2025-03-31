using Pregiato.API.Models;
using Pregiato.API.Requests;

namespace Pregiato.API.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllUserAsync();
        Task<User> GetByUserIdAsync(Guid id);
        Task AddUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(Guid id);
        Task<User> GetByUsernameAsync(string username);
        Task SaveChangesAsync();
        Task GetByUserAsync(LoginUserRequest loginRequest);

        Task<User> GetByProducersAsync(string Name);
    }
}
