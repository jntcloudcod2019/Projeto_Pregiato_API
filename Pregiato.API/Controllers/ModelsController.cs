using iText.Commons.Actions.Contexts;
using iText.Kernel.Geom;
using iText.Layout.Element;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Pregiato.API.Data;
using Pregiato.API.Interface;
using Pregiato.API.Models;
using Pregiato.API.Requests;
using Pregiato.API.Response;
using Pregiato.API.Services;
using Swashbuckle.AspNetCore.Annotations;
using System.Data;
using Npgsql;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
namespace Pregiato.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ModelsController : ControllerBase
    {
        private readonly IModelRepository _modelRepository;
        private readonly IContractService _contractService;
        private readonly IJwtService _jwtService;
        private readonly IUserService _userService;
        private readonly IServiceUtilites _serviceUtilites;
        private readonly IUserRepository _userRepository;
        private readonly ModelAgencyContext _agencyContext;
        private readonly CustomResponse _customResponse;
        private readonly IDbContextFactory<ModelAgencyContext> _contextFactory;

        public ModelsController
              (CustomResponse customResponse,
               ModelAgencyContext agencyContext,
               IModelRepository modelRepository,
               IContractService contractService,
               IJwtService jwtService,
               IUserService userService,
               IServiceUtilites serviceUtilites,
               IUserRepository userRepository,
               IDbContextFactory<ModelAgencyContext> contextFactory)
        {
            _modelRepository = modelRepository ?? throw new ArgumentNullException(nameof(modelRepository));
            _agencyContext = agencyContext ?? throw new ArgumentNullException(nameof(agencyContext));
            _contractService = contractService;
            _jwtService = jwtService; ;
            _userService = userService;
            _serviceUtilites = serviceUtilites;
            _userRepository = userRepository;
            _customResponse = customResponse;
            _contextFactory = contextFactory;
        }

        [Authorize(Policy = "GlobalPolitic")]
        [HttpGet("GetAllModels")]
        [SwaggerOperation("Retorna todos os modelos cadastrados.")]
        public async Task<IActionResult> GetAllModels()
        {
            var modelsExists = await _modelRepository.GetAllModelAsync();
            return Ok(modelsExists);
        }

        [Authorize(Policy = "GlobalPolitic")]
        [HttpPost("AddModels")]
        [SwaggerOperation("Criar novo modelo.")]
        [ProducesResponseType(typeof(CustomResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CustomResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(CustomResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddNewModel([FromBody] CreateModelRequest createModelRequest)
        {
           
            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} |Validando se model {createModelRequest.Name} | Documento: {createModelRequest.CPF}");

            var checkExistence = await _modelRepository.GetModelCheck(createModelRequest);

            if (checkExistence != null)
            {
                Console.WriteLine($"[ERROR] {DateTime.Now:yyyy-MM-dd HH:mm:ss} | Modelo: {createModelRequest.Name} já cadastrado.. ");

                return BadRequest(new CustomResponse
                {
                    StatusCode = StatusCodes.Status304NotModified,
                    Message = $"Modelo {createModelRequest.Name} já cadastrado.",
                    Data = null,
                });
            }
                  var producer = await _userRepository.GetByProducersAsync(createModelRequest.Nameproducers); 
          
            var model = new Model
            {
                CodProducers = createModelRequest.Nameproducers ?? "PMSYSAPI01",
                Name = createModelRequest.Name,
                CPF = createModelRequest.CPF,
                RG = createModelRequest.RG,
                DateOfBirth = createModelRequest.DateOfBirth,
                Age = await _serviceUtilites.CalculateAge(createModelRequest.DateOfBirth ?? DateTime.MinValue),
                Email = createModelRequest.Email,
                PostalCode = createModelRequest.PostalCode,
                Address = createModelRequest.Address,
                NumberAddress = createModelRequest.NumberAddress,
                Complement = createModelRequest.Complement,
                BankAccount = createModelRequest.BankAccount,
                Neighborhood = createModelRequest.Neighborhood,
                City = createModelRequest.City,
                UF = createModelRequest.UF,
                TelefonePrincipal = createModelRequest.TelefonePrincipal,
                TelefoneSecundario = createModelRequest.TelefoneSecundario,
            };

            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} |  Processando cadastro do Moelo: {model.Name} | Documento:{model.CPF}. ");
            await _modelRepository.AddModelAsync(model);

            Console.WriteLine($"[INFO] {DateTime.Now:yyyy-MM-dd HH:mm:ss} |  Modelo cadastro: {model.Name} | Documento:{model.CPF}. ");
            await _userService.RegisterUserModelAsync(createModelRequest.Name, createModelRequest.Email);

            return Ok(new ModelResponse
            {
                Mensage = "Cadastro do modelo criado com sucesso.",
                Model = new ModelInfo
                {
                    Name = model.Name
                }
            });
        }

        [Authorize(Policy = "GlobalPolitic")]
        [HttpDelete("DeleteModel{id}")]
        [SwaggerOperation("Deletar cadastro de modelos.")]
        public async Task<IActionResult> DeleteModel(Guid id)
        {
            var modelExists = await _modelRepository.GetByIdModelAsync(id);
            if (modelExists == null)
            {
                return NotFound();
            }
            await _modelRepository.DeleteModelAsync(id);
            return NoContent();
        }

        //[Authorize(Policy = "AdminOrManagerOrModel")]
        //[HttpGet("modelFeedJobs")]
        //public async Task<IActionResult> GetModelFeed()
        //{
        //    var username = await _jwtService.GetAuthenticatedUsernameAsync();
        //    if (string.IsNullOrEmpty(username))
        //    {
        //        return new UnauthorizedResult();
        //    }

        //    var model = await _agencyContext.Models
        //        .FirstOrDefaultAsync(m => m.Name == username);


        //    if (model == null)
        //    {
        //        return Unauthorized("Usuário não encontrado na base de dados.");
        //    }

        //    var feed = await _agencyContext.ModelJobs
        //   .Where(mj => mj.IdModel == model.IdModel && mj.Status == "Pending")
        //   .Select(mj => new
        //   {
        //       mj.IdModel,
        //       mj.JobId,
        //       mj.JobDate,
        //       mj.Location,
        //       mj.Time,
        //       mj.AdditionalDescription,
        //       mj.Status
        //   })
        //   .ToListAsync();

        //    return Ok(new
        //    {
        //        Feed = feed
        //    });
        //}

        [Authorize(Policy = "GlobalPolitic")]
        [HttpGet("findModel")]
        public async Task<IActionResult> FindModel([FromQuery] string query)
        {
            var model = await _modelRepository.GetModelByCriteriaAsync(query);

            if (model == null)
            {
                return NotFound("Modelo não encontrado.");
            }

            return Ok(new
            {
                model.IdModel,
                model.Name,
                model.CPF,
                model.RG,
                model.Address,
                model.NumberAddress,
                model.BankAccount,
                model.PostalCode,
                model.Complement,
                model.TelefonePrincipal,
                model.TelefoneSecundario
            });
        }


        [Authorize(Policy = "GlobalPoliticsAgency")]
        [HttpGet("my-contracts")]
        public async Task<IActionResult> GetMyContracts()
        {
            var authorizationHeader = HttpContext.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            {
                return Unauthorized("Token de autenticação não fornecido ou inválido.");
            }
            var token = authorizationHeader.Substring("Bearer ".Length).Trim();
            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken;
            try
            {
                jwtToken = handler.ReadJwtToken(token);
            }
            catch (Exception)
            {
                return Unauthorized("Token inválido.");
            }
            var usernameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");
            if (usernameClaim == null)
            {
                return Unauthorized("Usuário não autenticado.");
            }
            var username = usernameClaim.Value;

            var schforModel = await _userRepository.GetByUsernameAsync(username);

            var model = await _agencyContext.Models
                .FirstOrDefaultAsync(m => m.Email == schforModel.Email);
            if (model == null)
            {
                return Unauthorized("Usuário não encontrado na base de dados.");
            }
            var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
            if (roleClaim == null || roleClaim.Value != "Model")
            {
                return Forbid("Permissão insuficiente.");
            }

            var contracts = await _agencyContext.Contracts
                 .Where(c => c.ModelId == model.IdModel)
                 .Select(c => new
                 {
                     c.ModelId,
                     c.ContractFilePath,
                     c.Content
                 })
                 .ToListAsync();

            if (contracts != null)
            {
                var listContracts = contracts.Select(c =>
                {
                    byte[] fileBytes = c.Content ?? Array.Empty<byte>();
                    return new
                    {
                        ModelId = c.ModelId,
                        ContractFilePath = c.ContractFilePath,
                        ContentBase64 = fileBytes.Length > 0 ? Convert.ToBase64String(fileBytes) : null
                    };
                }).ToList();
                return Ok(listContracts);
            }
            return BadRequest(_customResponse.Message = "Desculpe, mas não encontramos contratos relacionados ao seu usuário. ");
        }


        [Authorize(Policy = "GlobalPoliticsAgency")]
        [HttpPut("update-dna-property")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateDnaData([FromBody] UpdateDnaPropertyRequest request)
        {
            if (string.IsNullOrEmpty(request?.PropertyName) || request.Values == null || !request.Values.Any())
                return BadRequest("Nome da propriedade ou valores inválidos.");

            try
            {
                var token = HttpContext.Request.Headers["Authorization"]
                    .FirstOrDefault()?.Split("Bearer ").Last();

                if (string.IsNullOrEmpty(token))
                    return Unauthorized("Token não fornecido");

                var username = new JwtSecurityTokenHandler()
                    .ReadJwtToken(token)
                    .Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.Name)?
                    .Value;

                if (string.IsNullOrEmpty(username))
                    return Unauthorized("Token inválido");

                await using var context = await _contextFactory.CreateDbContextAsync();

                var user = await context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Name == username);

                if (user == null)
                    return Unauthorized("Usuário não encontrado");

                var model = await context.Models
                    .FirstOrDefaultAsync(m => m.Email == user.Email);

                if (model == null)
                    return Unauthorized("Modelo não encontrado");

                var propertyJson = JsonSerializer.Serialize(request.Values);

                var sql = @"
                    UPDATE ""Model"" m 
                    SET ""DNA"" = jsonb_set(m.""DNA"", ARRAY[@propertyName], @propertyJson::jsonb, true),
                        ""UpdatedAt"" = @updatedAt
                    WHERE m.""Email"" = @email"
                ;

                await context.Database.ExecuteSqlRawAsync(
                    sql,
                    new NpgsqlParameter("@propertyName", request.PropertyName),
                    new NpgsqlParameter("@propertyJson", propertyJson),
                    new NpgsqlParameter("@updatedAt", DateTime.UtcNow),
                    new NpgsqlParameter("@email", user.Email)
                 );

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }
    }
}

