using MailKit.Net.Smtp;
using MimeKit;
using Pregiato.API.Interface;
using Pregiato.API.Models;
using Pregiato.API.Interface;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Projeto_Pregiato_API.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;

        public EmailService(IOptions<SmtpSettings> smtpOptions)
        {
            _smtpSettings = smtpOptions.Value;
        }

        public async Task<string> LoadTemplate(string templatePath, Dictionary<string, string> replacements)
        {
            var templateContent = File.ReadAllText(templatePath);

            foreach (var replacement in replacements)
            {
                templateContent = templateContent.Replace($"{{{replacement.Key}}}", replacement.Value);
            }

            return templateContent;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            if (string.IsNullOrEmpty(toEmail))
            {
                throw new ArgumentNullException(nameof(toEmail), "O endereço de e-mail de destino não pode ser nulo ou vazio.");
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Agência Pregiato Management", "noreply@pregiato.local"));  
            message.To.Add(new MailboxAddress(toEmail, toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = body
            };
            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                // Conectando-se ao servidor SMTP interno
                await client.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, _smtpSettings.UseSsl);

                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
    }
}