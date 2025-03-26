using Microsoft.AspNetCore.Mvc;
using Pregiato.API.Interface;
using Pregiato.API.Requests;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using Pregiato.API.Data;
using Microsoft.AspNetCore.Authorization;
using Pregiato.API.Response;
using Pregiato.API.Models;
using PuppeteerSharp;
using Pregiato.API.Responses;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using Pregiato.API.Services.ServiceModels;
using System.Diagnostics.Contracts;
using System.Text;
using Newtonsoft.Json;
namespace Pregiato.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgencyContractController : ControllerBase
    {
        private readonly IContractService _contractService;
        private readonly IModelRepository _modelRepository;
        private readonly IContractRepository _contractRepository;
        private readonly ModelAgencyContext _context;
        private readonly CustomResponse _customResponse;
        public AgencyContractController(
              IContractService contractService,
              IModelRepository modelRepository,
              IPaymentService paymentService,
              IContractRepository contractRepository,
              ModelAgencyContext context,
              CustomResponse customResponse)
        {
            _contractService = contractService ?? throw new ArgumentNullException(nameof(contractService));
            _modelRepository = modelRepository ?? throw new ArgumentNullException(nameof(modelRepository));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _contractRepository = contractRepository;
            _customResponse = customResponse;
        }

        [Authorize(Policy = "AdminOrManager")]
        [SwaggerOperation(Summary = "Gera um contrato Termo de comprometimento", Description = "Este endpoint gera o Termo de comprometimento.")]
        [SwaggerResponse(200, "Contrato gerado com sucesso", typeof(string))]
        [SwaggerResponse(400, "Requisição inválida")]
        [SwaggerResponse(404, "Modelo não encontrado")]
        [HttpPost("generate/commitmentTerm")]
        public async Task<IActionResult> GenerateCommitmentTermContract
        ([FromBody] CreateRequestCommitmentTerm createRequestCommitmentTerm, [FromQuery] string queryModel)
        {
            if (createRequestCommitmentTerm == null || queryModel == null)
            {
                throw new Exception("Campos de parametros vazio.");
            }

            var contract = await _contractService.GenerateContractCommitmentTerm(createRequestCommitmentTerm, queryModel);

            await _context.SaveChangesAsync();

            return Ok($"Termo de comprometimento , gerado com sucesso. Código da Proposta: {contract.CodProposta}.");
        }


        [Authorize(Policy = "AdminOrManager")]
        [SwaggerResponse(200, "Contrato gerado com sucesso", typeof(string))]
        [SwaggerResponse(400, "Requisição inválida.")]
        [SwaggerResponse(404, "Modelo não encontrado")]
        [SwaggerOperation("Processo de gerar o contrato: Termo de Concessão de direito de imagem.")]
        [HttpPost("generate/imageRightsTerm")]
        public async Task<IActionResult> GenerateImageRightsTermContract
        ([FromQuery] string queryModel)
        {
            var model = await _modelRepository.GetModelByCriteriaAsync(queryModel);

            if (model == null)
            {
                return NotFound("Modelo não encontrado.");
            }

            var parameters = new Dictionary<string, string?>
            {
                    {"Nome-Modelo", model.Name },
                    {"CPF-Modelo", model.CPF },
                    {"RG-Modelo", model.RG },
                    {"Endereço-Modelo", model.Address},
                    {"Numero-Modelo",model.NumberAddress},
                    {"Bairro-Modelo", model.Neighborhood},
                    {"Cidade-Modelo", model.City},
                    {"CEP-Modelo", model.PostalCode},
                    {"Complemento-Modelo", model.Complement},
                    {"Telefone-Principal", model.TelefonePrincipal},
                    {"Telefone-Secundário", model.TelefoneSecundario},
            };
            var contract = await _contractService.GenetayeContractImageRightsTerm(queryModel);
            await _context.SaveChangesAsync();
            return Ok($"Termo de Concessão de direito de imagem para: {model.Name}, gerado com sucesso. Código da Proposta: {contract.CodProposta}.");
        }

        [Authorize(Policy = "AdminOrManager")]
        [SwaggerOperation("Processo de gerar contrato de Agenciamento e Fotoprgrafia.")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [HttpPost("generate/Agency&PhotographyProductionContracts")]
        public async Task<IActionResult> GenerateAgencyPhotographyProductionContractsAsync(CreateContractModelRequest createContractModelRequest)
        {

            try
            {
                if (createContractModelRequest == null || !ModelState.IsValid)
                {
                    var erros = ModelState.Values.SelectMany(v => v.Errors)
                                         .Select(e => e.ErrorMessage)
                                         .ToList();
                    return ActionResultIndex.Failure($"Os dados fornecidos são inválidos: {string.Join(", ", erros)}");
                }

                Console.WriteLine($"Buscando dados do modelo {createContractModelRequest.ModelIdentification}");

                var model = await _modelRepository.GetModelByCriteriaAsync(createContractModelRequest.ModelIdentification);

                if (model == null)
                {
                    return ActionResultIndex.Failure("Modelo não encontrado com os critérios fornecidos.");
                }

                List<ContractBase> contracts = await _contractService.GenerateAllContractsAsync(createContractModelRequest, model);

                if (contracts == null || !contracts.Any())
                {
                    return ActionResultIndex.Failure("Nenhum contrato foi gerado.");
                }

                string contentString = await _contractService.ConvertBytesToString(
                contracts.FirstOrDefault(c => c.TemplateFileName == "PhotographyProductionContractMinority.html" || 
                c.TemplateFileName == "PhotographyProductionContract.html")?.Content);

                byte[] pdfBytes = await _contractService.ExtractBytesFromString(contentString);

                var message = $"Contrato para {model.Name}, emitidos com sucesso!";
                var contractsSummary = contracts.Select(c => new ContractSummary
                {
                    CodProposta = c.CodProposta
                }).ToList();

                var metadata = new
                {
                    Message = message,
                    Contracts = contractsSummary
                };

                string metadataJson = System.Text.Json.JsonSerializer.Serialize(metadata);

                Response.Headers.Add("X-Contract-Metadata", metadataJson);
                return File(pdfBytes, "application/pdf", "contract.pdf");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao gerar contratos: {ex.Message}");
                return ActionResultIndex.Failure($"Ocorreu um erro na operação: {ex.Message}", isSpeakOnOperation: true);
            }
        }

        [Authorize(Policy = "AdminOrManagerOrModel")]
        [HttpGet("download-contract")]
        public async Task<IActionResult> DownloadContractAsync(int proposalCode)
        {

            var contract = await _contractRepository.DownloadContractAsync(proposalCode);

            if (contract == null)
            {
                return NotFound("Contrato não encontrado.");
            }

            string contentString = await _contractService.ConvertBytesToString(contract.Content);

            byte[] pdfBytes;

            try
            {
                pdfBytes = await _contractService.ExtractBytesFromString(contentString);
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao processar o conteúdo do contrato: {ex.Message}");
            }

            return File(pdfBytes, "application/pdf", "contract.pdf");
        }

        [Authorize(Policy = "AdminOrManager")]
        [HttpPost("upload/payment-receipt")]
        public async Task<IActionResult> UploadPaymentReceipt([FromForm] UploadPaymentReceiptRequest request)
        {
            if (request.File == null || request.File.Length == 0)
                return BadRequest("The receipt is required for Pix payments.");

            var payment = await _context.Payments.FindAsync(request.PaymentId);

            if (payment == null)
                return NotFound("Payment not found.");

            using var memoryStream = new MemoryStream();
            await request.File.CopyToAsync(memoryStream);
            payment.Comprovante = memoryStream.ToArray();
            _context.Entry(payment).Property(p => p.Comprovante).IsModified = true;

            await _context.SaveChangesAsync();

            return Ok("Receipt uploaded successfully.");
        }


        [Authorize(Policy = "AdminOrManager")]
        [HttpGet("payment-receipt/{paymentId}")]
        public async Task<IActionResult> GetPaymentReceipt(Guid paymentId)
        {
            var payment = await _context.Payments
                .AsNoTracking()
                .Where(p => p.Id == paymentId)
                .Select(p => new { p.Comprovante })
                .FirstOrDefaultAsync();

            if (payment == null || payment.Comprovante == null)
                return NotFound("Receipt not found.");

            return File(payment.Comprovante, "application/pdf", "payment_receipt.pdf");
        }

        [HttpGet("all-contracts")]
        [Authorize(Policy = "AdminOrManagerOrModel")]
        public async Task<IActionResult> GetAllContractsForAgencyAsync()
        {
            try
            {
                var contracts = await _contractRepository.GetAllContractsAsync();

                if (contracts == null || !contracts.Any())
                {
                    return ActionResultIndex.Failure("Nenhum contrato encontrado na base de dados.");
                }
                return ActionResultIndex.Success(
                    data: new
                    {
                        TotalContracts = contracts.Count,
                        Contracts = contracts
                    },
                    message: "Todos os contratos foram recuperados com sucesso!"
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao recuperar contratos: {ex.Message}");
                return ActionResultIndex.Failure($"Erro ao recuperar os contratos: {ex.Message}", isSpeakOnOperation: true);
            }
        }
    }
}
