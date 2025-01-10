using Microsoft.AspNetCore.Mvc;
using Pregiato.API.Interface;


namespace Pregiato.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IJwtService _jwtService;

        public AuthController(IJwtService jwtService)
        {
            _jwtService = jwtService;
        }

        [HttpPost("/Auth/login/")]
        public IActionResult Login([FromBody] Models.LoginRequest request)
        {
            // Simulação de autenticação (substituir por validação real)
            if (request.Username == "admin" && request.Password == "password")
            {
                var token = _jwtService.GenerateToken(request.Username, "Admin");
                return Ok(new { Token = token });
            }

            return Unauthorized("Invalid username or password.");
        }
    }

}

