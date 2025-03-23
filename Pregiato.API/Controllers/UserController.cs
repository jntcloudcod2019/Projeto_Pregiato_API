using Microsoft.AspNetCore.Mvc;
using Pregiato.API.Requests;
using Pregiato.API.Interface;
using Pregiato.API.Models;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;
using Pregiato.API.Response;
using Pregiato.API.DTO;

namespace Pregiato.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository;
        private readonly CustomResponse _customResponse;

        public UserController(IUserService userService, IUserRepository userRepository, CustomResponse customResponse)
        {
            _userService = userService;
            _userRepository = userRepository;
            _customResponse = customResponse;
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
