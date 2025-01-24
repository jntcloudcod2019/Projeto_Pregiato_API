using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pregiato.API.Interface;
using Pregiato.API.Requests;



namespace Pregiato.API.Controllers
{
    [ApiController]
    [Route("/Auth/")]
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
        
       // [Authorize(Roles = "AdministratorPolicy, ManagerPolicy, ModelPolicy")]
        [HttpPost("login/")]
        public async Task<IActionResult> Login([FromBody] Login login)
        {
            var user = await _userRepository.GetByUsernameAsync(login.Username);
            if (user == null)
            {
                return Unauthorized("Usuário não encontrado.");
            }

            if (!_passwordHasherService.VerifyPasswordHash(login.Password, user.PasswordHash))
            {
                return Unauthorized("Senha incorreta.");
            }

            string userRole;
            switch (user.UserType)
            {
                case "Administrator":
                    userRole = "AdministratorPolicy";
                    break;
                case "Manager":
                    userRole = "ManagerPolicy";
                    break;
                case "Model":
                    userRole = "ModelPolicy";
                    break;
                default:
                    return Unauthorized("Tipo de usuário inválido.");
            }
            var token = await _userService.AuthenticateUserAsync(login);

            return Ok(new
            {
                Token = token,
                Role = userRole
            });
        }
    }

}

