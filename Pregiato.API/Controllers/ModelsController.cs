using iText.Layout.Element;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pregiato.API.Data;
using Pregiato.API.Interface;
using Pregiato.API.Models;
using Pregiato.API.Requests;
using Pregiato.API.Response;
using Swashbuckle.AspNetCore.Annotations;
using System.IdentityModel.Tokens.Jwt;
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

        public ModelsController
              (CustomResponse customResponse,
               ModelAgencyContext agencyContext,
               IModelRepository modelRepository,
               IContractService contractService,
               IJwtService jwtService,
               IUserService userService,
               IServiceUtilites serviceUtilites,
               IUserRepository userRepository)
        {
            _modelRepository = modelRepository ?? throw new ArgumentNullException(nameof(modelRepository));
            _agencyContext = agencyContext ?? throw new ArgumentNullException(nameof(agencyContext));
            _contractService = contractService; 
            _jwtService = jwtService; ;
            _userService = userService; 
            _serviceUtilites = serviceUtilites;
            _userRepository = userRepository;
            _customResponse = customResponse;

        }

        [Authorize(Policy = "AdminOrManager")]
        [HttpGet("GetAllModels")]
        [SwaggerOperation("Retorna todos os modelos cadastrados.")]
        public async Task<IActionResult> GetAllModels()
        {
            var modelsExists = await _modelRepository.GetAllModelAsync();
            return Ok(modelsExists);
        }


        [Authorize(Policy = "AdminOrManager")]
        [HttpPost("AddModels")]
        [SwaggerOperation("Criar novo modelo.")]
        [ProducesResponseType(typeof(CustomResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CustomResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(CustomResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddNewModel([FromBody] CreateModelRequest createModelRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new CustomResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Preenchimento de campos invalidos.",
                    Data = null
                });
            }

            var checkExistence = await _modelRepository.ModelExistsAsync(createModelRequest);

            if (checkExistence != null)
            {
                return BadRequest(new CustomResponse
                {
                    StatusCode = StatusCodes.Status304NotModified,
                    Message = $"Modelo {createModelRequest.Name} já cadastrado.",
                    Data = null
                }); 
            } 

            var model = new Model
            {
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
      
            await _modelRepository.AddModelAsync(model);

            await _userService.RegisterUserModel(createModelRequest.Name, createModelRequest.Email);

            return Ok(new ModelResponse
            {
                Mensage = "Cadastro do modelo criado com sucesso.",
                Model = new ModelInfo
                {
                    Name = model.Name
                }
            });
        }

        [Authorize(Policy = "AdminOrManager")]
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

        [Authorize(Policy = "AdminOrManagerOrModel")]
        [HttpGet("modelFeedJobs")]
        public async Task<IActionResult> GetModelFeed()
        {
            var username = await _jwtService.GetAuthenticatedUsernameAsync();
            if (string.IsNullOrEmpty(username))
            {
                return new UnauthorizedResult();
            }
        
            var model = await _agencyContext.Model
                .FirstOrDefaultAsync(m => m.Name == username);

            if (model == null)
            {
                return Unauthorized("Usuário não encontrado na base de dados.");
            }

            var feed = await _agencyContext.ModelJob
           .Where(mj => mj.ModelId == model.IdModel && mj.Status == "Pending")
           .Select(mj => new
           {
               mj.ModelId,
               mj.JobId,
               mj.JobDate,
               mj.Location,
               mj.Time,
               mj.AdditionalDescription,
               mj.Status
           })
           .ToListAsync();

            return Ok(new
            {
                Feed = feed
            });
        }

        [Authorize(Policy = "AdminOrManagerOrModel")]
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

        [Authorize(Policy = "AdminOrManagerOrModel")]
        [HttpGet("downloadContract/{id}")]
        public async Task<IActionResult> DownloadContract(int id)
        {
            var contract = await _agencyContext.Contracts.FindAsync(id);
            if (contract == null)
            {
                return NotFound("Contrato não encontrado.");
            }

            return File(contract.ContractFilePath, "application/pdf", $"{contract.Content}.pdf");
        }

        [Authorize(Policy = "AdminOrManagerOrModel")]
        [HttpPut("updateResgiterModel/{query}")]
        public async Task<IActionResult> UpdateRegisterModel(string query, [FromBody] UpdateModelRequest updateModelRequest)
        {
            var model = await _modelRepository.GetModelByCriteriaAsync(query);
            if (model == null)
            {
                return NotFound("Modelo não encontrado.");
            }

            var dnaJson = JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(new
            {
                appearance = updateModelRequest.Appearance,
                eyeAttributes = updateModelRequest.EyeAttributes,
                hairAttributes = updateModelRequest.HairAttributes ,
                physicalCharacteristics = updateModelRequest.PhysicalCharacteristics,
                additionalAttributes = updateModelRequest.AdditionalAttributes,


            }));

             model = new Model
             {
                Name = updateModelRequest.Name,
                CPF = updateModelRequest.CPF,
                RG = updateModelRequest.RG,
                DateOfBirth = updateModelRequest.DateOfBirth,
                Email =updateModelRequest.Email,
                PostalCode = updateModelRequest.PostalCode,
                Address = updateModelRequest.Address,
                NumberAddress =updateModelRequest.NumberAddress,
                Complement = updateModelRequest.Complement,
                BankAccount = updateModelRequest.BankAccount,
                Neighborhood = updateModelRequest.Neighborhood,
                City = updateModelRequest.City,
                TelefonePrincipal = updateModelRequest.TelefonePrincipal,
                TelefoneSecundario = updateModelRequest.TelefoneSecundario,
                DNA = dnaJson,
            };
                               
            await _modelRepository.UpdateModelAsync(model);

            return NoContent();
        }

        [Authorize(Policy = "AdminOrManagerOrModel")]
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

            var model = await _agencyContext.Model
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

            if (contracts != null )
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
            return BadRequest( _customResponse.Message = "Desculpe, mas não encontramos contratos relacionados ao seu usuário. ");
        }

    }
}

