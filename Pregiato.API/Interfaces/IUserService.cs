using Pregiato.API.Enums;
using Pregiato.API.Requests;

namespace Pregiato.API.Interfaces
{
    public interface IUserService
    {
        Task<RegistrationResult> RegisterUserAsync(string username, string email);
        Task<RegistrationResult> RegisterUserModelAsync(string username, string email);
        Task<string> AuthenticateUserAsync(LoginUserRequest? loginRequest);
        Task<RegistrationResult> RegisterUserProducersAsync(string username, string email);
        Task<RegistrationResult> RegisterUserAdministratorAsync(string username, string email);
        Task<RegistrationResult> RegisterUserCoordinationAsync(string username, string email);
        Task<RegistrationResult> RegisterManagerAsync(string username, string email);
        Task<RegistrationResult> RegisterTelemarketingAsync(string username, string email);
        Task<RegistrationResult> RegisterCEOAsync(string username, string email);
        Task<RegistrationResult> RegisterProductionAsync(string username, string email);
        Task<string> GenerateProducerCodeAsync();

    }
}
