using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Pregiato.API.Requests;
using Swashbuckle.AspNetCore.Annotations;
using Pregiato.API.Data;
using Microsoft.AspNetCore.Authorization;
using Pregiato.API.DTO;
using Pregiato.API.Interfaces;
using Pregiato.API.Response;
using Pregiato.API.Models;
using Pregiato.API.Services.ServiceModels;
using Pregiato.API.Services;
using Microsoft.EntityFrameworkCore;
using System.Net.NetworkInformation;
namespace Pregiato.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgencyContractController(
          IContractService contractService,
          IModelRepository modelRepository,
          IContractRepository contractRepository,
          IUserService userService,
          IDbContextFactory<ModelAgencyContext> contextFactory,
          ModelAgencyContext context,
         
          CustomResponse customResponse) : ControllerBase
    {
        private readonly ModelAgencyContext _context = context ?? throw new ArgumentNullException(nameof(context));
        private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        private readonly IContractService _contractService = contractService ?? throw new ArgumentNullException(nameof(contractService));
        private readonly IModelRepository _modelRepository = modelRepository ?? throw new ArgumentNullException(nameof(modelRepository));
        private readonly IDbContextFactory<ModelAgencyContext> _contextFactory = contextFactory ??  throw new ArgumentNullException(nameof(contextFactory));

        [Authorize(Policy = "ManagementPolicyContracts")]
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

            ContractBase contract = await _contractService.GenerateContractCommitmentTerm(createRequestCommitmentTerm, queryModel);

            await _context.SaveChangesAsync();

            return Ok($"Termo de comprometimento , gerado com sucesso. Código da Proposta: {contract.CodProposta}.");
        }


        [Authorize(Policy = "PolicyGenerateContracts")]
        [SwaggerResponse(200, "Contrato gerado com sucesso", typeof(string))]
        [SwaggerResponse(400, "Requisição inválida.")]
        [SwaggerResponse(404, "Modelo não encontrado")]
        [SwaggerOperation("Processo de gerar o contrato: Termo de Concessão de direito de imagem.")]
        [HttpPost("generate/imageRightsTerm")]
        public async Task<IActionResult> GenerateImageRightsTermContract
        ([FromQuery] string queryModel)
        {
            Model? model = await _modelRepository.GetModelByCriteriaAsync(queryModel);

            if (model == null)
            {
                return NotFound("Modelo não encontrado.");
            }

            Dictionary<string, string?> parameters = new Dictionary<string, string?>
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
            ContractBase contract = await _contractService.GenetayeContractImageRightsTerm(queryModel);
            await _context.SaveChangesAsync();
            return Ok($"Termo de Concessão de direito de imagem para: {model.Name}, gerado com sucesso. Código da Proposta: {contract.CodProposta}.");
        }

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
                    List<string> erros = ModelState.Values.SelectMany(v => v.Errors)
                                                         .Select(e => e.ErrorMessage)
                                                         .ToList();
                    return ActionResultIndex.Failure($"OS DADOS FORNECIDOS SÃO INVÁLIDOS: {string.Join(", ", erros).ToUpper()}");
                }

                Console.WriteLine($"BUSCANDO DADOS DO MODELO {createContractModelRequest.ModelIdentification}");

                Model? model = await _modelRepository.GetModelByCriteriaAsync(createContractModelRequest.ModelIdentification);

                if (model == null)
                {
                    return ActionResultIndex.Failure("MODELO NÃO ENCONTRADO COM OS CRITÉRIOS FORNECIDOS.");
                }

                List<ContractBase> contracts = await _contractService.GenerateAllContractsAsync(createContractModelRequest, model);

                if (contracts == null || !contracts.Any())
                {
                    return ActionResultIndex.Failure("NENHUM CONTRATO FOI GERADO.");
                }

                string contentString = await _contractService.ConvertBytesToString(
                    contracts.FirstOrDefault(c =>
                        c.TemplateFileName == "PhotographyProductionContractMinority.html" ||
                        c.TemplateFileName == "PhotographyProductionContract.html")?.Content);

                byte[] pdfBytes = await _contractService.ExtractBytesFromString(contentString)
                                                         .ConfigureAwait(true);

                string message = $"CONTRATO PARA {model.Name.ToUpper()}, EMITIDOS COM SUCESSO!";
                List<ContractSummary> contractsSummary = contracts.Select(c => new ContractSummary
                {
                    CodProposta = c.CodProposta
                }).ToList();

                var response = new ContractGenerationResponse
                {
                    ContractName = "Photography Production Contract",
                    Message = message,
                    Contracts = contractsSummary
                };

                string metadataJson = JsonSerializer.Serialize(new
                {
                    Message = message,
                    Contracts = contractsSummary
                });


                Response.Headers.Add("X-Contract-Metadata", metadataJson);

                return File(pdfBytes, "application/pdf", "contract.pdf");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERRO AO GERAR CONTRATOS: {ex.Message}");
                return ActionResultIndex.Failure($"OCORREU UM ERRO NA OPERAÇÃO: {ex.Message}", isSpeakOnOperation: true);
            }
        }

        //[Authorize(Policy = "PolicyGenerateContracts")]
        [HttpGet("download-contract")]
        public async Task<IActionResult> DownloadContractAsync(int proposalCode)
        {

            ContractDTO? contract = await contractRepository.DownloadContractAsync(proposalCode).ConfigureAwait(true);

            if (contract == null)
            {
                return NotFound("CONTRATO NÃO ENCONTRADO.");
            }

            string contentString = await _contractService.ConvertBytesToString(contract.Content).ConfigureAwait(true);

            byte[] pdfBytes;

            try
            {
                pdfBytes = await _contractService.ExtractBytesFromString(contentString).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                return BadRequest($"ERRO AO PROCESSAR O CONTEÚDO DO CONTRATO: {ex.Message}");
            }

            return File(pdfBytes, "application/pdf", "contract.pdf");
        }

        [Authorize(Policy = "PolicyGenerateContracts")]
        [HttpPost("upload/payment-receipt")]
        public async Task<IActionResult> UploadPaymentReceipt([FromForm] UploadPaymentReceiptRequest request)
        {
            if (request.File == null || request.File.Length == 0)
                return BadRequest("The receipt is required for Pix payments.");

            Payment? payment = await _context.Payments.FindAsync(request.PaymentId);

            if (payment == null)
                return NotFound("Payment not found.");

            using MemoryStream memoryStream = new MemoryStream();
            await request.File.CopyToAsync(memoryStream);
            payment.Comprovante = memoryStream.ToArray();
            _context.Entry(payment).Property(p => p.Comprovante).IsModified = true;

            await _context.SaveChangesAsync().ConfigureAwait(true);

            return Ok("Receipt uploaded successfully.");
        }


        [Authorize(Policy = "ManagementPolicyLevel3")]
        [HttpGet("all-contracts")]
        public async Task<IActionResult> GetAllContractsForAgencyAsync()
        {
            try
            {
                List<ContractSummaryDTO>? contracts = await contractRepository.GetAllContractsAsync().ConfigureAwait(true);

                if (contracts == null || !contracts.Any())
                {
                    return ActionResultIndex.Failure("NENHUM CONTRATO ENCONTRADO NA BASE DE DADOS.");
                }
                return ActionResultIndex.Success(
                    data: new
                    {
                        TotalContracts = contracts.Count,
                        Contracts = contracts
                    },
                    message: "TODOS OS CONTRATOS FORAM RECUPERADOS COM SUCESSO!"
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERRO AO RECUPERAR CONTRATOS: {ex.Message.ToUpper()}");
                return ActionResultIndex.Failure($"ERRO AO RECUPERAR OS CONTRATOS: {ex.Message.ToUpper()}", isSpeakOnOperation: true);
            }
        }

        [Authorize(Policy = "PolicyProducers")]
        [HttpGet("all-contracts-producers")]
        public async Task<IActionResult> GetAllContractsForProducers()
        {
            try
            {
                var user = await _userService.UserCaptureByToken().ConfigureAwait(true);

                if (user.CodProducers == null && user.UserType == UserType.Producers)
                {
                    return BadRequest("USUÁRIO NÃO ENCONTRADO OU NÃO É UM PRODUTOR.");
                }

                List<ContractSummaryDTO> contracts = await contractRepository
                        .GetAllContractsForProducersAsync(user.CodProducers)
                        .ConfigureAwait(true);

                    if (!contracts.Any())
                    {
                        return ActionResultIndex.Failure("NENHUM CONTRATO ENCONTRADO NA BASE DE DADOS.");
                    }

                    return ActionResultIndex.Success(
                        data: new
                        {
                            TotalContracts = contracts.Count,
                            Contracts = contracts
                        },
                        message: "TODOS OS CONTRATOS FORAM RECUPERADOS COM SUCESSO!"
                    );
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERRO AO RECUPERAR CONTRATOS: {ex.Message.ToUpper()}");
                return ActionResultIndex.Failure($"ERRO AO RECUPERAR OS CONTRATOS: {ex.Message.ToUpper()}",
                    isSpeakOnOperation: true);
            }

        }
    }
    
}
