using Microsoft.AspNetCore.Mvc;
using Pregiato.API.Requests;
using Pregiato.API.Interfaces;
using Pregiato.API.Models;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;
using Pregiato.API.Response;
using Pregiato.API.DTO;
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
        private readonly ITokenExpirationService _tokenExpirationService;

        public UserController(IJwtService jwtService, IUserService userService, IUserRepository userRepository,
            CustomResponse customResponse, ITokenExpirationService tokenExpirationService)
        {
            _userService = userService;
            _userRepository = userRepository;
            _customResponse = customResponse;
            _tokenExpirationService = tokenExpirationService; 
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
                        Message = "REQUISIÇÃO INVÁLIDA. VERIFIQUE OS DADOS ENVIADOS."
                    });
                   
                }

                var token = await _userService.AuthenticateUserAsync(loginUserRequest)
                                                    .ConfigureAwait(true);


                return Ok(new LoginResponse
                {
                    Token = token,
                    User = new UserInfo
                    {
                        UserId = loginUserRequest.IdUser?.ToString().ToUpper() ?? string.Empty,
                        Name = loginUserRequest.NickNAme.ToUpper() ?? string.Empty,
                        Email = loginUserRequest.Email.ToUpper().ToUpper() ?? string.Empty,
                        UserType = loginUserRequest.UserType.ToUpper() ?? string.Empty
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse
                {
                    Message = "ERRO INTERNO NO SERVIDOR."

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

                var expires = _tokenExpirationService.GetExpirationToken(token);

                return Ok(new
                {
                    IsValid = true,
                    UserId = userId,
                    Expires = expires
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

        [Authorize(Policy = "ManagementPolicyLevel3")]
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

        [Authorize(Policy = "ManagementPolicyLevel2")]
        [HttpPost("register/Administrator")]
        public async Task<IActionResult> RegisterAdministrator([FromBody] UserRegisterDto user)
        {

            try
            {
                if (user == null || string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Email))
                {
                    var errorResponse = ApiResponse<object>.Info("NOME DE USUÁRIO E EMAIL SÃO OBRIGATÓRIOS.");
                    return BadRequest(errorResponse);
                }

                var result = await _userService.RegisterUserAdministratorAsync(user.Username, user.Email)
                    .ConfigureAwait(true);

                if (result == RegistrationResult.UserAlreadyExists)
                {

                    var errorResponse = ApiResponse<object>.Info("USUÁRIO JÁ ESTÁ CADASTRADO.");
                    return BadRequest(errorResponse);
                }

                var successMessage = $"USUÁRIO {user.Username.ToUpper()} CADASTRADO COM SUCESSO.";
                var successResponse = ApiResponse<object>.Success(null, successMessage);
                return Ok(successResponse);
            }
            catch (Exception exception)
            {
                var errorResponse = ApiResponse<object>.Fail($"FALHA AO REGISTRAR O USUÁRIO: {exception.Message}");
                return BadRequest(errorResponse);
            }
        }

       // [Authorize(Policy = "ManagementPolicyLevel3")]
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

                var result = await _userService.RegisterUserProducersAsync(user.Username, user.Email)
                    .ConfigureAwait(true);

                if (result == RegistrationResult.UserAlreadyExists)
                {

                    var errorResponse = ApiResponse<object>.Info("USUÁRIO JÁ ESTÁ CADASTRADO.");
                    return BadRequest(errorResponse);
                }

                var successMessage = $"USUÁRIO {user.Username.ToUpper()} CADASTRADO COM SUCESSO.";
                var successResponse = ApiResponse<object>.Success(null, successMessage);
                return Ok(successResponse);
            }
            catch (Exception exception)
            {
                var errorResponse = ApiResponse<object>.Fail($"FALHA AO REGISTRAR O USUÁRIO: {exception.Message}");
                return BadRequest(errorResponse);
            }
        }

        [Authorize(Policy = "ManagementPolicyLevel3")]
        [HttpPost("register/Coordination")]
        public async Task<IActionResult> RegisterCoordination([FromBody] UserRegisterDto? user)
        {
            try
            {
                if (user == null || string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Email))
                {
                    var errorResponse = ApiResponse<object>.Info("NOME DE USUÁRIO E EMAIL SÃO OBRIGATÓRIOS.");
                    return BadRequest(errorResponse);
                }

                var result = await _userService.RegisterUserCoordinationAsync(user.Username, user.Email)
                    .ConfigureAwait(true);

                if (result == RegistrationResult.UserAlreadyExists)
                {

                    var errorResponse = ApiResponse<object>.Info("USUÁRIO JÁ ESTÁ CADASTRADO.");
                    return BadRequest(errorResponse);
                }

                var successMessage = $"USUÁRIO {user.Username.ToUpper()} CADASTRADO COM SUCESSO.";
                var successResponse = ApiResponse<object>.Success(null, successMessage);
                return Ok(successResponse);
            }
            catch (Exception exception)
            {
                var errorResponse = ApiResponse<object>.Fail($"FALHA AO REGISTRAR O USUÁRIO: {exception.Message}");
                return BadRequest(errorResponse);
            }
        }

        [Authorize(Policy = "PolicyCEO")]
        [HttpPost("register/Manager")]
        public async Task<IActionResult> RegisterManager([FromBody] UserRegisterDto? user)
        {
            try
            {
                if (user == null || string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Email))
                {
                    var errorResponse = ApiResponse<object>.Info("NOME DE USUÁRIO E EMAIL SÃO OBRIGATÓRIOS.");
                    return BadRequest(errorResponse);
                }

                var result = await _userService.RegisterManagerAsync(user.Username, user.Email).ConfigureAwait(true);

                if (result == RegistrationResult.UserAlreadyExists)
                {

                    var errorResponse = ApiResponse<object>.Info("USUÁRIO JÁ ESTÁ CADASTRADO.");
                    return BadRequest(errorResponse);
                }

                var successMessage = $"USUÁRIO {user.Username.ToUpper()} CADASTRADO COM SUCESSO.";
                var successResponse = ApiResponse<object>.Success(null, successMessage);
                return Ok(successResponse);
            }
            catch (Exception exception)
            {
                var errorResponse = ApiResponse<object>.Fail($"FALHA AO REGISTRAR O USUÁRIO: {exception.Message}");
                return BadRequest(errorResponse);
            }
        }

        [Authorize(Policy = "ManagementPolicyLevel4")]
        [HttpPost("register/Telemarketing")]
        public async Task<IActionResult> RegisterTelemarking([FromBody] UserRegisterDto? user)
        {
            try
            {
                if (user == null || string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Email))
                {
                    var errorResponse = ApiResponse<object>.Info("NOME DE USUÁRIO E EMAIL SÃO OBRIGATÓRIOS.");
                    return BadRequest(errorResponse);
                }

                var result = await _userService.RegisterTelemarketingAsync(user.Username, user.Email)
                    .ConfigureAwait(true);

                if (result == RegistrationResult.UserAlreadyExists)
                {

                    var errorResponse = ApiResponse<object>.Info("USUÁRIO JÁ ESTÁ CADASTRADO.");
                    return BadRequest(errorResponse);
                }

                var successMessage = $"USUÁRIO {user.Username.ToUpper()} CADASTRADO COM SUCESSO.";
                var successResponse = ApiResponse<object>.Success(null, successMessage);
                return Ok(successResponse);
            }
            catch (Exception exception)
            {
                var errorResponse = ApiResponse<object>.Fail($"FALHA AO REGISTRAR O USUÁRIO: {exception.Message}");
                return BadRequest(errorResponse);
            }
        }

        [Authorize(Policy = "PolicyCEO")]
        [HttpPost("register/CEO")]
        public async Task<IActionResult> RegisterCeo([FromBody] UserRegisterDto? user)
        {

            try
            {
                if (user == null || string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Email))
                {
                    var errorResponse = ApiResponse<object>.Info("NOME DE USUÁRIO E EMAIL SÃO OBRIGATÓRIOS.");
                    return BadRequest(errorResponse);
                }

                var result = await _userService.RegisterCEOAsync(user.Username, user.Email).ConfigureAwait(true);

                if (result == RegistrationResult.UserAlreadyExists)
                {

                    var errorResponse = ApiResponse<object>.Info("USUÁRIO JÁ ESTÁ CADASTRADO.");
                    return BadRequest(errorResponse);
                }

                var successMessage = $"USUÁRIO {user.Username.ToUpper()} CADASTRADO COM SUCESSO.";
                var successResponse = ApiResponse<object>.Success(null, successMessage);
                return Ok(successResponse);
            }
            catch (Exception exception)
            {
                var errorResponse = ApiResponse<object>.Fail($"FALHA AO REGISTRAR O USUÁRIO: {exception.Message}");
                return BadRequest(errorResponse);
            }
        }

        [Authorize(Policy = "ManagementPolicyLevel4")]
        [HttpPost("register/Production")]
        public async Task<IActionResult> RegisterProduction([FromBody] UserRegisterDto user)
        {
            try
            {
                if (user == null || string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Email))
                {
                    var errorResponse = ApiResponse<object>.Info("NOME DE USUÁRIO E EMAIL SÃO OBRIGATÓRIOS.");
                    return BadRequest(errorResponse);
                }

                var result = await _userService.RegisterProductionAsync(user.Username, user.Email).ConfigureAwait(true);

                if (result == RegistrationResult.UserAlreadyExists)
                {

                    var errorResponse = ApiResponse<object>.Info("USUÁRIO JÁ ESTÁ CADASTRADO.");
                    return BadRequest(errorResponse);
                }

                var successMessage = $"USUÁRIO {user.Username.ToUpper()} CADASTRADO COM SUCESSO.";
                var successResponse = ApiResponse<object>.Success(null, successMessage);
                return Ok(successResponse);
            }
            catch (Exception exception)
            {
                var errorResponse = ApiResponse<object>.Fail($"FALHA AO REGISTRAR O USUÁRIO: {exception.Message}");
                return BadRequest(errorResponse);
            }

        }

        [AllowAnonymous]
        [HttpGet("GetUsersProducers")]
        public async Task<IActionResult> GetProducers()
        {
            try
            {


                IEnumerable<User> producers = await _userRepository.GetProducers();

                if (producers == null || !producers.Any())
                {
                    return Ok(new ProducersResponse
                    {
                        SUCESS = false,
                        MESSAGE = "NENHUM PRODUTOR ENCONTRADO.",
                        DATA = null
                    });
                }

                var resulProducers = producers.Select(produc => new ResulProducersResponse
                {
                    ID = produc.UserId.ToString(),
                    NAME = produc.Name,
                    CODPRODUCER = produc.CodProducers
                }).ToList();


                return Ok(new ProducersResponse
                {
                    SUCESS = true,
                    MESSAGE = "PRODUTORES ENCONTRADOS COM SUCESSO.",
                    DATA = resulProducers
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "OCORREU UM ERRO AO BUSCAR OS PRODUTORES.",
                    error = new
                    {
                        code = "INTERNAL_SERVER_ERROR",
                        details = ex.Message
                    }
                });
            }
        }

        [Authorize("GlobalPolitics")]
        [HttpGet("Get/Users")]
        public async Task<IActionResult> GetUsers()
        {
            IEnumerable<User?> usersResult = await _userRepository.GetAllUserAsync()
                .ConfigureAwait(true);
            try
            {
                var enumerable = usersResult.ToList();
                if (!enumerable.Any())
                {
                    return Ok(new UsersResponse
                    {
                        SUCESS = false,
                        MESSAGE = "NENHUM USUÁRIO ENCONTRADO",
                        DATA = null 
                    });
                }

                var users = enumerable.Select(user => new ResultUsersResponse
                {
                    ID = user.UserId.ToString(),
                    NAME = user.Name,
                    EMAIL = user.Email,
                    POSITION = user.UserType
                }).ToList();

                return Ok(new UsersResponse
                {
                    SUCESS = true,
                    DATA = users
                });
            }
            
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "OCORREU UM ERRO AO BUSCAR OS MODELOS.",
                    error = new
                    {
                        code = "INTERNAL_SERVER_ERROR",
                        details = ex.Message
                    }
                });
            }
        }
    }
}