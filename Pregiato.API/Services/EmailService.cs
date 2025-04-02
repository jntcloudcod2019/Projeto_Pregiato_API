using MailKit.Net.Smtp;
using MimeKit;
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
           
        }

        public async Task<string> LoadTemplate(Dictionary<string, string> replacements)
        {
            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "WelcomeEmailTemplate.html");

            if (!File.Exists(templatePath))
            {
                Console.WriteLine($"[ERROR] {DateTime.Now:yyyy-MM-dd HH:mm:ss} |  Template de e-mail não encontrado: {templatePath}.");
                //Criar resiliência...
            }

            Console.WriteLine($"[ERROR] {DateTime.Now:yyyy-MM-dd HH:mm:ss} | Populando tamplate do e-mail..: {templatePath}.");

            string templateContent = await File.ReadAllTextAsync(templatePath);
            foreach (KeyValuePair<string, string> replacement in replacements)
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
                string templateContent = await LoadTemplate(replacements);

                MimeMessage message = new MimeMessage();
                message.From.Add(new MailboxAddress("Pregiato Management", "jonathanfrnnd3@gmail.com"));
                message.To.Add(new MailboxAddress(toEmail, toEmail));
                message.Subject = subject;

                BodyBuilder bodyBuilder = new BodyBuilder();

                string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "logo.png");

                if (File.Exists(imagePath))
                {
                    MimeEntity? image = bodyBuilder.LinkedResources.Add(imagePath);
                    image.ContentId = "logo";
                }
                else
                {
                    Console.WriteLine($"[ERROR] {DateTime.Now:yyyy-MM-dd HH:mm:ss} | Imagem da logo não encontrada: {imagePath}");

                }

                templateContent = templateContent.Replace("{logo}", "cid:logo");
                bodyBuilder.HtmlBody = templateContent;

                message.Body = bodyBuilder.ToMessageBody();

                using SmtpClient client = new SmtpClient();
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

