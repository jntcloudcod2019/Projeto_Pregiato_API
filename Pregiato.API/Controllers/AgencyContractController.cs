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
using Pregiato.API.Enums;

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
                model.NumberAddress,
                model.BankAccount,
                model.PostalCode,
                model.Complement,
                model.TelefonePrincipal,
                model.TelefoneSecundario
            });
        }

        [SwaggerOperation("Processo de gerar Contrato da Agência.")]
        [HttpPost("generate/agency")]
        public async Task<IActionResult> GenerateAgencyContract([FromQuery] string query, [FromBody] ContractRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(query))
                {
                    return BadRequest("O parâmetro de busca (CPF, RG, Nome ou ID do Modelo) é obrigatório.");
                }

                var model = await _context.Models.FirstOrDefaultAsync(m =>
                    m.CPF == query || m.RG == query || m.Name.Contains(query) || m.IdModel.ToString() == query);

                if (model == null)
                {
                    return NotFound("Modelo não encontrado.");
                }

                var contract = await _contractService.GenerateContractAsync(model.IdModel, request.JobId, "Agency", new Dictionary<string, string>());

                return Ok(new
                {
                    Success = true,
                    Message = "Contrato gerado com sucesso.",
                    ContractId = contract.ContractId,
                    CodProposta = contract.CodProposta,
                    FilePath = contract.ContractFilePath
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"Erro ao gerar o contrato: {ex.Message}"
                });
            }
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
                    {"Local-Contrato", "São Paulo, SP"},
                    {"Data-Contrato", DateTime.Now.ToString("dd/MM/yyyy") },
                    {"Mês-Contrato", DateTime.Now.ToString("Mmmm")},
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
                    {"Nome-Empresa", "Pregiato management"},
                    {"CNPJ-Empresa", "34871424/0001-43"},
                    {"Endereço-Empresa", "Rua Butantã"},
                    {"Numero-Empresa","468"},
                    {"Complemento-Empresa", "3º Andar"},
                    {"Bairro-Empresa", "Pinheiros"},
                    {"CEP-Empresa","05424-000"}
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
                    {"Local-Contrato", "São Paulo, SP"},
                    {"Data-Contrato", DateTime.Now.ToString("dd/MM/yyyy") },
                    {"Mês-Contrato", DateTime.Now.ToString("Mmmm")},
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
                    {"Nome-Empresa", "Pregiato management"},
                    {"CNPJ-Empresa", "34871424/0001-43"},
                    {"Endereço-Empresa", "Rua Butantã"},
                    {"Numero-Empresa","468"},
                    {"Complemento-Empresa", "3º Andar"},
                    {"Bairro-Empresa", "Pinheiros"},
                    {"CEP-Empresa","05424-000"}
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
                    {"Local-Contrato", "São Paulo, SP"},
                    {"Data-Contrato", DateTime.Now.ToString("dd/MM/yyyy") },
                    {"Mês-Contrato", DateTime.Now.ToString("Mmmm")},
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
                    {"Nome-Empresa", "Pregiato management"},
                    {"CNPJ-Empresa", "34871424/0001-43"},
                    {"Endereço-Empresa", "Rua Butantã"},
                    {"Numero-Empresa","468"},
                    {"Complemento-Empresa", "3º Andar"},
                    {"Bairro-Empresa", "Pinheiros"},
                    {"CEP-Empresa","05424-000"}
            };

            var contract = await _contractService.GenerateContractAsync(model.IdModel, request.
                JobId, "ImageRights", parameters);

            return Ok(contract);
        }

        //[SwaggerOperation("Processo de gerar todos os contratos")]
        //[HttpPost("generate/all")]
        //public async Task<IActionResult> GenerateAllContractsAsync
        //([FromQuery] string? idModel = null, [FromQuery] string? cpf = null, 
        //[FromQuery] string? rg = null, [FromBody] ContractRequest? request = null)
        //{
        //    if (string.IsNullOrEmpty(idModel) && string.IsNullOrEmpty(cpf) && string.IsNullOrEmpty(rg))
        //    {
        //        return BadRequest("Pelo menos um dos parâmetros 'IdModel', 'CPF', ou 'RG' deve ser fornecido.");
        //    }
        //    var model = await _context.Models.FirstOrDefaultAsync(m =>
        //        (idModel != null && m.IdModel.ToString() == idModel) ||
        //        (cpf != null && m.CPF == cpf) ||
        //        (rg != null && m.RG == rg));

        //    if (model == null)
        //    {
        //        return NotFound("Modelo não encontrado.");
        //    }

        //    var parameters = new Dictionary<string, string>
        //    {
        //            {"Local-Contrato"," São Paulo"},
        //            {"Data-Contrato", DateTime.UtcNow.ToString("dd/MM/yyyy")},
        //            {"Mês-Contrato", DateTime.UtcNow.ToString("MMMM")},
        //            {"Nome-Modelo", model.Name },
        //            {"CPF-Modelo", model.CPF },
        //            {"RG-Modelo", model.RG },
        //            {"Endereço-Modelo", model.Address},
        //            {"Numero-Modelo",model.NumberAddress},
        //            {"Bairro-Modelo", model.Neighborhood},
        //            {"Cidade-Modelo", model.City},
        //            {"CEP-Modelo", model.PostalCode},
        //            {"Complemento-Modelo", model.Complement},
        //            {"Telefone-Principal", model.TelefonePrincipal},
        //            {"Telefone-Secundário", model.TelefoneSecundario},
        //            {"Nome-Empresa","Pregiato Management"},
        //            {"CNPJ-Empresa", "34871424/0001-43"},
        //            {"Endereço-Empresa","468"},
        //            {"Numero-Empresa","3 Andar"},
        //            {"Complemento-Empresa", "Pinheiros"},
        //            {"Cidade-Empresa", "São Paulo"},
        //            {"Bairro-Empresa",  "05424-000"},
        //            {"CEP-Empresa",DateTime.UtcNow.ToString("dd/MM/yyyy")},
        //            {"Vigência-Contrato", DateTime.UtcNow.ToString("MMMM")}
        //    };

        //    if (request?.JobId == null)
        //    {
        //        return BadRequest("O JobId é obrigatório para gerar os contratos.");
        //    }

        //    var contracts = await _contractService.GenerateAllContractsAsync(
        //      idModel: idModel,
        //      cpf: cpf,
        //      rg: rg,
        //      jobId: request.JobId
        //   );

        //    return Ok(contracts);
        //}


        [HttpPost("generate/all")]
        public async Task<IActionResult> GenerateAllContractsAsync(
        [FromQuery] string? idModel = null,
        [FromQuery] string? cpf = null,
        [FromQuery] string? rg = null,
        [FromBody] ContractRequest? request = null)
        {
            if (string.IsNullOrEmpty(idModel) && string.IsNullOrEmpty(cpf) && string.IsNullOrEmpty(rg))
            {
                return BadRequest("Pelo menos um dos parâmetros 'IdModel', 'CPF', ou 'RG' deve ser fornecido.");
            }

            var model = await _context.Models
                               .FirstOrDefaultAsync(m => (idModel != null && m.IdModel.ToString() == idModel) ||
                               (cpf != null && m.CPF == cpf) ||
                               (rg != null && m.RG == rg));

            if (model == null)
            {
                return NotFound("Modelo não encontrado.");
            }

            if (request?.Payments == null || !request.Payments.Any())
            {
                return BadRequest("Pelo menos um pagamento deve ser fornecido.");
            }
            decimal valorContrato = 0;
            var metodosPagamento = new List<string>();

            // Processar pagamentos e calcular o valor total
            foreach (var paymentRequest in request.Payments)
            {
                // Converte o PaymentRequest para Payment
                var payment = new Payment
                {
                    Id = Guid.NewGuid(),
                    ContractId = Guid.Empty, // Será atualizado após a geração dos contratos
                    Valor = paymentRequest.Valor,
                    QuantidadeParcela = paymentRequest.QuantidadeParcela,
                    FinalCartao = paymentRequest.FinalCartao,
                    DataPagamento = paymentRequest.DataPagamento ?? DateTime.UtcNow,
                    StatusPagamento = paymentRequest.StatusPagamento,
                    Comprovante = paymentRequest.Comprovante,
                    DataAcordoPagamento = paymentRequest.DataAcordoPagamento,
                    MetodoPagamento = paymentRequest.MetodoPagamento
                };

                // Validação do pagamento
                var validationResult = await  _paymentService.ValidatePayment(payment);
                if (validationResult == null)
                {
                    return BadRequest($"Erro no pagamento: {validationResult}");
                }

                valorContrato += payment.Valor;
                metodosPagamento.Add(payment.MetodoPagamento.ToString());

                // Salvar pagamentos no banco de dados
                var paymentEntity = new Payment
                {
                    Id = Guid.NewGuid(),
                    ContractId = Guid.Empty, // Será atualizado após a criação do contrato
                    Valor = payment.Valor,
                    QuantidadeParcela = payment.QuantidadeParcela,
                    FinalCartao = payment.FinalCartao,
                    DataPagamento = payment.DataPagamento ?? DateTime.UtcNow,
                    StatusPagamento = payment.StatusPagamento,
                    Comprovante = payment.Comprovante,
                    DataAcordoPagamento = payment.DataAcordoPagamento,
                    MetodoPagamento = payment.MetodoPagamento
                };

                await _context.AddAsync(paymentEntity);
            }

            // Preencher os parâmetros com base no modelo e no valor do contrato
            var parameters = new Dictionary<string, string>
            {
                {"Local-Contrato", "São Paulo"},
                {"Data-Contrato", DateTime.UtcNow.ToString("dd/MM/yyyy")},
                {"Mês-Contrato", DateTime.UtcNow.ToString("MMMM")},
                {"Nome-Modelo", model.Name},
                {"CPF-Modelo", model.CPF},
                {"RG-Modelo", model.RG},
                {"Endereço-Modelo", model.Address},
                {"Numero-Modelo", model.NumberAddress},
                {"Bairro-Modelo", model.Neighborhood},
                {"Cidade-Modelo", model.City},
                {"CEP-Modelo", model.PostalCode},
                {"Complemento-Modelo", model.Complement},
                {"Telefone-Principal", model.TelefonePrincipal},
                {"Telefone-Secundário", model.TelefoneSecundario},
                {"Nome-Empresa", "Pregiato Management"},
                {"CNPJ-Empresa", "34871424/0001-43"},
                {"Endereço-Empresa", "Rua Butantã"},
                {"Numero-Empresa", "468"},
                {"Complemento-Empresa", "3 Andar"},
                {"Cidade-Empresa", "São Paulo"},
                {"Bairro-Empresa", "Pinheiros"},
                {"CEP-Empresa", "05424-000"},
                {"Vigência-Contrato", DateTime.UtcNow.ToString("MMMM")},
                {"Valor-Contrato", valorContrato.ToString("C")},
                {"Forma-Pagamento",  string.Join(", ", metodosPagamento)}
            };

            if (request.JobId == null)
            {
                return BadRequest("O JobId é obrigatório para gerar os contratos.");
            }

            // Geração dos contratos
            var contracts = await _contractService.GenerateAllContractsAsync(
                idModel: idModel,
                cpf: cpf,
                rg: rg,
                jobId: request.JobId,
                parameters : parameters

            );

            // Atualiza o ID do contrato nos pagamentos e salva no banco
            foreach (var contract in contracts)
            {
                var relatedPayments = await _context.Payments.Where(p => p.ContractId == Guid.Empty).ToListAsync();
                foreach (var payment in relatedPayments)
                {
                    payment.ContractId = contract.ContractId;
                }
            }

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
