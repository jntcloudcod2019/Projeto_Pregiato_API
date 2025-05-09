namespace Pregiato.API.Services.ServiceModels
{
    public record ChangePasswordRequest(
      string CurrentPassword,
      string NewPassword,
      string ConfirmNewPassword
    );
}
