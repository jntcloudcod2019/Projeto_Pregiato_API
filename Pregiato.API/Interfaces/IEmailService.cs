namespace Pregiato.API.Interface
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(Dictionary<string, string> replacements,string toEmail, string subject);
        Task<string> LoadTemplate( Dictionary<string, string> replacements);
    }
}
