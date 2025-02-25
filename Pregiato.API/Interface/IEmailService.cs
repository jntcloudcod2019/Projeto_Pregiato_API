namespace Pregiato.API.Interface
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);

        Task<string> LoadTemplate(string templatePath, Dictionary<string, string> replacements);
    }
}
