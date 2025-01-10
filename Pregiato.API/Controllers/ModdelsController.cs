using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pregiato.API.Interface;
using Pregiato.API.Models;
using Pregiato.API.Requests;
using Swashbuckle.AspNetCore.Annotations;

namespace Pregiato.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/Models")]
    public class ModdelsController : ControllerBase
    {
        private readonly IModelRepository _modelRepository;
        public ModdelsController(IModelRepository modelRepository)
        {
            _modelRepository = modelRepository ?? throw new ArgumentNullException(nameof(modelRepository));
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

            var model = new Moddels
            {
                Status = true,
                IdModel = new Guid(),
                CPF = createModelRequest.CPF,
                RG = createModelRequest.RG,                
                Name = createModelRequest.Name,
                Email = createModelRequest.Email,
                Address = createModelRequest.Address,
                PostalCode = createModelRequest.PostalCode,
                BankAccount = createModelRequest.BankAccount,
                PasswordHash = createModelRequest.PasswordHash               
            };

            await _modelRepository.AddModelAsync(model);
            return CreatedAtAction(nameof(AddNewModel), model); 
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