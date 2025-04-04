using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pregiato.API.Data;
using Pregiato.API.Models;
using Pregiato.API.Requests;
using Pregiato.API.Response;
using Swashbuckle.AspNetCore.Annotations;
using Npgsql;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Pregiato.API.DTO;
using Pregiato.API.Interfaces;
using iText.Commons.Actions.Contexts;
using PuppeteerSharp;

namespace Pregiato.API.Controllers
{
    using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
    using Alias = string;

    [ApiController]
    [Route("api/[controller]")]
    public class ModelsController : ControllerBase
    {
        private readonly IModelRepository _modelRepository;
        private readonly IUserService _userService;
        private readonly IServiceUtilites _serviceUtilites;
        private readonly IUserRepository _userRepository;
        private readonly IProducersRepository _producersRepository;
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
               IProducersRepository producersRepository,
               IDbContextFactory<ModelAgencyContext> contextFactory)
        {
            _modelRepository = modelRepository ?? throw new ArgumentNullException(nameof(modelRepository));
            _agencyContext = agencyContext ?? throw new ArgumentNullException(nameof(agencyContext));
            ;
            _userService = userService;
            _serviceUtilites = serviceUtilites;
            _userRepository = userRepository;
            _customResponse = customResponse;
            _contextFactory = contextFactory;
            _producersRepository = producersRepository;
        }

