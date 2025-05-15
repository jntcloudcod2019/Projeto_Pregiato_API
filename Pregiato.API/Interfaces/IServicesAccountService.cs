using Pregiato.API.Models;

namespace Pregiato.API.Interfaces
{
    public interface IServicesAccount
    {
        Task<Task> RequestResetAsync(User user);
        Task<bool> ValidateCodeAsync(string whatsApp, string code);
        Task ResetPasswordAsync(string whatsApp, string newPassword);
    }
}
