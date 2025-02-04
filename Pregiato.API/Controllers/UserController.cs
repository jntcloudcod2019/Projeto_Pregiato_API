using Microsoft.AspNetCore.Mvc;
using Pregiato.API.Requests;
using Pregiato.API.Interface;
using Pregiato.API.Models;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;

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

        [HttpPost("register/login")]
        [Authorize(Policy = "Administrator")]
        [Authorize(Policy = "ManagerPolicy")]
        [Authorize(Policy = "ModelPolicy")]
        [SwaggerOperation(Summary = "Autentica um usuário e retorna um token JWT")]
        [SwaggerResponse(200, "Retorna o token JWT", typeof(string))]
        [SwaggerResponse(401, "Não autorizado")]
        public async Task<IActionResult> Login([FromBody]LoginUserRequest loginUserRequest)
        {
            try
            {
                var token = await _userService.AuthenticateUserAsync(loginUserRequest);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
        }

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
