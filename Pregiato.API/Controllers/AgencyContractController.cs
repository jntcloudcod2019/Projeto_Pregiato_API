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

        [HttpGet("download-contract")]
        public async Task<IActionResult> DownloadContract(int codProposta, Guid idContract, Guid idModel)
        {
            // Buscar o contrato no banco de dados
            var contract = await _context.Contracts
                .Where(c => c.CodProposta == codProposta|| c.ContractId == idContract || c.ModelId == idModel)
                .FirstOrDefaultAsync();

            if (contract == null)
            {
                return NotFound("Contrato não encontrado.");
            }

            // Gerar o PDF
            string filePath = GenerateContractPdf(contract);

            // Retornar o caminho do arquivo
            return Ok(new { Message = "PDF gerado com sucesso.", FilePath = filePath });
        }


        private string GenerateContractPdf(ContractBase contract)
        {
            // Caminho para salvar o PDF na área de trabalho
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string fileName = $"Contrato_{contract.CodProposta}_{contract.ContractId}.pdf";
            string filePath = Path.Combine(desktopPath, fileName);

            // Usando iText7 para criar o PDF
            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                PdfWriter writer = new PdfWriter(stream);
                PdfDocument pdfDoc = new PdfDocument(writer);
                Document document = new Document(pdfDoc);

                // Adicionar conteúdo ao PDF
                document.Add(new Paragraph($"Contrato ID: {contract.ContractId}"));
                document.Add(new Paragraph($"Código da Proposta: {contract.CodProposta}"));
                document.Add(new Paragraph($"ID do Modelo: {contract.ModelId}"));
                document.Add(new Paragraph($"Local: {contract.City ?? "N/A"}"));
                document.Add(new Paragraph($"Bairro: {contract.Neighborhood ?? "N/A"}"));
                document.Add(new Paragraph($"Data de Criação: {contract.CreatedAt}"));
                document.Add(new Paragraph($"Última Atualização: {contract.UpdatedAt}"));

                // Adicionar conteúdo específico do contrato
                if (contract.Content != null)
                {
                    string contentText = System.Text.Encoding.UTF8.GetString(contract.Content);
                    document.Add(new Paragraph("Conteúdo do Contrato:"));
                    document.Add(new Paragraph(contentText));
                }

                document.Close();
            }

            return filePath;
        }




    }
}
