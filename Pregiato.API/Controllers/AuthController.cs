using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pregiato.API.Interface;
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
        
        [Authorize(Roles = "AdministratorPolicy, ManagerPolicy, ModelPolicy")]
        [HttpPost("/Auth/login/")]
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

            var token = _userService.AuthenticateUserAsync(loginRequest);

            return Ok(new { Token = token });
        }
    }

}

