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
using static System.Runtime.InteropServices.JavaScript.JSType;
using Org.BouncyCastle.Asn1.Ocsp;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;


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
        private readonly ContractService contractService;
        private readonly PaymentService paymentService; 
       
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

       
        [SwaggerOperation("Processo de gerar o contrato: TERMO DE COMPROMETIMENTO")]
        [HttpPost("generate/ContractCommitmentTerm")]
        public async Task<IActionResult> GenerateCommitmentTerm
        ([FromBody] CreateRequestContractImageRights createRequestContractImageRights,
        [FromQuery] string queryModel)
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

            createRequestContractImageRights.cpfModelo = model.CPF;

            var contracts = await _contractService.GenerateContractCommitmentTerm(createRequestContractImageRights, queryModel);

            _context.SaveChangesAsync();

            return Ok(contracts);
        }


        [SwaggerOperation("Processo de gerar todos os contratos.")]
        [HttpPost("generate/allContracts")]
        public async Task<IActionResult> GenerateAllContractsAsync
        ([FromBody] PaymentRequest paymentRequest,
        [FromQuery] string query)
     
        {
            var model = await _modelRepository.GetModelByCriteriaAsync(query);

            if (model == null)
            {
                return NotFound("Modelo não encontrado.");
            }
      
            var validationResult = await _paymentService.ValidatePayment(paymentRequest);
            if (validationResult != "validação de pagamento ok")
            {
                return BadRequest($"Erro ao validar o pagamento: {validationResult}");
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


        [HttpGet("download-contract")]
        public async Task<IActionResult> DownloadContract(int codProposta, Guid idContract, Guid idModel)
        {
            try
            {
                string filePath = await _contractService.GenerateContractPdf(codProposta, idContract);

                if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
                {
                    return NotFound("O contrato não foi encontrado ou não foi gerado corretamente.");
                }

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
