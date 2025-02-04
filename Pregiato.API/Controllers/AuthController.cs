using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pregiato.API.Interface;
using Pregiato.API.Models;
using Pregiato.API.Requests;

namespace Pregiato.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IJwtService _jwtService;
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasherService _passwordHasherService;
        private readonly IUserService _userService;
        public AuthController(IJwtService jwtService, IUserRepository userRepository, IPasswordHasherService passwordHasherService, IUserService userService)
        {
            _jwtService = jwtService;
            _userRepository = userRepository;  
            _passwordHasherService = passwordHasherService;
            _userService = userService; 
        }


        [HttpPost("jwthook/custom-claims")]
        public IActionResult GetCustomClaims([FromBody] JwtHookRequest request)
        {

            if (!Request.Headers.TryGetValue("X-Supabase-Hook-Secret", out var secretHeader) ||
                   secretHeader != "v1,whsec_GmZLIdFOVOufdXBvh7oppUsUgdzEAG2dHqEeSCMIPR+QK9c0FvFxlfa/FCOV32YBazOLJG6NVw2/7dEW")
            {
                return Unauthorized("Segredo inválido.");
            }
            var customClaims = new Dictionary<string, object>
            {
                { "username", request.Email },
                { "user_type", request.UserType }, 
                { "custom_claim", "custom_value" } 
            };

            return Ok(customClaims);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserRequest loginRequest)
        {
           var user =  await _userRepository.GetByUsernameAsync(loginRequest.Username);    
            if (user ==  null)
            {
                return Unauthorized("Usuário não encontrado.");
            }
            if ((!_passwordHasherService.VerifyPasswordHash(loginRequest.Password, user.PasswordHash)))
            {
                return Unauthorized("Senha incorreta.");
            }
            loginRequest.UserType = user.UserType;

            var token = _userService.AuthenticateUserAsync(loginRequest);

            return Ok(new { Token = token });
        }
    }

}