        [Authorize(Policy = "GlobalPolitics")]
        [HttpGet("GetAllModels")]
        [SwaggerOperation("Retorna todos os modelos cadastrados.")]
        public async Task<IActionResult> GetAllModels()
        {

            try
            {
                IEnumerable<Model> modelsExists = await _modelRepository.GetAllModelAsync().ConfigureAwait(true);

                if (modelsExists == null || !modelsExists.Any())
                {
                    return Ok(new ModelsResponse
                    {
                        SUCESS = false,
                        MESSAGE = "NENHUM MODELO ENCONTRADO.",
                        DATA = null
                    });
                }

               
                var resultModels = modelsExists.Select(model => new ResulModelsResponse
                {
                    ID = model.IdModel.ToString(),
                    NAME = model.Name,
                    CPF = model.CPF,
                    RG = model.RG,
                    DATEOFBIRTH = model.DateOfBirth,
                    EMAIL = model.Email,
                    AGE = model.Age,
                    TELEFONEPRINCIPAL = model.TelefonePrincipal,
                    STATUS = model.Status ? "ATIVO" : "DESCONTINUADO", 
                    RESPONSIBLEPRODUCER = model.CodProducers,
                    ADRESSINFO = new AdressInfo 
                    {
                        ADDRESS = model.Address,
                        NUMBERADDRESS = model.NumberAddress,
                        POSTALCODE = model.PostalCode, 
                        CITY = model.City,
                        UF = model.UF
                    }
                }).ToList();

                return Ok(new ModelsResponse
                {
                    SUCESS = true,
                    DATA = resultModels
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

        [Authorize(Policy = "GlobalPolitics")]
        [HttpPost("AddModels")]
        [SwaggerOperation("Criar novo modelo.")]
        [ProducesResponseType(typeof(CustomResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CustomResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(CustomResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddNewModel([FromBody] CreateModelRequest createModelRequest)
        {
           
            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} |Validando se model {createModelRequest.Name} | Documento: {createModelRequest.CPF}");

            ModelCheckDto? checkExistence = await _modelRepository.GetModelCheck(createModelRequest)
                                                                  .ConfigureAwait(true);

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

            User producer = await _userRepository.GetByProducersAsync(createModelRequest.Nameproducers)
                                                 .ConfigureAwait(true);
          
            Model model = new Model
            {
                CodProducers = producer.CodProducers,
                Name = createModelRequest.Name,
                CPF = createModelRequest.CPF,
                RG = createModelRequest.RG,
                DateOfBirth = createModelRequest.DateOfBirth,
                Age = await _serviceUtilites
                    .CalculateAge(createModelRequest.DateOfBirth ?? DateTime.MinValue)
                    .ConfigureAwait(true),
                Email = createModelRequest.Email,
                PostalCode = createModelRequest.PostalCode,
                Address = createModelRequest.Address,
                NumberAddress = createModelRequest.NumberAddress,
                Complement = createModelRequest.Complement,
                BankAccount = createModelRequest.BankAccount,
                Neighborhood = createModelRequest.Neighborhood,
                City = createModelRequest.City,
                UF = createModelRequest.UF,
                Status = true,
                TelefonePrincipal = createModelRequest.TelefonePrincipal,
                TelefoneSecundario = createModelRequest.TelefoneSecundario

            };

            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} |  Processando cadastro do Moelo: {model.Name} | Documento:{model.CPF}. ");
            await _modelRepository.AddModelAsync(model).ConfigureAwait(true);

            Console.WriteLine($"[INFO] {DateTime.Now:yyyy-MM-dd HH:mm:ss} |  Modelo cadastro: {model.Name} | Documento:{model.CPF}. ");
            await _userService.RegisterUserModelAsync(createModelRequest.Name, createModelRequest.Email, producer.CodProducers);

            return Ok(new ModelResponse
            {
                Mensage = "CADASTRO DO MODELO CRIADO COM SUCESSO.",
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
            Model? modelExists = await _modelRepository.GetByIdModelAsync(id);
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

        [Authorize(Policy = "GlobalPolitics")]
        [HttpGet("findModel")]
        public async Task<IActionResult> FindModel([FromQuery] Alias query)
        {

            try
            {
                Model model = await _modelRepository.GetModelByCriteriaAsync(query);

                if (model == null)
                {
                    return Ok(new ModelsResponse
                    {
                        SUCESS = false,
                        MESSAGE = $"MODELO COM ID {query.ToUpper()} NÃO ENCONTRADO.",
                        DATA = null
                    });
                }

                var producer = await _producersRepository.GetProducersAsync(model.CodProducers);

                var resultModel = new ResulModelsResponse
                {
                    ID = model.IdModel.ToString(),
                    NAME = model.Name,
                    CPF = model.CPF,
                    RG = model.RG,
                    DATEOFBIRTH = model.DateOfBirth,
                    EMAIL = model.Email,
                    AGE = model.Age,
                    TELEFONEPRINCIPAL = model.TelefonePrincipal,
                    STATUS = model.Status ? "ATIVO" : "DESCONTINUADO",
                    RESPONSIBLEPRODUCER = producer.Name.ToUpper() ,
                    ADRESSINFO = string.IsNullOrEmpty(model.Address) && string.IsNullOrEmpty(model.City) && string.IsNullOrEmpty(model.UF)
                        ? null
                        : new AdressInfo
                        {
                            ADDRESS = model.Address,
                            NUMBERADDRESS = model.NumberAddress,
                            POSTALCODE = model.PostalCode,
                            CITY = model.City,
                            UF = model.UF
                        }
                };
                
                return Ok(new ModelsResponse
                {
                    SUCESS = true,
                    DATA = new List<ResulModelsResponse> { resultModel } 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Ocorreu um erro ao buscar o modelo.",
                    error = new
                    {
                        code = "INTERNAL_SERVER_ERROR",
                        details = ex.Message
                    }
                });
            }
        }
        [Authorize(Policy = "GlobalPoliticsAgency")]
        [HttpGet("my-contracts")]
        public async Task<IActionResult> GetMyContracts()
        {
            Alias authorizationHeader = HttpContext.Request.Headers["Authorization"].ToString();
            if (Alias.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            {
                return Unauthorized("Token de autenticação não fornecido ou inválido.");
            }
            Alias token = authorizationHeader.Substring("Bearer ".Length).Trim();
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
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
                .FirstOrDefaultAsync(m => m.Email == schforModel!.Email).ConfigureAwait(true);
            if (model == null)
            {
                return Unauthorized("Usuário não encontrado na base de dados.");
            }
            Claim? roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
            if (roleClaim == null || roleClaim.Value != "Model")
            {
                return Forbid("Permissão insuficiente.");
            }

            var contracts = await _agencyContext.Contracts
                .Where(c => c.IdModel == model.IdModel)
                .Select(c => new
                {
                    c.IdModel,
                    c.ContractFilePath,
                    c.Content
                })
                .ToListAsync().ConfigureAwait(true);

            {
                var listContracts = contracts.Select(c =>
                {
                    byte[] fileBytes = c.Content ?? [];
                    return new
                    {
                        ModelId = c.IdModel,
                        ContractFilePath = c.ContractFilePath,
                        ContentBase64 = fileBytes.Length > 0 ? Convert.ToBase64String(fileBytes) : null
                    };
                }).ToList();
                return Ok(listContracts);
            }
        }

        [Authorize(Policy = "GlobalPoliticsAgency")]
        [HttpPut("update-dna-property")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateDnaData([FromBody] ModelDnaData requestDNA)
        {

            if (requestDNA == null)
            {
                return BadRequest("Dados inválidos.");
            }

            var user = await _userService.UserCaptureByToken().ConfigureAwait(true);
            if (user == null)
            {
                return BadRequest("USUÁRIO NÃO ENCONTRADO.");
            }

            string jsonDna = JsonSerializer.Serialize(requestDNA, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            JsonDocument dnaDocument;
            try
            {
                dnaDocument = JsonDocument.Parse(jsonDna);
            }
            catch (JsonException ex)
            {
                return BadRequest($"Erro ao processar o JSON: {ex.Message}");
            }


            using ModelAgencyContext context = _contextFactory.CreateDbContext();

            var model = await context.Models
                .FirstOrDefaultAsync(m => m.Email == user.Email).ConfigureAwait(true);

            if (model == null)
            {
                return NotFound("Modelo não encontrado para o e-mail do usuário logado.");
            }

            model.DNA = dnaDocument;
            context.Entry(model).Property(m => m.DNA).IsModified = true;
            await context.SaveChangesAsync().ConfigureAwait(true);
            return Ok();
        }
    }
}

