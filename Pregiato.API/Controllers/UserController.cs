using Microsoft.AspNetCore.Mvc;
using Pregiato.API.Requests;
using Pregiato.API.Interface;
using Pregiato.API.Models;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;
using Pregiato.API.Response;
using Pregiato.API.DTO;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Pregiato.API.Services.ServiceModels;
using Pregiato.API.Services;
using Microsoft.IdentityModel.Tokens;

namespace Pregiato.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly CustomResponse _customResponse;
        private readonly GetTokenExpiration _getTokenExpiration; 
        public UserController(IJwtService jwtService, IUserService userService, IUserRepository userRepository, CustomResponse customResponse)
        {
            _userService = userService;
            _userRepository = userRepository;
            _customResponse = customResponse;
            _jwtService = jwtService;
        }

        [AllowAnonymous]
        [HttpPost("register/login")]
        [SwaggerOperation(Summary = "Autentica um usuário e retorna um token JWT")]
        [SwaggerResponse(200, "Retorna o token JWT", typeof(string))]
        [SwaggerResponse(400, "Requisição inválida")]
        [SwaggerResponse(401, "Não autorizado")]
        public async Task<IActionResult> Login([FromBody]LoginUserRequest loginUserRequest)
        {
            try
            {
                if (loginUserRequest == null)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Requisição inválida. Verifique os dados enviados."
                    });
                }

                var token = await _userService.AuthenticateUserAsync(loginUserRequest);

                return Ok(new LoginResponse
                {
                    Token = token,
                    User = new UserInfo
                    {
                        UserId = loginUserRequest.IdUser?.ToString() ?? string.Empty,
                        Username = loginUserRequest.Username ?? string.Empty,
                        Email = loginUserRequest.Email ?? string.Empty,
                        UserType = loginUserRequest.UserType ?? string.Empty
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Erro interno no servidor.",
                    Details = ex.Message
                });
            }
        }

        [AllowAnonymous]
        [HttpPost("register/logout")]
        public async Task<IActionResult> Logout()
        {
            var token = HttpContext.Request.Headers["Authorization"]
                .ToString()
                .Replace("Bearer ", "");

            if (await _jwtService.InvalidateTokenAsync(token))
            {
                return Ok(new { Message = "Logout realizado com sucesso" });
            }

            return BadRequest(new { Message = "Token já expirado ou inválido" });
        }

        [AllowAnonymous]
        [HttpGet("register/validate")]
        public async Task<IActionResult> ValidateToken()
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"]
                    .ToString()
                    .Replace("Bearer ", "");

                if (string.IsNullOrEmpty(token))
                {
                    return Unauthorized(new { IsValid = false, Error = "Token não fornecido" });
                }

                if (!await _jwtService.IsTokenValidAsync(token))
                {
                    return Unauthorized(new { IsValid = false, Error = "Token inválido" });
                }

                var userId = await _jwtService.GetUserIdFromTokenAsync(token);


                return Ok(new
                {
                    IsValid = true,
                    UserId = userId,
                    Expires =  _getTokenExpiration.GetExpirationToken(token)
                });
            }
            catch (SecurityTokenException ex)
            {
                return Unauthorized(new { IsValid = false, Error = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine( $"Erro ao validar token {ex}");
                return StatusCode(500, new { Error = "Erro interno ao validar token" });
            }
        }

        [Authorize(Policy = "AdminOrManager")]
        [HttpDelete("deleteUser{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _userRepository.DeleteUserAsync(id);
                return Ok(new { message = "Usuário deletado com sucesso." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [Authorize(Policy = "AdminOrManager")]
        [HttpPost("register/administrator")]
        public async Task<IActionResult> RegisterAdministrator([FromBody] UserRegisterDto user)
        {

            try
            {
                if (user == null || string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Email))
                {
                    return BadRequest(new { error = "Nome de usuário e email são obrigatórios." });
                }

                var result = await _userService.RegisterUserAsync(user.Username, user.Email);

                return Ok(new { message = "Usuário ADM cadastrado com sucesso." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [Authorize(Policy = "AdminOrManager")]
        [HttpPost("register/model")]
        public async Task<IActionResult> RegisterModel([FromBody] UserRegisterDto user)
        {
            try
            {
                if (user == null || string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Email))
                {
                    return BadRequest(new { error = "Nome de usuário e email são obrigatórios." });
                }

                var result = await _userService.RegisterUserAsync(user.Username, user.Email);

                return Ok(new { message = "Usuário ADM cadastrado com sucesso." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }       
        }

        [Authorize(Policy = "AdminOrManager")]
        [HttpPost("register/manager")]
        public async Task<IActionResult> RegisterManager([FromBody] UserRegisterDto user)
        {
            try
            {
                if (user == null || string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Email))
                {
                    return BadRequest(new { error = "Nome de usuário e email são obrigatórios." });
                }

                var result = await _userService.RegisterUserAsync(user.Username, user.Email);              
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }        
            return Ok(_customResponse.Message = $"Usuario {user} cadastrado com sucessp.");
        }

    }
    
}
