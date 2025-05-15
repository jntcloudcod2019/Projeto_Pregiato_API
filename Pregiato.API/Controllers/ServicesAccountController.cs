using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pregiato.API.Data;
using Pregiato.API.DTO;
using Pregiato.API.Interfaces;
using Pregiato.API.Models;
using Pregiato.API.Requests;
using Pregiato.API.Response;
using Pregiato.API.Services.ServiceModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Pregiato.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServicesAccountController : Controller
    {

        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly IProcessWhatsApp  _processWhatsApp;
        private readonly IDbContextFactory<ModelAgencyContext> _contextFactory;
        private readonly IServicesAccount _servicesAccount;

        public ServicesAccountController(IUserService userService, IUserRepository userRepository, IJwtService jwtService, IProcessWhatsApp processWhatsApp,
                                         IDbContextFactory<ModelAgencyContext> contextFactory, IServicesAccount servicesAccount)
        {
            _userService = userService;
            _userRepository = userRepository;
            _jwtService = jwtService;
            _contextFactory = contextFactory;
            _servicesAccount = servicesAccount;
            _processWhatsApp = processWhatsApp;

        }

        [HttpPost("profile/change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userIdentity = await _userService.UserCaptureByToken();
            if (userIdentity is null)
                return Unauthorized();

            var user = await _userRepository.GetByUsernameAsync(userIdentity.Email);

            if (user is null)
            {
                var errorResponse = ApiResponse<object>.Info("USUÁRIO NÃO ENCONTRADO.");
                return NotFound(errorResponse);
            }

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword.Trim(), user.PasswordHash))
            {
                var errorResponse = ApiResponse<object>.Info("SENHA ATUAL INCORRETA.");
                return BadRequest(errorResponse);
            }

            await _userService.UpdatePasswordAsync(user.UserId, request.NewPassword);

            var sucessResponse = ApiResponse<object>.InfoSucess("SENHA ALTERADA COM SUCESSO");
            return Ok(sucessResponse);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(string query)
        {
            User user = await _userRepository.GetByUsernameAsync(query);

            if (user == null)
            {
                var errorResponse = ApiResponse<object>.Info("USUÁRIO NÃO ENCONTRADO.");
                return BadRequest(errorResponse);
            }

            await _servicesAccount.RequestResetAsync(user);

            var sucessResponse = ApiResponse<object>.InfoSucess($"CODIGO ENVIADO PARA WHATSAPP: {user.WhatsApp} .");

            return Ok(sucessResponse);
        }


        [HttpPost("validate-reset-code")]
        public async Task<IActionResult> ValidateCode([FromBody] ValidateCodeDTO dto)
        {
            var valid = await _servicesAccount.ValidateCodeAsync(dto.WhatsApp, dto.Code);

            if (!valid)
            {
                var errorResponse = ApiResponse<object>.Info("CÓDIGO INVÁLIDO.");
                return BadRequest(errorResponse);
            }

            var token = await _jwtService.GeneratePasswordResetToken(dto.WhatsApp);

            return Ok(new LoginResponse
            {
                Token = token
            });
        }


        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordWithTokenDTO dto)
        {
            var handler = new JwtSecurityTokenHandler();

            if (!handler.CanReadToken(dto.ResetToken))
            {
                var errorResponse = ApiResponse<object>.Info("TOKEN INVÁLIDO.");
                return BadRequest(errorResponse);
            }

            var token = handler.ReadJwtToken(dto.ResetToken);
            var purpose = token.Claims.FirstOrDefault(c => c.Type == "purpose")?.Value;
            var whatsApp = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.MobilePhone)?.Value;

            if (purpose != "password-reset" || string.IsNullOrEmpty(whatsApp))
            {

                var errorResponse = ApiResponse<object>.Info("TOKEN INVÁLIDO");
                return Unauthorized(errorResponse);
            }

            await _servicesAccount.ResetPasswordAsync(whatsApp, dto.NewPassword);

            var sucessResponse = ApiResponse<object>.InfoSucess($"SENHA ATUALIZADA COM SUCESSO");
            return Ok(sucessResponse);
        }
    }
}
