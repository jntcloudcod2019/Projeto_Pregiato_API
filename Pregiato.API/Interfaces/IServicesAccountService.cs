namespace Pregiato.API.Interfaces
{
    public interface IServicesAccountService
    {
        Task RequestResetAsync(string query);
        Task<bool> ValidateCodeAsync(string whatsApp, string code);
        Task ResetPasswordAsync(string whatsApp, string code, string newPassword);
    }
}
