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
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
namespace Pregiato.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgencyContractController(
          IContractService contractService,
          IModelRepository modelRepository,
          IContractRepository contractRepository,
          IUserService userService,
          IAutentiqueService autentiqueService,
          IDbContextFactory<ModelAgencyContext> contextFactory,
          ModelAgencyContext context,
          CustomResponse customResponse) : ControllerBase
    {
        private readonly IAutentiqueService _autentiqueService = autentiqueService;
        private readonly ModelAgencyContext _context = context ?? throw new ArgumentNullException(nameof(context));
        private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        private readonly IContractService _contractService = contractService ?? throw new ArgumentNullException(nameof(contractService));
        private readonly IModelRepository _modelRepository = modelRepository ?? throw new ArgumentNullException(nameof(modelRepository));
        private readonly IDbContextFactory<ModelAgencyContext> _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        private readonly IContractRepository _contractRepository = contractRepository ?? throw new ArgumentNullException(nameof(contractRepository));

        //   [Authorize(Policy = "ManagementPolicyLevel5")]
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

        // [Authorize(Policy = "ManagementPolicyLevel5")]
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

        // [Authorize(Policy = "ManagementPolicyLevel5")]
        [SwaggerOperation("Processo de gerar contrato de Agenciamento e Fotografia.")]
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

                    return ActionResultIndex.Failure(
                        $"OS DADOS FORNECIDOS SÃO INVÁLIDOS: {string.Join(", ", erros).ToUpper()}");
                }

                Console.WriteLine($"BUSCANDO DADOS DO MODELO {createContractModelRequest.ModelIdentification}");

                var model = await _modelRepository.GetModelByCriteriaAsync(createContractModelRequest.ModelIdentification);

                if (model == null)
                {
                    return ActionResultIndex.Failure("MODELO NÃO ENCONTRADO COM OS CRITÉRIOS FORNECIDOS.");
                }

                var existingContracts = await _contractRepository.ExistsContractForTodayAsync(model.IdModel);

                if (existingContracts.Any())
                {
                    return Unauthorized(new CustomResponse
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        Message = $"JÁ EXISTEM CONTRATO(S) GERADO(S) PARA ESTE MODELO HOJE. " +
                                  $"CASO PRECISE GERAR UM NOVO CONTRATO EXCLUA OS QUE FORAM GERADOS.",
                        Data = existingContracts.Select(c => new ContractResponse
                        {
                            StatusCode = StatusCodes.Status400BadRequest,
                            ContractFilePath = c.ContractFilePath,
                            CodProposta = c.CodProposta,
                            ValorContrato = c.ValorContrato,
                            DataContrato = c.DataContrato
                        })
                    });
                }

                var generatedContracts = await _contractService.GenerateAllContractsAsync(createContractModelRequest, model);

                if (generatedContracts == null || !generatedContracts.Any())
                {
                    return BadRequest(new CustomResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = $" ERRO AO GERAR CONTRATOS. CONSULTAR TIME DE TECNOLOGIA."
                    });
                }

                var response = new CustomResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = $"CONTRATOS GERADOS COM SUCESSO PARA {model.Name.ToUpper()}",
                    Data = generatedContracts.Select(c => new ContractResponse
                    {
                        ContractFilePath = c.ContractFilePath,
                        CodProposta = c.CodProposta,
                        ValorContrato = c.ValorContrato,
                        DataContrato = c.DataContrato

                    })
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERRO AO GERAR CONTRATOS: {ex.Message}");
                return ActionResultIndex.Failure(
                    $"OCORREU UM ERRO NA OPERAÇÃO: {ex.Message}",
                    isSpeakOnOperation: true);
            }
        }

        // [Authorize(Policy = "GlobalPoliticsAgency")]
        [HttpGet("download-contract")]
        public async Task<IActionResult> DownloadContractAsync(int proposalCode)
        {

            ContractDTO? contract = await contractRepository.DownloadContractAsync(proposalCode)
                                                            .ConfigureAwait(true);
            if (contract == null)
            {
                return NotFound("CONTRATO NÃO ENCONTRADO.");
            }

            string contentString = await _contractService.ConvertBytesToString(contract.Content)
                                                         .ConfigureAwait(true);
            byte[] pdfBytes;

            try
            {
                pdfBytes = await _contractService.ExtractBytesFromString(contentString)
                                                  .ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                return BadRequest($"ERRO AO PROCESSAR O CONTEÚDO DO CONTRATO: {ex.Message}");
            }

            return File(pdfBytes, "application/pdf", "contract.pdf");
        }

        // [Authorize(Policy = "ManagementPolicyLevel2")]
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

        // [Authorize(Policy = "PolicyProducers")]
        [HttpGet("all-contracts-producers")]
        public async Task<IActionResult> GetAllContractsForProducers()
        {
            try
            {
                var user = await _userService.UserCaptureByToken().ConfigureAwait(true);

                if (user.UserType != UserType.PRODUCERS)
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

        [HttpPost("delete-contract")]
        public async Task<IActionResult> ReceiveWebhook([FromBody] AutentiqueWebhookRequest request)
        {

            request.Event = "document.deleted";

            try
            {
                 var documentAutentique = await _autentiqueService.ProcessDocumentAutentiqueAsync(request.Data.Id);

                if (documentAutentique == null)
                {
                    return NotFound(new CustomResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = $"CONTRATO NÃO ENCONTRADO"
                    });
                }

                request.Data.Name = documentAutentique.DocumentName;

                await _autentiqueService.ProcessDeleteContractAsync(documentAutentique);
                var contract = await _contractRepository.GetContractByCriteriaAsync
                                    (documentAutentique.IdContract, documentAutentique.CodProposta, documentAutentique.IdModel);

                await _contractRepository.DeleteAsync(contract.ContractId, contract.CodProposta, contract.IdModel);

                return Ok(new CustomResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = $"CONTRATO {documentAutentique.DocumentName} EXCLUIDO"
                });

            }

            catch (Exception ex)
            {
                return BadRequest(new CustomResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = $"ERRO AO EXCLUIR CONTRATO{request.Data.Name}, ENTRE EM CONTATO O TIME DE T.I.",
                    Data = ex
                });
            }
        }
    }

}



