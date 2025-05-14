using Microsoft.EntityFrameworkCore;
using Pregiato.API.Data;
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
        ModelAgencyContext modelAgencyContext) : IServicesAccountService
    {
        private readonly IUserRepository _userRepository = userRepository;
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

            using ModelAgencyContext context = await _contextFactory.CreateDbContextAsync();
            context.PasswordReset.Add(request);
            await context.SaveChangesAsync();

            var dto = new WhatsAppMessage
            {
                UserName = user.Name,
                VerificationCode = request.VerificationCode,
                ExpiresAt = request.ExpiresAt,
            };

            await _rabbitmqProducer.SendMessageWhatsAppAsync(DefaulSQS, new
            {
                phone = user.WhatsApp,
                message = dto.SendMessageVerificationCode()
            });

            return Task.CompletedTask;
        }

        public async Task ResetPasswordAsync(string whatsApp, string code, string newPassword)
        {
            var request = await _modelAgencyContext.PasswordReset
            .Where(x => x.WhatsApp == whatsApp && x.VerificationCode == code && !x.Used && x.ExpiresAt > DateTime.UtcNow)
                .FirstOrDefaultAsync();

            if (request == null)
            {
                throw new Exception("CÓDIGO INVÁLIDO OU EXPIRADO");
            }

            var user = await _userRepository.GetByUsernameAsync(whatsApp);

            if (user == null)
            {
                throw new Exception("USUÁRIO NÃO ENCONTRADO");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            request.Used = true;

            await _modelAgencyContext.SaveChangesAsync();
        }

        public async Task<bool> ValidateCodeAsync(string whatsApp, string code)
        {
            using ModelAgencyContext context = await _contextFactory.CreateDbContextAsync();
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
