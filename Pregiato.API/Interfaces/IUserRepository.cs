using Pregiato.API.Models;
using Pregiato.API.Requests;
using Pregiato.API.Services.ServiceModels;

namespace Pregiato.API.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<User?>> GetAllUserAsync();
        Task<User?> GetByUserIdAsync(Guid id);
        Task AddUserAsync(User? user);
        Task UpdateUserAsync(User? user);
        Task DeleteUserAsync(Guid id);
        Task<User?> GetByUsernameAsync(string username);
        Task<UserWhitResultRegister> GetByUser(string username, string email);
        Task SaveChangesAsync();
        Task GetByUserAsync(LoginUserRequest loginRequest);
        Task<IEnumerable<User>> GetProducers();
        Task<User> GetByProducersAsync(string Name);
        Task<IEnumerable<User>> GetUsersAsync();

    }
}
