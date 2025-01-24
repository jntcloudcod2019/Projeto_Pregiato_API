using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pregiato.API.Interface;
using Pregiato.API.Models;
using Pregiato.API.Requests;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json;

namespace Pregiato.API.Controllers
{
    //[Authorize("AdministratorPolicy, ManagerPolicy")]
    [ApiController]
    [Route("api/Models")]
    public class ModdelsController : ControllerBase
    {
        private readonly IModelRepository _modelRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService; 
        public ModdelsController(IModelRepository modelRepository, IUserService userService)
        {
            _modelRepository = modelRepository ?? throw new ArgumentNullException(nameof(modelRepository));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
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
        public  async Task <IActionResult> AddNewModel([FromBody] CreateModelRequest createModelRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var dnaJson = JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                physicalCharacteristics = createModelRequest.PhysicalCharacteristics,
                appearance = createModelRequest.Appearance,
                additionalAttributes = createModelRequest.AdditionalAttributes
            }));

            var model = new Moddels
            {
                Name = createModelRequest.Name,
                CPF = createModelRequest.CPF,
                RG = createModelRequest.RG,
                Email = createModelRequest.Email,
                PostalCode = createModelRequest.PostalCode,
                Address = createModelRequest.Address,
                BankAccount = createModelRequest.BankAccount,
                PasswordHash = createModelRequest.PasswordHash,
                Neighborhood = createModelRequest.Neighborhood,
                City = createModelRequest.City,
                DNA = dnaJson,  
            };
            
            await _modelRepository.AddModelAsync(model);

            var modelUser = new LoginUserRequest
            {
              Username = model.Name,    
              Password = model.PasswordHash,
              UserType = "Model"
            };

            var userResult = await _userService.RegisterUserAsync(modelUser.Username,model.Email, modelUser.Password,modelUser.UserType.ToString());
            if (userResult == null)
            {
                return StatusCode(500, "Erro ao criar usuário para o modelo.");
            }

            return Ok("Modelo e usuário criado com sucesso!");
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
            
            if (modelExists== null)
            {
                return NotFound();
            }

            switch (updateModelRequest)
            {
                case { Name: not null  }:
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


    }
}