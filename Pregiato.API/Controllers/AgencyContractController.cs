using Microsoft.AspNetCore.Mvc;
using Pregiato.API.Interface;
using Pregiato.API.Requests;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using Pregiato.API.Data;
using Pregiato.API.Models;

namespace Pregiato.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgencyContractController : ControllerBase
    {
        private readonly IContractService _contractService;
        private readonly IModelRepository _modelRepository;
        private readonly ModelAgencyContext _context;

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
                { "Endereço-Modelo", model.Address ?? "N/A" },
                { "Numero-Modelo", model.BankAccount ?? "N/A" },
                { "Bairro-Modelo", "N/A" },
                { "Cidade-Modelo", "N/A" },
                { "CEP-Modelo", model.PostalCode ?? "N/A" }
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

            // Garantir que os valores de Cidade e Bairro estão preenchidos
            model.Neighborhood ??= "N/A";
            model.City ??= "N/A";

            var parameters = new Dictionary<string, string>
            {
                { "Nome-Modelo", model.Name },
                { "CPF-Modelo", model.CPF },
                { "RG-Modelo", model.RG },
                { "Endereço-Modelo", model.Address ?? "N/A" },
                { "Numero-Modelo", model.BankAccount ?? "N/A" },
                { "Bairro-Modelo", model.Neighborhood },
                { "Cidade-Modelo", model.City },
                { "CEP-Modelo", model.PostalCode ?? "N/A" }
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

        //[HttpGet("download/{contractId}")]
        //[SwaggerOperation("Busca e baixa um contrato pelo ID.")]
        //public async Task<IActionResult> DownloadContractAsync(Guid contractId)
        //{
        //    // Busca o contrato pelo ID na base de dados
        //    var contract = await _modelRepository.GetByIdModelAsync(contractId);    

        //    if (contract == null)
        //    {
        //        return NotFound("Contrato não encontrado.");
        //    }

        //    // Verifica se o conteúdo do contrato está disponível
        //    if (contract.Content == null || contract.Content.Length == 0)
        //    {
        //        return BadRequest("O conteúdo do contrato está vazio ou não foi salvo corretamente.");
        //    }

        //    // Caminho para salvar o contrato na área de trabalho
        //    var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        //    var filePath = Path.Combine(desktopPath, contract.ContractFilePath);

        //    // Salva o arquivo na área de trabalho
        //    await System.IO.File.WriteAllBytesAsync(filePath, contract.Content);

        //    // Retorna a confirmação do salvamento
        //    return Ok($"Contrato salvo com sucesso na área de trabalho: {filePath}");

        //    /*
        //    // Código para fazer o download do arquivo (comente a linha acima e descomente abaixo para ativar o download)
        //    return File(contract.Content, "application/pdf", contract.ContractFilePath);
        //    */
        //}


    }
}
