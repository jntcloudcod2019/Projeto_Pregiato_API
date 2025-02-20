using Microsoft.AspNetCore.Mvc;
using Pregiato.API.Interface;
using Pregiato.API.Requests;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using Pregiato.API.Data;
using Microsoft.AspNetCore.Authorization;

namespace Pregiato.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgencyContractController : ControllerBase
    {
        private readonly IContractService _contractService;
        private readonly IModelRepository _modelRepository;
        private readonly IPaymentService _paymentService;
        private readonly ModelAgencyContext _context;     
        public AgencyContractController(
            IContractService contractService,
            IModelRepository modelRepository,
            ModelAgencyContext context,
            IPaymentService paymentService)
        {
            _contractService = contractService ?? throw new ArgumentNullException(nameof(contractService));
            _modelRepository = modelRepository ?? throw new ArgumentNullException(nameof(modelRepository));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _paymentService = paymentService;
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
        [SwaggerOperation("Processo de gerar o contrato: CONTRATO DE PRODUÇÃO FOTOGRÁFICA E ACESSO A PLATAFORMA MY PREGIATO")]
        [HttpPost("generate/photographyProductionContract")]
        public async Task<IActionResult> GeneratePhotographyProductionContract
        ([FromBody] PaymentRequest paymentRequest,[FromQuery] string queryModel)
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


            var contract = await _contractService.GenerateContractPhotographyProduction(paymentRequest, queryModel);

            await _context.SaveChangesAsync();

            return Ok($"Contrato de PFPMP para {model.Name}, gerado com sucesso. Código da Proposta: {contract.CodProposta}.");
        }

        [Authorize(Policy = "AdminOrManager")]
        [SwaggerResponse(200, "Contrato gerado com sucesso", typeof(string))]
        [SwaggerResponse(400, "Requisição inválida")]
        [SwaggerResponse(404, "Modelo não encontrado")]
        [SwaggerOperation("Processo de gerar o contrato: CONTRATO DE AGENCIAMENTO")]
        [HttpPost("generate/agencyContract")]
        public async Task<IActionResult> GenerateAgencyContract
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

            var contract = await _contractService.GenerateContractAgency(queryModel);

            await _context.SaveChangesAsync();

            return Ok($"Contrato de PFPMP para {model.Name}, gerado com sucesso. Código da Proposta: {contract.CodProposta}.");

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


        [SwaggerOperation("Processo de gerar todos os contratos.")]
        [HttpPost("generate/allContracts")]
        public async Task<IActionResult> GenerateAllContractsAsync([FromBody] PaymentRequest paymentRequest, [FromQuery] string query)
        {
            var model = await _modelRepository.GetModelByCriteriaAsync(query);

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
                    {"Valor-Contrato",paymentRequest.Valor.ToString("C")},
                    {"Forma-Pagamento", paymentRequest.MetodoPagamento}
            };

            var contracts = await _contractService.GenerateAllContractsAsync(
             paymentRequest,
             idModel: model.IdModel.ToString(),
             cpf: model.CPF,
             rg: model.RG
           );

            await _context.SaveChangesAsync();
            return Ok(contracts);
        }

        [Authorize(Policy = "AdminOrManagerOrModel")]
        [HttpGet("download-contract")]
        public async Task<IActionResult> DownloadContract([FromQuery] Guid? modelId,[FromQuery] Guid? contractId,[FromQuery] int codProposta)
        {
            var contract = await _context.Contracts
                .FirstOrDefaultAsync(c =>
                    (modelId != null && c.ModelId == modelId) ||
                    (contractId != null && c.ContractId == contractId) ||
                    (codProposta != null && c.CodProposta == codProposta));

            if (contract == null)
            {
                return NotFound("Contrato não encontrado.");
            }

            var pdfBytes = contract.Content;

            return File(pdfBytes, "application/pdf", contract.ContractFilePath);
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
           
            // Inclusão de dados na base sem o mapeamento feito EntityFramework na tabela. 
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
