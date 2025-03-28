using MailKit.Net.Smtp;
using MimeKit;
using Pregiato.API.Interface;
using Microsoft.Extensions.Options;
using MailKit.Security;
using Pregiato.API.Interfaces;
using Pregiato.API.Services.ServiceModels;

namespace Pregiato.API.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IEnvironmentVariableProviderEmail envVarProvider, ILogger<EmailService> logger)
        {
            _logger = logger;
            ////_smtpSettings = new SmtpSettings
            ////{
            ////    Server = "smtp.gmail.com",
            ////    Port = 465,
            ////    Username = "jonathnfrnnd3@gmail.com",
            ////    Password = "bssugfuronwdrxsn",
            ////    UseTls = true
            ////};

            //if (string.IsNullOrEmpty(_smtpSettings.Server) || string.IsNullOrEmpty(_smtpSettings.Username) || string.IsNullOrEmpty(_smtpSettings.Password))
            //{
            //    Console.WriteLine($"[ERROR] {DateTime.Now:yyyy-MM-dd HH:mm:ss} |  Configurações do SMTP não foram carregadas corretamente. Verifique as variáveis de ambiente.");
            //}
        }

        public async Task<string> LoadTemplate(Dictionary<string, string> replacements)
        {
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "WelcomeEmailTemplate.html");

            if (!File.Exists(templatePath))
            {
                Console.WriteLine($"[ERROR] {DateTime.Now:yyyy-MM-dd HH:mm:ss} |  Template de e-mail não encontrado: {templatePath}.");
                //Criar resiliência...
            }

            Console.WriteLine($"[ERROR] {DateTime.Now:yyyy-MM-dd HH:mm:ss} | Populando tamplate do e-mail..: {templatePath}.");

            var templateContent = await File.ReadAllTextAsync(templatePath);
            foreach (var replacement in replacements)
            {
                templateContent = templateContent.Replace($"{{{replacement.Key}}}", replacement.Value);
            }

            return templateContent;
        }

        public async Task<bool> SendEmailAsync(Dictionary<string, string> replacements, string toEmail, string subject)
        {
            try
            {
                Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} | Populando e-mail. ");
                var templateContent = await LoadTemplate(replacements);

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Pregiato Management", "jonathanfrnnd3@gmail.com"));
                message.To.Add(new MailboxAddress(toEmail, toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder();

                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "logo.png");

                if (File.Exists(imagePath))
                {
                    var image = bodyBuilder.LinkedResources.Add(imagePath);
                    image.ContentId = "logo";
                }
                else
                {
                    Console.WriteLine($"[ERROR] {DateTime.Now:yyyy-MM-dd HH:mm:ss} | Imagem da logo não encontrada: {imagePath}");

                }

                templateContent = templateContent.Replace("{logo}", "cid:logo");
                bodyBuilder.HtmlBody = templateContent;

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                //_logger.LogInformation($"Conectando ao SMTP {_smtpSettings.Server}:{_smtpSettings.Port}...");

                await client.ConnectAsync("smtp.gmail.com", 465, MailKit.Security.SecureSocketOptions.SslOnConnect);

                _logger.LogInformation("Autenticando no servidor SMTP...");

                await client.AuthenticateAsync("jonathanfrnnd3@gmail.com", "dawq saxv alkx fqhi");

                _logger.LogInformation("Enviando e-mail...");

                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"E-mail enviado com sucesso para {toEmail}.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao enviar e-mail para {toEmail}.");
                return false;
            }
        }
    }
}

