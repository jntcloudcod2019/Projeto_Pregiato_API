using Microsoft.EntityFrameworkCore;
using Pregiato.API.Data;
using Pregiato.API.Interfaces;

namespace Pregiato.API.Services
{
    public class ServicesAccountService : IServicesAccountService
    {
        private readonly IUserService _userService;
        private readonly IProcessWhatsApp _processWhatsApp;
        private readonly IModelRepository _modelRepository;
        private readonly IDbContextFactory<ModelAgencyContext> _contextFactory;

        public ServicesAccountService(IUserService userService, IProcessWhatsApp processWhatsApp, IDbContextFactory<ModelAgencyContext> contextFactory)
        {
            _userService = userService;
            _processWhatsApp = processWhatsApp;
            _contextFactory = contextFactory;
        }

        public async Task RequestResetAsync(string query)
        {
            throw new NotImplementedException();
        }

        public async Task ResetPasswordAsync(string whatsApp, string code, string newPassword)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> ValidateCodeAsync(string whatsApp, string code)
        {
            throw new NotImplementedException();
        }
    }
}
