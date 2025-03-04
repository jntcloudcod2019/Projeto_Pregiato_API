using MailKit.Net.Smtp;
using MimeKit;
using Pregiato.API.Interface;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
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

        public async Task<string> LoadTemplate(string templatePath, Dictionary<string, string> replacements)
        {
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

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
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

            var bodyBuilder = new BodyBuilder { HtmlBody = body };
            message.Body = bodyBuilder.ToMessageBody();

            int attempt = 0;

            while (attempt < _smtpSettings.MaxRetryAttempts)
            {
                try
                {
                    using var client = new SmtpClient();

                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                    _logger.LogInformation($"Tentativa {attempt + 1}: Conectando ao SMTP {_smtpSettings.Server}:{_smtpSettings.Port}");

                    await client.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, SecureSocketOptions.StartTls);
                    if (_smtpSettings.UseTls)
                    {
                        client.SslProtocols = System.Security.Authentication.SslProtocols.Tls;
                    }
                    await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);

                    _logger.LogInformation($"E-mail enviado com sucesso para {toEmail}");
                    return true;
                }
                catch (Exception ex)
                {
                    attempt++;
                    _logger.LogError(ex, $"Erro ao enviar e-mail para {toEmail}. Tentativa {attempt}/{_smtpSettings.MaxRetryAttempts}");

                    if (attempt >= _smtpSettings.MaxRetryAttempts)
                    {
                        _logger.LogError($"Falha ao enviar e-mail após {attempt} tentativas.");
                        return false;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(_smtpSettings.RetryDelaySeconds));
                }
            }

            return false;
        }
    }
}
