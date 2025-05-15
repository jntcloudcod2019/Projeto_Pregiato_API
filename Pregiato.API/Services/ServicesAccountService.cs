using Microsoft.EntityFrameworkCore;
using Pregiato.API.Data;
using Pregiato.API.Helper;
using Pregiato.API.Interfaces;
using Pregiato.API.Models;
using Pregiato.API.Services.ServiceModels;

namespace Pregiato.API.Services
{
    public class ServicesAccountService(
        IUserService userService,
        IUserRepository userRepository,
        IProcessWhatsApp processWhatsApp,
        IRabbitMQProducer rabbitmqProducer,
        IDbContextFactory<ModelAgencyContext> contextFactory,
        ModelAgencyContext modelAgencyContext) : IServicesAccount
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IUserService _userService = userService;
        private readonly IRabbitMQProducer _rabbitmqProducer = rabbitmqProducer;
        private readonly IDbContextFactory<ModelAgencyContext> _contextFactory = contextFactory;
        private readonly ModelAgencyContext _modelAgencyContext = modelAgencyContext;
        private static readonly string DefaulSQS = "sqs-send-Credentials";

        public async Task<Task> RequestResetAsync(User user)
        {
            var code = new Random().Next(100000, 999999).ToString();
            var request = new PasswordReset
            {
                WhatsApp = user.WhatsApp,
                VerificationCode = code,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            };

            await using var context = await _contextFactory.CreateDbContextAsync();
            context.PasswordReset.Add(request);
            await context.SaveChangesAsync();

            var dto = new WhatsAppMessage
            {
                UserName = user.Name,
                VerificationCode = request.VerificationCode
            };

            var validatedPhone = PhoneNumberUtils.NormalizeToE164(user.WhatsApp);

            await _rabbitmqProducer.SendMessageWhatsAppAsync(DefaulSQS, new
            {
                phone = validatedPhone,
                message = dto.SendMessageVerificationCode()
            });

            return Task.CompletedTask;
        }

        public async Task ResetPasswordAsync(string whatsApp, string newPassword)
        {
            var user = await _userRepository.GetByUsernameAsync(whatsApp);

            if (user == null)
            {
                throw new Exception("USUÁRIO NÃO ENCONTRADO");
            }

            await _userService.UpdatePasswordAsync(user.UserId, newPassword);
            await _modelAgencyContext.SaveChangesAsync();
        }

        public async Task<bool> ValidateCodeAsync(string whatsApp, string code)
        {
            ModelAgencyContext context = await _contextFactory.CreateDbContextAsync();
            _ = context.PasswordReset
                .Where(x => x.WhatsApp == whatsApp
                    && !x.Used
                    && x.VerificationCode == code
                    && x.ExpiresAt > DateTime.UtcNow)
                .FirstOrDefaultAsync();

            return context != null;
        }
    }
}
