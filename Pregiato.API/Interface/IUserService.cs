using Microsoft.AspNetCore.Identity.Data;
using Pregiato.API.Models;
using Pregiato.API.Requests;

namespace Pregiato.API.Interface
{
    public interface IUserService
    {
        Task<string> RegisterUserAsync(string username, string email, string password, string  userType);
        Task <string> AuthenticateUserAsync(Login login);
        Task DeleteUserAsync(Guid id);
    }
}
