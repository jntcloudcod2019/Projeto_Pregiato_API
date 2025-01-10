using Pregiato.API.Models;

namespace Pregiato.API.Interface
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
    }
}
