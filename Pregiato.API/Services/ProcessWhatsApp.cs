using Pregiato.API.Helper;
using Pregiato.API.Interfaces;
using Pregiato.API.Models;
using Pregiato.API.Services.ServiceModels;

namespace Pregiato.API.Services
{
    public class ProcessWhatsApp(IRabbitMQProducer rabbitmqProducer) : IProcessWhatsApp
    {
        private static readonly string DefaulSQS = "sqs-send-Credentials";
        private readonly IRabbitMQProducer _rabbitmqProducer = rabbitmqProducer;

        public async Task<Task> ProcessWhatsAppAsync(Model model, string nikeName, string password)
        {
            var validatedPhone = PhoneNumberUtils.NormalizeToE164(model.TelefonePrincipal);
            var dto = new WhatsAppCredentialMessage
            {
                Phone = validatedPhone,
                UserName = model.Name,
                NickName = nikeName,
                Password = password
            };

            await _rabbitmqProducer.SendMessageWhatsAppAsync(DefaulSQS, new
            {
                phone = dto.Phone,
                message = dto.GetFormattedMessage()
            });

            return Task.CompletedTask;
        }
    }
}
