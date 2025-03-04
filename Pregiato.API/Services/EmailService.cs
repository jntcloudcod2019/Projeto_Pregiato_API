using MailKit.Net.Smtp;
using MimeKit;
using Pregiato.API.Interface;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using SmtpServer.ComponentModel;
using Pregiato.API.Models;
using MailKit.Security;

namespace Pregiato.API.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<SmtpSettings> smtpOptions, ILogger<EmailService> logger)
        {
            _smtpSettings = smtpOptions.Value;
            _logger = logger;
        }


        public async Task<string> LoadTemplate(Dictionary<string, string> replacements)
        {
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "WelcomeEmailTemplate.html");

            if (!File.Exists(templatePath))
            {
                _logger.LogError($"Template de e-mail não encontrado: {templatePath}");
                throw new FileNotFoundException("Template de e-mail não encontrado.", templatePath);
            }

            var templateContent = await File.ReadAllTextAsync(templatePath);
            foreach (var replacement in replacements)
            {
                templateContent = templateContent.Replace($"{{{replacement.Key}}}", replacement.Value);
            }

            return templateContent;
        }

        public async Task<bool> SendEmailAsync(Dictionary<string, string> replacements, string toEmail, string subject)
        {
            if (string.IsNullOrEmpty(toEmail))
            {
                _logger.LogError("Endereço de e-mail de destino inválido.");
                throw new ArgumentNullException(nameof(toEmail), "O e-mail de destino não pode ser nulo ou vazio.");
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Pregiato Management", _smtpSettings.Username));
            message.To.Add(new MailboxAddress(toEmail, toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = "Este é um e-mail de teste do servidor SMTP do Outlook." };
            message.Body = bodyBuilder.ToMessageBody();

            try
            {
                using var client = new SmtpClient();
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                _logger.LogInformation($"Conectando ao SMTP {_smtpSettings.Server}:{_smtpSettings.Port}...");
                await client.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, SecureSocketOptions.StartTls);

                _logger.LogInformation("Autenticando no servidor SMTP do Outlook...");
                await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);

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
