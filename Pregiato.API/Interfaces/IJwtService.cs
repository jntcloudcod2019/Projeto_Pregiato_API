using Pregiato.API.Requests;

namespace Pregiato.API.Interfaces
{
    public interface IJwtService
    {
        Task<string> GenerateToken(LoginUserRequest? user);
        Task<bool> IsTokenValidAsync(string token);
        Task<bool> InvalidateTokenAsync(string token);
        Task<string> GetUserIdFromTokenAsync(string token);
        Task<string> GetUsernameFromTokenAsync(string token);
        Task<string> GetAuthenticatedUsernameAsync();
        Task<string> GeneratePasswordResetToken(string whatsApp);
    }
}
