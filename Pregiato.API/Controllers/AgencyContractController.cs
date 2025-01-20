using Microsoft.AspNetCore.Mvc;
using Pregiato.API.Interface;
using Pregiato.API.Requests;
using Microsoft.EntityFrameworkCore;
using iText.Commons.Actions.Contexts;
using Pregiato.API.Data;
using Swashbuckle.AspNetCore.Annotations;
using Pregiato.API.Models;

namespace Pregiato.API.Controllers
{
    public class AgencyContractController : ControllerBase
    {
        private readonly IContractService _contractService;
        private readonly IModelRepository _modelRepository;
        private readonly ModelAgencyContext _context;
       

        public AgencyContractController(IContractService contractService, IModelRepository modelRepository)
        {
            _contractService = contractService;
            _modelRepository = modelRepository;
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
                model.BankAccount,
                model.PostalCode
            });
        }

        [SwaggerOperation("Processo de gerar Contrato da Agência.")]
        [HttpPost("generate/agency")]
        public async Task<IActionResult> GenerateAgencyContract([FromQuery] string query, [FromBody] ContractRequest request)
        {
            var model = await _context.Models.FirstOrDefaultAsync(m =>
                m.CPF == query || m.RG == query || m.Name.Contains(query) || m.IdModel.ToString() == query);

            if (model == null)
            {
                return NotFound("Modelo não encontrado.");
            }

            var parameters = new Dictionary<string, string>
                {
                { "Nome-Modelo", model.Name },
                { "CPF-Modelo", model.CPF },
                { "RG-Modelo", model.RG },
                { "Endereço-Modelo", model.Address ?? "N/A" },
                { "Numero-Modelo", model.BankAccount ?? "N/A" },
                { "Bairro-Modelo", "N/A" },
                { "Cidade-Modelo", "N/A" },
                { "CEP-Modelo", model.PostalCode ?? "N/A" }
           };

            var contract = await _contractService.GenerateContractAsync(model.IdModel, request.JobId, "Agency", parameters);

            return Ok(contract);
        }

        [SwaggerOperation("Processo de gerar Contrato photography.")]
        [HttpPost("generate/photography")]
        public async Task<IActionResult> GeneratePhotographyContract([FromQuery] string query, [FromBody] ContractRequest request)
        {
            var model = await _context.Models.FirstOrDefaultAsync(m =>
                m.CPF == query || m.RG == query || m.Name.Contains(query) || m.IdModel.ToString() == query);

            if (model == null)
            {
                return NotFound("Modelo não encontrado.");
            }

            var parameters = new Dictionary<string, string>
            {
                { "Nome-Modelo", model.Name },
                { "CPF-Modelo", model.CPF },
                { "RG-Modelo", model.RG },
                { "Endereço-Modelo", model.Address ?? "N/A" },
                { "Numero-Modelo", model.BankAccount ?? "N/A" },
                { "Bairro-Modelo", "N/A" },
                { "Cidade-Modelo", "N/A" },
                { "CEP-Modelo", model.PostalCode ?? "N/A" }
            };

            var contract = await _contractService.GenerateContractAsync(model.IdModel, request.JobId, "Photography", parameters);

            return Ok(contract);
        }

        [SwaggerOperation("Processo de gerar contrato de comprometimento.")]
        [HttpPost("generate/commitment")]
        public async Task<IActionResult> GenerateCommitmentTerm([FromQuery] string query, [FromBody] ContractRequest request)
        {
            var model = await _context.Models.FirstOrDefaultAsync(m =>
                m.CPF == query || m.RG == query || m.Name.Contains(query) || m.IdModel.ToString() == query);

            if (model == null)
            {
                return NotFound("Modelo não encontrado.");
            }

            var parameters = new Dictionary<string, string>
            {
                { "Nome-Modelo", model.Name },
                { "CPF-Modelo", model.CPF },
                { "RG-Modelo", model.RG },
                { "Endereço-Modelo", model.Address ?? "N/A" },
                { "Numero-Modelo", model.BankAccount ?? "N/A" },
                { "Bairro-Modelo", "N/A" },
                { "Cidade-Modelo", "N/A" },
                { "CEP-Modelo", model.PostalCode ?? "N/A" }
            };

            var contract = await _contractService.GenerateContractAsync(model.IdModel, request.JobId, "Commitment", parameters);

            return Ok(contract);
        }

        [SwaggerOperation("Processo de gerar contrato de imagem.")]
        [HttpPost("generate/image-rights")]
        public async Task<IActionResult> GenerateImageRightsTerm([FromQuery] string query, [FromBody] ContractRequest request)
        {
            var model = await _context.Models.FirstOrDefaultAsync(m =>
                m.CPF == query || m.RG == query || m.Name.Contains(query) || m.IdModel.ToString() == query);

            if (model == null)
            {
                return NotFound("Modelo não encontrado.");
            }

            var parameters = new Dictionary<string, string>
            {
                { "Nome-Modelo", model.Name },
                { "CPF-Modelo", model.CPF },
                { "RG-Modelo", model.RG },
                { "Endereço-Modelo", model.Address ?? "N/A" },
                { "Numero-Modelo", model.BankAccount ?? "N/A" },
                { "Bairro-Modelo", "N/A" },
                { "Cidade-Modelo", "N/A" },
                { "CEP-Modelo", model.PostalCode ?? "N/A" }
            };

            var contract = await _contractService.GenerateContractAsync(model.IdModel, request.JobId, "ImageRights", parameters);

            return Ok(contract);
        }
        [SwaggerOperation("Processo de gerar todos os contratos")]
        [HttpPost("generate/all")]
        public async Task<IActionResult> GenerateAllContracts([FromQuery] string query, [FromBody] ContractRequest request)
        {
            var model = await _context.Models.FirstOrDefaultAsync(m =>
                m.CPF == query || m.RG == query || m.Name.Contains(query) || m.IdModel.ToString() == query);

            if (model == null)
            {
                return NotFound("Modelo não encontrado.");
            }

            var parameters = new Dictionary<string, string>
            {
                { "Nome-Modelo", model.Name },
                { "CPF-Modelo", model.CPF },
                { "RG-Modelo", model.RG },
                { "Endereço-Modelo", model.Address ?? "N/A" },
                { "Numero-Modelo", model.BankAccount ?? "N/A" },
                { "Bairro-Modelo", "N/A" },
                { "Cidade-Modelo", "N/A" },
                { "CEP-Modelo", model.PostalCode ?? "N/A" }
            };

            var contracts = new List<ContractBase>();

            contracts.Add(await _contractService.GenerateContractAsync(model.IdModel, request.JobId, "Agency", parameters));
            contracts.Add(await _contractService.GenerateContractAsync(model.IdModel, request.JobId, "Photography", parameters));
            contracts.Add(await _contractService.GenerateContractAsync(model.IdModel, request.JobId, "Commitment", parameters));
            contracts.Add(await _contractService.GenerateContractAsync(model.IdModel, request.JobId, "ImageRights", parameters));

            return Ok(contracts);
        }


    }
}
