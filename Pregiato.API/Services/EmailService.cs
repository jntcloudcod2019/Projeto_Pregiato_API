﻿using MailKit.Net.Smtp;
using MimeKit;
using Pregiato.API.Interface;
using Microsoft.Extensions.Options;
using Pregiato.API.Models;
using MailKit.Security;
using Pregiato.API.Interfaces;

namespace Pregiato.API.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IEnvironmentVariableProviderEmail envVarProvider, ILogger<EmailService> logger)
        {
            _logger = logger;
            _smtpSettings = new SmtpSettings
            {
                Server = envVarProvider.GetVariable("SERVER_EMAIL"),
                Port = int.TryParse(envVarProvider.GetVariable("SERVER_EMAIL_PORT"), out var port) ? port : 587,
                Username = envVarProvider.GetVariable("SERVER_EMAIL_USERNAME"),
                Password = envVarProvider.GetVariable("SERVER_EMAIL_PASSWORD"),
                UseTls = true
            };
            
            if (string.IsNullOrEmpty(_smtpSettings.Server) || string.IsNullOrEmpty(_smtpSettings.Username) || string.IsNullOrEmpty(_smtpSettings.Password))
            {
                throw new InvalidOperationException("Configurações do SMTP não foram carregadas corretamente. Verifique as variáveis de ambiente.");
            }
            _logger.LogInformation("Configurações do SMTP carregadas com sucesso.");
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
                _logger.LogInformation($"Populando tamplate do e-mail P:{replacement.Key}  V:{replacement.Value}");
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

            try
            {
                var templateContent = await LoadTemplate(replacements);

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Pregiato Management", _smtpSettings.Username));
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
                    _logger.LogWarning($"Imagem da logo não encontrada: {imagePath}");
                }

                templateContent = templateContent.Replace("{logo}", "cid:logo");
                bodyBuilder.HtmlBody = templateContent;

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                _logger.LogInformation($"Conectando ao SMTP {_smtpSettings.Server}:{_smtpSettings.Port}...");

                await client.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, SecureSocketOptions.StartTls);

                _logger.LogInformation("Autenticando no servidor SMTP...");

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

