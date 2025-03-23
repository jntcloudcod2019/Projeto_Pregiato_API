using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using Pregiato.API.Interface;
using Pregiato.API.Models;
using Pregiato.API.Services.ServiceModels;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

public class RabbitMQProducer : IRabbitMQProducer
{
    private readonly RabbitMQConfig _config;
    private readonly string _rabbitMqUri;
    public RabbitMQProducer(IOptions<RabbitMQConfig> options)
    {
        _config = options.Value;
        _rabbitMqUri = _config.RabbitMqUri; 
    }
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

            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();

            var body = Encoding.UTF8.GetBytes(jsonMessage);

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: _config.QueueName,
                body: body
            );
           
            await channel.CloseAsync();
           await connection.CloseAsync();    

           
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao enviar mensagem para fila {_config.QueueName}| Error: {ex.Message}");
        }
        return ("Ok");
    }
}
