using iText.Commons.Actions.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pregiato.API.Data;
using Pregiato.API.Interface;
using Pregiato.API.Models;
using Pregiato.API.Requests;
using Pregiato.API.Services;
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
        private readonly ModelAgencyContext _agencyContext;

        public ModelsController
              (IModelRepository modelRepository, 
               ModelAgencyContext agencyContext, 
               IContractService contractService,
               IJwtService jwtService,
               IUserService userService,
               IServiceUtilites serviceUtilites)
        {
            _modelRepository = modelRepository ?? throw new ArgumentNullException(nameof(modelRepository));
            _agencyContext = agencyContext ?? throw new ArgumentNullException(nameof(agencyContext));
            _contractService = contractService; 
            _jwtService = jwtService; ;
            _userService = userService; 
            _serviceUtilites = serviceUtilites; 

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
        public async Task<IActionResult> AddNewModel([FromBody] CreateModelRequest createModelRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
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

            await _userService.RegisterUserModel(createModelRequest.Name.Split(' ')[0], createModelRequest.Email);

            return Ok($"Modelo {model.Name}, criado com sucesso!");
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
        public async Task<IActionResult> UpdateRegisterModel(string query , [FromBody] UpdateModelRequest updateModelRequest)
        {
            var model = await _modelRepository.GetModelByCriteriaAsync(query);
            if (model == null)
            {
                return NotFound("Modelo não encontrado.");
            }

            var dnaJson = JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(new
            {
                physicalCharacteristics = updateModelRequest.PhysicalCharacteristics,
                appearance = updateModelRequest.Appearance,
                additionalAttributes = updateModelRequest.AdditionalAttributes
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
            var model = await _agencyContext.Model
                .FirstOrDefaultAsync(m => m.Name == username);
            if (model == null)
            {
                return Unauthorized("Usuário não encontrado na base de dados.");
            }
            var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
            if (roleClaim == null || roleClaim.Value != "Model")
            {
                return Forbid("Permissão insuficiente.");
            }
            var modelSearch = await _agencyContext.Model
                .Where(m => m.Name == username)
                .Select(m => new { m.IdModel })
                .FirstOrDefaultAsync();
            if (model == null)
            {
                throw new Exception($"Nenhum modelo encontrado para o usuário: {username}");
            }
            var contracts = await _agencyContext.Contracts
                 .Where(c => c.ModelId == modelSearch.IdModel) 
                 .Select(c => new
                 {
                     c.ModelId,
                     c.ContractFilePath,
                     c.Content 
                 })
                 .ToListAsync();
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

    }
}

