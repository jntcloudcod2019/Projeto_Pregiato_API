using Microsoft.AspNetCore.Mvc;
using Pregiato.API.Requests;
using Pregiato.API.Interface;
using Pregiato.API.Models;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;
using Pregiato.API.Response;

namespace Pregiato.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository;   

        public UserController(IUserService userService, IUserRepository userRepository)
        {
            _userService = userService;
            _userRepository = userRepository;   
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
               
                // Autentica o usuário e gera o token JWT
                var token = await _userService.AuthenticateUserAsync(loginUserRequest);

                return Ok(new LoginResponse
                {
                    Token = token,
                    User = new UserInfo
                    {
                        UserId = loginUserRequest.IdUser.ToString(),
                        Username = loginUserRequest.Username,
                        Email = loginUserRequest.Email,
                        UserType = loginUserRequest.UserType
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
                await _userService.DeleteUserAsync(id);
                return Ok(new { message = "Usuário deletado com sucesso." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [Authorize(Policy = "AdminOrManager")]
        [HttpPost("register/administrator")]
        public async Task<IActionResult> RegisterAdministrator([FromBody] UserRegisterDto dto)
        {
            try
            {
                var result = await _userService.RegisterUserAsync(dto.Username, dto.Email, dto.Password, UserType.Administrator.ToString());
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [Authorize(Policy = "AdminOrManager")]
        [HttpPost("register/model")]
        public async Task<IActionResult> RegisterModel([FromBody] UserRegisterDto dto)
        {
            try
            {
                
                var result = await _userService.RegisterUserAsync(dto.Username, dto.Email, dto.Password, UserType.Model);
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [Authorize(Policy = "AdminOrManager")]
        [HttpPost("register/manager")]
        public async Task<IActionResult> RegisterManager([FromBody] UserRegisterDto dto)
        {
            try
            {
                var result = await _userService.RegisterUserAsync(dto.Username, dto.Email, dto.Password, UserType.Manager);
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

    }
}
