using Pregiato.API.DTO;
using Pregiato.API.Enums;
using Pregiato.API.Models;
using Pregiato.API.Requests;

namespace Pregiato.API.Interfaces
{
    public interface IUserService
    {
        Task<User> UserCaptureByToken();
        Task<string> GenerateProducerCodeAsync();
        Task<RegistrationResult> RegisterUserAsync(string username, string email);
        Task<RegistrationResult> RegisterUserModelAsync(string username, string email, string codProducers, Model model);
        Task<string> AuthenticateUserAsync(LoginUserRequest? loginRequest);
        Task<RegistrationResult> RegisterUserProducersAsync(UserRegisterDto userRegisterDto);
        Task<RegistrationResult> RegisterUserAdministratorAsync(UserRegisterDto userRegisterDto);
        Task<RegistrationResult> RegisterUserCoordinationAsync(UserRegisterDto userRegisterDto);
        Task<RegistrationResult> RegisterManagerAsync(UserRegisterDto userRegisterDto);
        Task<RegistrationResult> RegisterTelemarketingAsync(UserRegisterDto userRegisterDto);
        Task<RegistrationResult> RegisterCEOAsync(UserRegisterDto userRegisterDto);
        Task<RegistrationResult> RegisterProductionAsync(UserRegisterDto userRegisterDto);
        Task<bool> UpdatePasswordAsync(Guid userId, string newPassword);
    }
}

