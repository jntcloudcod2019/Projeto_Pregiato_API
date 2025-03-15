namespace Pregiato.API.Services.ServiceModels
{
    public class RabbitMQConfig
    {
        public string RabbitMqUri { get; set; }
        public int Port { get; set; } = 5672;
        public string QueueName { get; set; } = "sqs-inboud-sendfile";
    }
}
