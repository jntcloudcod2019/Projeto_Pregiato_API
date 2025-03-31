using Microsoft.Extensions.Options;
using Pregiato.API.Interfaces;
using Pregiato.API.Models;
using Pregiato.API.Services.ServiceModels;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

public class RabbitMQProducer(IOptions<RabbitMQConfig> options) : IRabbitMQProducer
{
    private readonly RabbitMQConfig _config = options.Value;

    public async Task<string> SendMensage(List<ContractBase> contracts, string modelDocument)
    {
        try
        {
            var contractIds = contracts.Select(c => c.ContractId).ToList();

            var contractMessage = new ContractMessage
            {
                ContractIds = contractIds,
                CpfModel = modelDocument,
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            };

            var factory = new ConnectionFactory()
            {
                HostName = "mouse.rmq5.cloudamqp.com", 
                VirtualHost = "ewxcrhtv",
                UserName = "ewxcrhtv",
                Password = "DNcdH0NEeP4Fsgo2_w-vd47CqjelFk_S", 
                Port = 5672, 
                AutomaticRecoveryEnabled = true, 
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10) 
            };

            var jsonMessage = JsonSerializer.Serialize(contractMessage, new JsonSerializerOptions { WriteIndented = true });
            var connection = await factory.CreateConnectionAsync().ConfigureAwait(true);
            var channel = await connection.CreateChannelAsync().ConfigureAwait(true);

            var body = Encoding.UTF8.GetBytes(jsonMessage);

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: _config.QueueName,
                body: body
            ).ConfigureAwait(true);

            await channel.CloseAsync().ContinueWith(t =>
            {
 
                if (t.IsCompletedSuccessfully)
                {
                    Console.WriteLine("Canal fechado com sucesso.");
                }
                else
                {
                    Console.WriteLine($"Falha ao fechar o canal: {t.Exception?.Message}");
                }
            });
            await connection.CloseAsync().ConfigureAwait(true);    
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao enviar mensagem para fila {_config.QueueName}| Error: {ex.Message}");
        }
        return ("Ok");
    }
}
