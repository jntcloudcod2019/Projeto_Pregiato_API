using Microsoft.AspNetCore.Mvc;
using Pregiato.API.Interface;
using Pregiato.API.Requests;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using Pregiato.API.Data;
using Pregiato.API.Models;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using iText.Layout;
using Pregiato.API.Services;

namespace Pregiato.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgencyContractController : ControllerBase
    {
        private readonly IContractService _contractService;
        private readonly IModelRepository _modelRepository;
        private readonly ModelAgencyContext _context;
        private readonly ContractService contractService;

        // Construtor atualizado para incluir _context
        public AgencyContractController(
            IContractService contractService,
            IModelRepository modelRepository,
            ModelAgencyContext context)
        {
            _contractService = contractService ?? throw new ArgumentNullException(nameof(contractService));
            _modelRepository = modelRepository ?? throw new ArgumentNullException(nameof(modelRepository));
            _context = context ?? throw new ArgumentNullException(nameof(context));
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
                { "Endereço-Modelo", model.Address },
                { "Numero-Modelo", model.BankAccount },
                { "Bairro-Modelo", model.Neighborhood },
                { "Cidade-Modelo", model.City },
                { "CEP-Modelo", model.PostalCode }
            };

            var contract = await _contractService.GenerateContractAsync(model.IdModel, request.JobId, "Agency", parameters);

            return Ok(contract);
        }

        [SwaggerOperation("Processo de gerar Contrato Photography.")]
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
                { "Endereço-Modelo", model.Address },
                { "Numero-Modelo", model.BankAccount },
                { "Bairro-Modelo",model.Neighborhood },
                { "Cidade-Modelo", model.City },
                { "CEP-Modelo", model.PostalCode }
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
                { "Endereço-Modelo", model.Address },
                { "Numero-Modelo", model.BankAccount },
                { "Bairro-Modelo", model.Neighborhood },
                { "Cidade-Modelo", model.City},
                { "CEP-Modelo", model.PostalCode }
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
                { "Endereço-Modelo", model.Address },
                { "Numero-Modelo", model.BankAccount },
                { "Bairro-Modelo", model.Neighborhood },
                { "Cidade-Modelo", model.City },
                { "CEP-Modelo", model.PostalCode }
            };

            var contract = await _contractService.GenerateContractAsync(model.IdModel, request.JobId, "ImageRights", parameters);

            return Ok(contract);
        }

        [SwaggerOperation("Processo de gerar todos os contratos")]
        [HttpPost("generate/all")]
        public async Task<IActionResult> GenerateAllContractsAsync([FromQuery] string? idModel = null, [FromQuery] string? cpf = null, [FromQuery] string? rg = null, [FromBody] ContractRequest? request = null)
        {
            if (string.IsNullOrEmpty(idModel) && string.IsNullOrEmpty(cpf) && string.IsNullOrEmpty(rg))
            {
                return BadRequest("Pelo menos um dos parâmetros 'IdModel', 'CPF', ou 'RG' deve ser fornecido.");
            }

            var model = await _context.Models.FirstOrDefaultAsync(m =>
                (idModel != null && m.IdModel.ToString() == idModel) ||
                (cpf != null && m.CPF == cpf) ||
                (rg != null && m.RG == rg));

            if (model == null)
            {
                return NotFound("Modelo não encontrado.");
            }

            var parameters = new Dictionary<string, string>
            {
                { "Local-Contrato", "São Paulo, SP"},
                { "Data-Contrato", DateTime.Now.ToString("dd/MM/yyyy") },
                { "Mês-Contrato", DateTime.Now.ToString("MMMM")},
                { "Nome-Modelo", model.Name },
                { "CPF-Modelo", model.CPF },
                { "RG-Modelo", model.RG },
                { "Endereço-Modelo", model.Address },
                { "Numero-Modelo", model.BankAccount },
                { "Bairro-Modelo", model.Neighborhood },
                { "Cidade-Modelo", model.City },
                { "CEP-Modelo", model.PostalCode }
            };

            if (request?.JobId == null)
            {
                return BadRequest("O JobId é obrigatório para gerar os contratos.");
            }

            // Lista para armazenar os contratos gerados
            var contracts = new List<ContractBase>();

            // Geração dos contratos
            contracts.Add(await _contractService.GenerateContractAsync(model.IdModel, request.JobId, "Agency", parameters));
            contracts.Add(await _contractService.GenerateContractAsync(model.IdModel, request.JobId, "Photography", parameters));
            contracts.Add(await _contractService.GenerateContractAsync(model.IdModel, request.JobId, "Commitment", parameters));
            contracts.Add(await _contractService.GenerateContractAsync(model.IdModel, request.JobId, "ImageRights", parameters));

            return Ok(contracts);
        }

        [HttpGet("download-contract")]
        public async Task<IActionResult> DownloadContract(int codProposta, Guid idContract, Guid idModel)
        {
            try
            {
                // Certifique-se de que o método retorne uma string
                string filePath = await _contractService.GenerateContractPdf(codProposta, idContract);

                // Validação do arquivo gerado
                if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
                {
                    return NotFound("O contrato não foi encontrado ou não foi gerado corretamente.");
                }

                // Retorna o arquivo para download
                byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                string fileName = Path.GetFileName(filePath);

                return File(fileBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao gerar o contrato: {ex.Message}");
            }

        }
    }
}
