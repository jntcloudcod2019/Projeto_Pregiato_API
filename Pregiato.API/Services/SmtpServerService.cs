using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmtpServer;
using SmtpServer.Storage;
using SmtpServer.ComponentModel;
using SmtpServer.Protocol;
using SmtpServer.ComponentModel;

namespace Pregiato.API.Services
{
    public class SmtpServerService : BackgroundService
    {
        private readonly ILogger<SmtpServerService> _logger;
        private SmtpServer.SmtpServer _smtpServer;

        public SmtpServerService(ILogger<SmtpServerService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var options = new SmtpServerOptionsBuilder()
                .ServerName("PregiatoSMTPServer")
                .Port(2525) // Define a porta do servidor SMTP interno
                .Build();

            // Criando o ServiceProvider corretamente
            var serviceProvider = new SmtpServer.ComponentModel.ServiceProvider();
            serviceProvider.Add(new SampleMessageStore()); // Registra a classe que processa os e-mails

            _smtpServer = new SmtpServer.SmtpServer(options, serviceProvider);

            _logger.LogInformation("Servidor SMTP interno iniciado na porta 2525.");
            await _smtpServer.StartAsync(stoppingToken);
        }
    }
}
