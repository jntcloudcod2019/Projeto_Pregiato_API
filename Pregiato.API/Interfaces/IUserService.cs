using Pregiato.API.Requests;

namespace Pregiato.API.Interfaces
{
    public interface IUserService
    {
        Task<string> RegisterUserAsync(string username, string email);
        Task<string> RegisterUserModelAsync(string username, string email);
        Task<string> AuthenticateUserAsync(LoginUserRequest loginRequest);
        Task<string> RegisterUserProducersAsync(string username, string email);
        Task<string> RegisterUserAdministratorAsync(string username, string email);
        Task<string> RegisterUserCoordinationAsync(string username, string email);
        Task<string> RegisterManagerAsync(string username, string email);
        Task<string> RegisterTelemarketingAsync(string username, string email);
        Task<string> RegisterCEOAsync(string username, string email);
        Task<string> RegisterProductionAsync(string username, string email);
        Task<string> GenerateProducerCodeAsync();

    }
}
