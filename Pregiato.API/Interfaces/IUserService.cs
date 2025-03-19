using Microsoft.AspNetCore.Identity.Data;
using Pregiato.API.Models;
using Pregiato.API.Requests;
using Pregiato.API.Response;

namespace Pregiato.API.Interface
{
    public interface IUserService
    {
        Task<string> RegisterUserAsync(string username, string  email);
        Task<string> RegisterUserModel(string username, string email);
        Task <string> AuthenticateUserAsync(LoginUserRequest loginRequest);
        Task DeleteUserAsync(Guid id);
    }
}
