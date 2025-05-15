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

        public async Task<Task> ProcessWhatsAppCollaboratorAsync(User user, string userName, string password)
        {
            var validatedPhone = PhoneNumberUtils.NormalizeToE164(user.WhatsApp);

            var dto = new WhatsAppMessage
            {
                Phone = validatedPhone,
                UserName = user.Name,
                NickName = user.NickName,
                Password = password
            };

            await _rabbitmqProducer.SendMessageWhatsAppAsync(DefaulSQS, new
            {
                phone = dto.Phone,
                message = dto.GetFormattedMessage()
            });

            return Task.CompletedTask;
        }

        public async Task<Task> ProcessWhatsAppModelAsync(Model model, string nikeName, string password)
        {
            var validatedPhone = PhoneNumberUtils.NormalizeToE164(model.TelefonePrincipal);
            var dto = new WhatsAppMessage
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
