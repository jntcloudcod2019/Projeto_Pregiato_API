using Microsoft.AspNetCore.Identity.Data;
using Pregiato.API.Requests;


namespace Pregiato.API.Interface
{
    public interface IJwtService
    {
        string GenerateToken(LoginUserRequest loginRequest);
        Task<string> GetUsernameFromTokenAsync(string token);
        Task<string> GetAuthenticatedUsernameAsync();
    }
}
