using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pregiato.API.Enums;
using Pregiato.API.Interfaces;
using Pregiato.API.Models;
using Pregiato.API.Services.ServiceModels;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

public class RabbitMQProducer(IOptions<RabbitMQConfig> options) : IRabbitMQProducer
{
    private readonly RabbitMQConfig _config = options.Value;
    private static readonly string SQSDeleteContract = "sqs-delete-document";

    public async Task<RegistrationResult> SendMessageDeleteContractAsync(DocumentsAutentique documentsAutentique, object message)
    {
        var factory = new ConnectionFactory
        {
            HostName = "mouse.rmq5.cloudamqp.com",
            VirtualHost = "ewxcrhtv",
            UserName = "ewxcrhtv",
            Password = "DNcdH0NEeP4Fsgo2_w-vd47CqjelFk_S",
            Port = 5672
        };

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(queue: SQSDeleteContract,
                             durable: true,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

        await channel.BasicPublishAsync(
                             exchange: string.Empty,
                             routingKey: SQSDeleteContract,
                             body: body);

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

        await connection.CloseAsync();

        return RegistrationResult.Success;

    }

    public async Task<string> SendMensage(List<ContractBase> contracts, string modelDocument)
    {
        try
        {
            var contractIds = contracts.Select(c => c.ContractId).ToList();
            var contractMessage = new ContractMessage
            {
                Action = "CREATE",
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
            await connection.CloseAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao enviar mensagem para fila {_config.QueueName}| Error: {ex.Message}");
        }
        return "Ok";
    }

    public async Task<Task> SendMessageWhatsAppAsync(string queueName, object message)
    {
        var factory = new ConnectionFactory
        {
            HostName = "mouse.rmq5.cloudamqp.com",
            VirtualHost = "ewxcrhtv",
            UserName = "ewxcrhtv",
            Password = "DNcdH0NEeP4Fsgo2_w-vd47CqjelFk_S",
            Port = 5672
        };

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(queue: queueName,
                             durable: true,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

        await channel.BasicPublishAsync(
                             exchange: string.Empty,
                             routingKey: queueName,
                             body: body);

        return Task.CompletedTask;
    }
}
