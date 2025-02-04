using iText.Commons.Actions.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pregiato.API.Data;
using Pregiato.API.Interface;
using Pregiato.API.Models;
using Pregiato.API.Requests;
using Pregiato.API.Services;
using Swashbuckle.AspNetCore.Annotations;
using System.Diagnostics.Contracts;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
namespace Pregiato.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ModdelsController : ControllerBase
    {
        private readonly IModelRepository _modelRepository;
        private readonly IContractService _contractService;
        private readonly IJwtService _jwtService;   
        private readonly ModelAgencyContext _agencyContext;

        public ModdelsController(IModelRepository modelRepository, ModelAgencyContext agencyContext, IContractService contractService, IJwtService jwtService)
        {
            _modelRepository = modelRepository ?? throw new ArgumentNullException(nameof(modelRepository));
            _agencyContext = agencyContext ?? throw new ArgumentNullException(nameof(agencyContext));
            _contractService = contractService; 
            _jwtService = jwtService;
        }

        [HttpGet("/GetAllModels")]
        [SwaggerOperation("Retorna todos os modelos cadastrados.")]
        public async Task<IActionResult> GetAllModels()
        {
            var modelsExists = await _modelRepository.GetAllModelAsync();
            return Ok(modelsExists);
        }

        [HttpPost("/AddModels")]
        [SwaggerOperation("Criar novo modelo.")]
        public async Task<IActionResult> AddNewModel([FromBody] CreateModelRequest createModelRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var dnaJson = JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(new
            {
                physicalCharacteristics = createModelRequest.PhysicalCharacteristics,
                appearance = createModelRequest.Appearance,
                additionalAttributes = createModelRequest.AdditionalAttributes
            }));

            var model = new Model
            {
                Name = createModelRequest.Name,
                CPF = createModelRequest.CPF,
                RG = createModelRequest.RG,
                Email = createModelRequest.Email,
                PostalCode = createModelRequest.PostalCode,
                Address = createModelRequest.Address,
                NumberAddress = createModelRequest.NumberAddress,   
                Complement = createModelRequest.Complement,
                BankAccount = createModelRequest.BankAccount,
                PasswordHash = createModelRequest.PasswordHash,
                Neighborhood = createModelRequest.Neighborhood,
                City = createModelRequest.City,
                DNA = dnaJson,
                TelefonePrincipal = createModelRequest.TelefonePrincipal,   
                TelefoneSecundario = createModelRequest.TelefoneSecundario,

            };

            await _modelRepository.AddModelAsync(model);
            return Ok($"Modelo {model.Name}, criado com sucesso!");
        }

        [HttpPut("/UpdateModels{id}")]
        [SwaggerOperation("Atualização de cadastro de modelos.")]
        public async Task<IActionResult> UpdateClient(Guid id, [FromBody] UpdateModelRequest updateModelRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var modelExists = await _modelRepository.GetByIdModelAsync(id);

            if (modelExists == null)
            {
                return NotFound();
            }

            switch (updateModelRequest)
            {
                case { Name: not null }:
                    modelExists.Name = updateModelRequest.Name;
                    break;

                case { CPF: not null }:
                    modelExists.CPF = updateModelRequest.CPF;
                    break;

                case { RG: not null }:
                    modelExists.RG = updateModelRequest.RG;
                    break;

                case { Email: not null }:
                    modelExists.Email = updateModelRequest.Email;
                    break;

                case { PostalCode: not null }:
                    modelExists.PostalCode = updateModelRequest.PostalCode;
                    break;

                case { Address: not null }:
                    modelExists.Address = updateModelRequest.Address;
                    break;

                case { BankAccount: not null }:
                    modelExists.BankAccount = updateModelRequest.BankAccount;
                    break;

                default:

                    break;

            }

            await _modelRepository.UpdateModelAsync(modelExists);
            return NoContent();
        }

        [HttpDelete("/DeleteModel{id}")]
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

        [HttpGet("/model-feed")]
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

        [HttpGet("find-model")]
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

        [HttpGet("my-contracts")]
        public async Task<IActionResult> GetMyContracts(string type = "files")
        {
            return await _contractService.GetMyContracts(type);
        }


        [HttpGet("download-contract/{id}")]
        public async Task<IActionResult> DownloadContract(int id)
        {
            var contract = await _agencyContext.Contracts.FindAsync(id);
            if (contract == null)
            {
                return NotFound("Contrato não encontrado.");
            }

            return File(contract.ContractFilePath, "application/pdf", $"{contract.Content}.pdf");
        }

        [HttpPut("updateModelDNA/{query}")]
        public async Task<IActionResult> UpdateRegisterModel(string query , [FromBody] CreateModelRequest createModelRequest)
        {
            var model = await _modelRepository.GetModelByCriteriaAsync(query);
            if (model == null)
            {
                return NotFound("Modelo não encontrado.");
            }

            var dnaJson = JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(new
            {
                physicalCharacteristics = createModelRequest.PhysicalCharacteristics,
                appearance = createModelRequest.Appearance,
                additionalAttributes = createModelRequest.AdditionalAttributes
            }));

             model = new Model
            {
                Name = createModelRequest.Name,
                CPF = createModelRequest.CPF,
                RG = createModelRequest.RG,
                Email = createModelRequest.Email,
                PostalCode = createModelRequest.PostalCode,
                Address = createModelRequest.Address,
                NumberAddress = createModelRequest.NumberAddress,
                Complement = createModelRequest.Complement,
                BankAccount = createModelRequest.BankAccount,
                PasswordHash = createModelRequest.PasswordHash,
                Neighborhood = createModelRequest.Neighborhood,
                City = createModelRequest.City,
                DNA = dnaJson,
                TelefonePrincipal = createModelRequest.TelefonePrincipal,
                TelefoneSecundario = createModelRequest.TelefoneSecundario,

            };
                               
           _modelRepository.UpdateModelAsync(model);    

            return NoContent();
        }
    }
}

