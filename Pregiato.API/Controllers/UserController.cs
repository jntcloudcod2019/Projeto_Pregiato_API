using Microsoft.AspNetCore.Mvc;
using Pregiato.API.Requests;
using Pregiato.API.Interfaces;
using Pregiato.API.Models;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;
using Pregiato.API.Response;
using Pregiato.API.DTO;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Pregiato.API.Services.ServiceModels;
using Pregiato.API.Services;
using Microsoft.IdentityModel.Tokens;
using Pregiato.API.Enums;

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
        private bool _toBoolean;

        public UserController(IJwtService jwtService, IUserService userService, IUserRepository userRepository,
            CustomResponse customResponse)
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
        public async Task<IActionResult> LoginAsync([FromBody] LoginUserRequest? loginUserRequest)
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
                        Name = loginUserRequest.NickNAme ?? string.Empty,
                        Email = loginUserRequest.Email.ToUpper() ?? string.Empty,
                        UserType = loginUserRequest.UserType ?? string.Empty
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse
                {
                    Message = "ERRO INTERNO NO SERVIDOR.",
                    Details = ex.Message.ToUpper()
                });
            }
        }

        [AllowAnonymous]
        [HttpPost("register/logout")]
        public async Task<IActionResult> LogoutAsync()
        {
            var token = HttpContext.Request.Headers.Authorization.ToString()
                .Replace("Bearer ", "");

            if (await _jwtService.InvalidateTokenAsync(token).ConfigureAwait(true))
            {
                return Ok(new { Message = "Logout realizado com sucesso" });
            }

            return BadRequest(new { Message = "Token já expirado ou inválido" });
        }

        [AllowAnonymous]
        [HttpGet("register/validate")]
        public async Task<IActionResult> ValidateTokenAsync()
        {
            try
            {
                var token = HttpContext.Request.Headers.Authorization.ToString()
                    .Replace("Bearer ", "");

                if (string.IsNullOrEmpty(token))
                {
                    return Unauthorized(new { IsValid = false, Error = "Token não fornecido" });
                }

                if (!await _jwtService.IsTokenValidAsync(token).ConfigureAwait(true))
                {
                    return Unauthorized(new { IsValid = false, Error = "Token inválido" });
                }

                var userId = await _jwtService.GetUserIdFromTokenAsync(token).ConfigureAwait(true);


                return Ok(new
                {
                    IsValid = true,
                    UserId = userId,
                    Expires = _getTokenExpiration.GetExpirationToken(token)
                });
            }
            catch (SecurityTokenException ex)
            {
                return Unauthorized(new { IsValid = false, Error = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao validar token {ex}");
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

        //[Authorize(Policy = "AdminOrManager")]
        [HttpPost("register/Administrator")]
        public async Task<IActionResult> RegisterAdministrator([FromBody] UserRegisterDto user)
        {

            try
            {
                if (user == null || string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Email))
                {
                    var errorResponse = ApiResponse<object>.Fail("NOME DE USUÁRIO E EMAIL SÃO OBRIGATÓRIOS.");
                    return BadRequest(errorResponse);
                }


                var result = await _userService.RegisterUserAdministratorAsync(user.Username, user.Email).ConfigureAwait(true);

                _toBoolean = Convert.ToBoolean(result);

                if (_toBoolean == false)
                {

                    var errorResponse = ApiResponse<object>.Fail($"FALHA AO REGISTRAR O USUÁRIO: {user.Username}");
                    return BadRequest(errorResponse);
                }

                var successMessage = $"USUÁRIO {user.Username} CADASTRADO COM SUCESSO.";
                var successResponse = ApiResponse<object>.Success(null, successMessage);
                return Ok(successResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Fail($"ERRO: {ex.Message}");
                return BadRequest(errorResponse);
            }
        }

        [Authorize(Policy = "ManagementPolicyLevel5")]
        [HttpPost("register/Model")]
        public async Task<IActionResult> RegisterModel([FromBody] UserRegisterDto user)
        {
            try
            {
                if (user == null || string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Email))
                {
                    var errorResponse = ApiResponse<object>.Info("NOME DE USUÁRIO E EMAIL SÃO OBRIGATÓRIOS.");
                    return BadRequest(errorResponse);
                }

                var result = await _userService.RegisterUserModelAsync(user.Username, user.Email).ConfigureAwait(true);

                if (result == RegistrationResult.UserAlreadyExists)
                {

                    var errorResponse = ApiResponse<object>.Info("USUÁRIO JÁ ESTÁ CADASTRADO.");
                    return BadRequest(errorResponse);
                }

                var successMessage = $"USUÁRIO {user.Username} CADASTRADO COM SUCESSO.";
                var successResponse = ApiResponse<object>.Success(null, successMessage);
                return Ok(successResponse); 
            }
            catch (Exception exception)
            {
                var errorResponse = ApiResponse<object>.Fail($"FALHA AO REGISTRAR O USUÁRIO: {exception.Message}");
                return BadRequest(errorResponse);
            }
        }
        

      //[Authorize(Policy = "AdminOrManager")]
        [HttpPost("register/producers")]
        public async Task<IActionResult> RegisterUserProducers([FromBody] UserRegisterDto? user)
        {
            try
            {
                if (user == null || string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Email))
                {
                    var errorResponse = ApiResponse<object>.Info("NOME DE USUÁRIO E EMAIL SÃO OBRIGATÓRIOS.");
                    return BadRequest(errorResponse);
                }

                var result = await _userService.RegisterUserProducersAsync(user.Username, user.Email).ConfigureAwait(true);

                if (result == RegistrationResult.UserAlreadyExists)
                {

                    var errorResponse = ApiResponse<object>.Info("USUÁRIO JÁ ESTÁ CADASTRADO.");
                    return BadRequest(errorResponse);
                }

                var successMessage = $"USUÁRIO {user.Username} CADASTRADO COM SUCESSO.";
                var successResponse = ApiResponse<object>.Success(null, successMessage);
                return Ok(successResponse);
            }
            catch (Exception exception)
            {
                var errorResponse = ApiResponse<object>.Fail($"FALHA AO REGISTRAR O USUÁRIO: {exception.Message}");
                return BadRequest(errorResponse);
            }
        }


        [Authorize(Policy = "AdminOrManager")]
        [HttpPost("register/Coordination")]
        public async Task<IActionResult> RegisterCoordination([FromBody] UserRegisterDto? user)
        {
            try
            {
                if (user == null || string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Email))
                {
                    var errorResponse = ApiResponse<object>.Fail("NOME DE USUÁRIO E EMAIL SÃO OBRIGATÓRIOS.");
                    return BadRequest(errorResponse);
                }

                var result = await _userService.RegisterUserCoordinationAsync(user.Username, user.Email)
                    .ConfigureAwait(true);
               
                _toBoolean = Convert.ToBoolean(result);

                if (_toBoolean == false)
                {

                    var errorResponse = ApiResponse<object>.Fail($"FALHA AO REGISTRAR O USUÁRIO: {user.Username}");
                    return BadRequest(errorResponse);
                }

                var successMessage = $"USUÁRIO {user.Username} CADASTRADO COM SUCESSO.";
                var successResponse = ApiResponse<object>.Success(null, successMessage);
                return Ok(successResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Fail($"ERRO: {ex.Message}");
                return BadRequest(errorResponse);
            }
        }

        [Authorize(Policy = "AdminOrManager")]
        [HttpPost("register/Manager")]
        public async Task<IActionResult> RegisterManager([FromBody] UserRegisterDto? user)
        {
            try
            {
                if (user == null || string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Email))
                {
                    var errorResponse = ApiResponse<object>.Fail("NOME DE USUÁRIO E EMAIL SÃO OBRIGATÓRIOS.");
                    return BadRequest(errorResponse);
                }

                var result = await _userService.RegisterManagerAsync(user.Username, user.Email).ConfigureAwait(true);

                _toBoolean = Convert.ToBoolean(result);

                if (_toBoolean == false)
                {

                    var errorResponse = ApiResponse<object>.Fail($"FALHA AO REGISTRAR O USUÁRIO: {user.Username}");
                    return BadRequest(errorResponse);
                }

                var successMessage = $"USUÁRIO {user.Username} CADASTRADO COM SUCESSO.";
                var successResponse = ApiResponse<object>.Success(null, successMessage);
                return Ok(successResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Fail($"ERRO: {ex.Message}");
                return BadRequest(errorResponse);
            }
        }


        [Authorize(Policy = "AdminOrManager")]
        [HttpPost("register/Telemarketing")]
        public async Task<IActionResult> RegisterTelemarking([FromBody] UserRegisterDto? user)
        {
            try
            {
                if (user == null || string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Email))
                {
                    var errorResponse = ApiResponse<object>.Fail("NOME DE USUÁRIO E EMAIL SÃO OBRIGATÓRIOS.");
                    return BadRequest(errorResponse);
                }

                var result = await _userService.RegisterTelemarketingAsync(user.Username, user.Email)
                    .ConfigureAwait(true);

                _toBoolean = Convert.ToBoolean(result);
                if (_toBoolean != false)
                {

                    var errorResponse = ApiResponse<object>.Fail($"FALHA AO REGISTRAR O USUÁRIO: {user.Username}");
                    return BadRequest(errorResponse);
                }

                var successMessage = $"USUÁRIO {user.Username} CADASTRADO COM SUCESSO.";
                var successResponse = ApiResponse<object>.Success(null, successMessage);
                return Ok(successResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Fail($"ERRO: {ex.Message}");
                return BadRequest(errorResponse);
            }
        }

        [Authorize(Policy = "AdminOrManager")]
        [HttpPost("register/CEO")]
        public async Task<IActionResult> RegisterCeo([FromBody] UserRegisterDto? user)
        {

            try
            {
                if (user == null || string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Email))
                {
                    var errorResponse = ApiResponse<object>.Fail("NOME DE USUÁRIO E EMAIL SÃO OBRIGATÓRIOS.");
                    return BadRequest(errorResponse);
                }

                var result = await _userService.RegisterCEOAsync(user.Username, user.Email).ConfigureAwait(true);

                _toBoolean = Convert.ToBoolean(result);
                if (_toBoolean == false)
                {

                    var errorResponse = ApiResponse<object>.Fail($"FALHA AO REGISTRAR O USUÁRIO: {user.Username}");
                    return BadRequest(errorResponse);
                }

                var successMessage = $"USUÁRIO {user.Username} CADASTRADO COM SUCESSO.";
                var successResponse = ApiResponse<object>.Success(null, successMessage);
                return Ok(successResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Fail($"ERRO: {ex.Message}");
                return BadRequest(errorResponse);
            }
        }

        [Authorize(Policy = "AdminOrManager")]
        [HttpPost("register/Production")]
        public async Task<IActionResult> RegisterProduction([FromBody] UserRegisterDto user)
        {
            try
            {
                if (user == null || string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Email))
                {
                    var errorResponse = ApiResponse<object>.Fail("NOME DE USUÁRIO E EMAIL SÃO OBRIGATÓRIOS.");
                    return BadRequest(errorResponse);
                }

                var result = await _userService.RegisterProductionAsync(user.Username, user.Email)
                    .ConfigureAwait(true);
              
                _toBoolean = Convert.ToBoolean(result);
                if (_toBoolean == false)
                {

                    var errorResponse = ApiResponse<object>.Fail($"FALHA AO REGISTRAR O USUÁRIO: {user.Username}");
                    return BadRequest(errorResponse);
                }

                var successMessage = $"USUÁRIO {user.Username} CADASTRADO COM SUCESSO.";
                var successResponse = ApiResponse<object>.Success(null, successMessage);
                return Ok(successResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Fail($"ERRO: {ex.Message}");
                return BadRequest(errorResponse);
            }

        }

    }
}
