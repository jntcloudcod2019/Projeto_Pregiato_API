using iText.Commons.Actions.Contexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pregiato.API.Data;
using Pregiato.API.Interfaces;
using Pregiato.API.Models;
using Pregiato.API.Services.ServiceModels;
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
        private readonly IDbContextFactory<ModelAgencyContext> _contextFactory;

        public ServicesAccountController(IUserService userService, IUserRepository userRepository, IJwtService jwtService,
                                         IDbContextFactory<ModelAgencyContext> contextFactory)
        {
            _userService = userService;
            _userRepository = userRepository;
            _jwtService = jwtService;
            _contextFactory = contextFactory;

        }

        [HttpPost("profile/change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userIdentity = await _userService.UserCaptureByToken();
            if (userIdentity is null)
                return Unauthorized();

            var user = await _userRepository.GetByUserIdAsync(userIdentity.UserId);
            if (user is null)
                return NotFound(new { error = "USUÁRIO NÃO ENCONTRADO." });

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword.Trim(), user.PasswordHash))
               return BadRequest(new { error = "Senha atual incorreta." });

            await _userService.UpdatePasswordAsync(user.UserId, request.NewPassword);
            return Ok(new { success = true, message = "SENHA ALTERADA COM SUCESSO." });
        }
    }
}
