using Microsoft.AspNetCore.Mvc;
using Pregiato.API.Interface;
using Pregiato.API.Requests;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using Pregiato.API.Data;
using Microsoft.AspNetCore.Authorization;
using Pregiato.API.Response;
using Pregiato.API.Models;
namespace Pregiato.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgencyContractController : ControllerBase
    {
        private readonly IContractService _contractService;
        private readonly IModelRepository _modelRepository;
        private readonly IPaymentService _paymentService;
        private readonly IContractRepository _contractRepository;    
        private readonly ModelAgencyContext _context;     
        public AgencyContractController(
            IContractService contractService,
            IModelRepository modelRepository,
            ModelAgencyContext context,
            IPaymentService paymentService,
            IContractRepository contractRepository)
        {
            _contractService = contractService ?? throw new ArgumentNullException(nameof(contractService));
            _modelRepository = modelRepository ?? throw new ArgumentNullException(nameof(modelRepository));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _paymentService = paymentService;
            _contractRepository = contractRepository; 
            
        } 

        [Authorize(Policy = "AdminOrManager")]
        [SwaggerOperation( Summary = "Gera um contrato Termo de comprometimento", Description = "Este endpoint gera o Termo de comprometimento.")]
        [SwaggerResponse(200, "Contrato gerado com sucesso", typeof(string))]
        [SwaggerResponse(400, "Requisição inválida")]
        [SwaggerResponse(404, "Modelo não encontrado")]
        [HttpPost("generate/commitmentTerm")]
        public async Task<IActionResult> GenerateCommitmentTermContract
        ([FromBody]CreateRequestCommitmentTerm createRequestCommitmentTerm,[FromQuery] string queryModel)
        {
            var model = await _modelRepository.GetModelByCriteriaAsync(queryModel);

            if (model == null)
            {
                return NotFound("Modelo não encontrado.");
            }

            var parameters = new Dictionary<string, string>
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

            createRequestCommitmentTerm.cpfModelo = model.CPF;

            var contract = await _contractService.GenerateContractCommitmentTerm(createRequestCommitmentTerm, queryModel);

            await _context.SaveChangesAsync();

            return Ok($"Termo de comprometimento para: {model.Name}, gerado com sucesso. Código da Proposta: {contract.CodProposta}.");
        }


        [Authorize(Policy = "AdminOrManager")]
        [SwaggerResponse(200, "Contrato gerado com sucesso", typeof(string))]
        [SwaggerResponse(400, "Requisição inválida")]
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

            var parameters = new Dictionary<string, string>
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

        //[Authorize(Policy = "AdminOrManager")]
        [SwaggerOperation("Processo de gerar contrato de Agencimaneto e Fotoprgrafia.")]
        [HttpPost("generate/Agency&PhotographyProductionContracts")]
        public async Task<IActionResult> GenerateAgencyPhotographyProductionContractsAsync(CreateContractModelRequest createContractModelRequest)
        {


            var model = await _modelRepository.GetModelByCriteriaAsync(createContractModelRequest.ModelIdentification);

            if (model == null)
            {
                return NotFound("Modelo não encontrado.");
            }

            List<ContractBase> contracts = await _contractService.GenerateAllContractsAsync(createContractModelRequest);

            var response = new ContractGenerationResponse
            {
                ContractName = $"Contrato de Agenciamento & Photography Production.",
                Message = $"Contrato para {model.Name}, emitidos com sucesso!",
                Contracts = contracts.Select(c => new ContractSummary
                {
                    CodProposta = c.CodProposta
                }).ToList()
            };
         
            return Ok(response);
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
                pdfBytes =  await _contractService.ExtractBytesFromString(contentString);
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
                .Where(p => p.Id == paymentId)
                .Select(p => new { p.Comprovante })
                .FirstOrDefaultAsync();

            if (payment == null || payment.Comprovante == null)
                return NotFound("Receipt not found.");

            return File(payment.Comprovante, "application/pdf", "payment_receipt.pdf");
        }
    }
}
