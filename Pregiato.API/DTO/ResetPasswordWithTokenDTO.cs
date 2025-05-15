namespace Pregiato.API.DTO
{
    public class ResetPasswordWithTokenDTO
    {
        public string ResetToken { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }
}
