using Microsoft.EntityFrameworkCore;
using Pregiato.API.Data;
using Pregiato.API.Models;
using Pregiato.API.Requests;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text.Json;
using System.Text;
using PuppeteerSharp.Media;
using PuppeteerSharp;
using Pregiato.API.Interfaces;
using Pregiato.API.Enums;
using Pregiato.API.Services.ServiceModels;

namespace Pregiato.API.Services
{
    public class ContractService : IContractService
    {
        private readonly IContractRepository _contractRepository;
        private readonly IModelRepository _modelRepository;
        private readonly IJwtService _jwtService;
        private readonly IPaymentService _paymentService;
        private readonly IConfiguration _configuration;
        private readonly IRabbitMQProducer _rabbitmqProducer;
        private readonly IBrowserService _browserService;
        private readonly IDbContextFactory<ModelAgencyContext> _contextFactory;
        private readonly IUserRepository _userRepository;
        private readonly IProducersRepository _producersRepository;
        public ContractService
               (IProducersRepository producersRepository,
               IContractRepository contractRepository,
               IModelRepository modelRepository,
               IPaymentService paymentSerice,
               IConfiguration configuration,
               IRabbitMQProducer rabbitMQProducer,
               IBrowserService browserService,
               IJwtService jwtService,
               IDbContextFactory<ModelAgencyContext> contextFactory,
               IUserRepository userRepository)
        {
            _contractRepository = contractRepository;
            _modelRepository = modelRepository;
            _jwtService = jwtService;
            _paymentService = paymentSerice;
            _configuration = configuration ?? throw new ArgumentException(nameof(configuration));
            _rabbitmqProducer = rabbitMQProducer;
            _browserService = browserService;
            _contextFactory = contextFactory;
            _userRepository = userRepository;
            _producersRepository = producersRepository;
        }


        private static readonly string DefaultNomeEmpresa = "Pregiato Management";
        private static readonly string DefaultCNPJEmpresa = "34871424/0001-43";
        private static readonly string DefaultEnderecoEmpresa = "Rua Butantã";
        private static readonly string DefaultNumeroEmpresa = "468";
        private static readonly string DefaultComplementoEmpresa = "3º Andar";
        private static readonly string DefaultBairroEmpresa = "Pinheiros";
        private static readonly string DefaultCidadeEmpresa = "São Paulo";
        private static readonly string DefaultCEPEmpresa = "05424-000";
        private static readonly string DefaultDataContrato = DateTime.UtcNow.ToString("dd/MM/yyyy");
        private static readonly string DefaultVigenciaContrato = DateTime.UtcNow.ToString("dd/MM/yyyy");
        private static readonly string DefaultMesContrato = DateTime.UtcNow.ToString("MMMM");
        private static readonly string DefaulTemplatePhotographyMinority = "PhotographyMinority";
        private static readonly string DefaulTemplatePhotography = "Photography";
        private static readonly string DefaulTemplateAgency = "Agency";

        public async Task<string> PopulateTemplate(string template, Dictionary<string, string> parameters)
        {
            if (string.IsNullOrWhiteSpace(template))
            {
                return ("O template está vazio ou inválido.");
            }

            if (parameters == null || !parameters.Any())
            {
               return ("Os parâmetros para preenchimento do template estão vazios ou nulos.");
            }

            StringBuilder stringBuilder = new StringBuilder(template);
            foreach (KeyValuePair<string, string> param in parameters)
            {
                stringBuilder.Replace($"<span class=\"highlight\">{{{param.Key}}}</span>", param.Value);
                stringBuilder.Replace($"{{{param.Key}}}", param.Value);
            }
            return await Task.FromResult(stringBuilder.ToString());
        }
        public async Task<byte[]> ConvertHtmlToPdf(string populatedHtml, Dictionary<string, string> parameters)
        {

            try
            {

                IBrowser browser = await _browserService.GetBrowserAsync().ConfigureAwait(true);

                await using IPage? page = await browser.NewPageAsync().ConfigureAwait(true);

                await page.SetContentAsync(populatedHtml, new NavigationOptions
                {
                    WaitUntil = [WaitUntilNavigation.Networkidle0]
                }).ConfigureAwait(true);

                PdfOptions pdfOptions = new PdfOptions
                {
                    Format = PaperFormat.A4,
                    PrintBackground = true,
                    MarginOptions = new MarginOptions
                    {
                        Top = "20px",
                        Bottom = "40px",
                        Left = "20px",
                        Right = "20px"
                    }
                };

                return await page.PdfDataAsync(pdfOptions).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync($"Erro ao converter HTML para PDF: {ex.Message}").ConfigureAwait(true);
                throw;
            }
        }
        public async Task SaveContractAsync(ContractBase contract, Stream pdfStream, Model model)
        {
            using MemoryStream memoryStream = new MemoryStream();

            await pdfStream.CopyToAsync(memoryStream).ConfigureAwait(true);

            byte[] pdfBytes = memoryStream.ToArray();

            var jsonObject = new
            {
                type = "Buffer",
                data = pdfBytes.Select(b => (int)b).ToArray()
            };

            byte[] jsonBytes = JsonSerializer.SerializeToUtf8Bytes(jsonObject);

            contract.Content = jsonBytes;

            string nameContract = contract.TemplateFileName.Contains('.')
                ? contract.TemplateFileName.Substring(0, contract.TemplateFileName.LastIndexOf('.'))
                : contract.TemplateFileName;

            contract.ContractFilePath = $"{model.Name}_CPF:{model.CPF}_{nameContract}_{DateTime.UtcNow:dd-MM-yyyy}.pdf";
            await _contractRepository.SaveContractAsync(contract);
        }
        private async Task<int> GetNextCodPropostaAsync()
        {
            int maxCodProposta = await _contextFactory.CreateDbContext().Contracts.MaxAsync(c => (int?)c.CodProposta) ?? 109;
            return maxCodProposta + 1;
        }
        public async Task<ContractBase>  GenerateContractAsync
                    (CreateContractModelRequest createContractModelRequest, Model model, string contractType, Dictionary<string, string> parameters)
        {
            parameters ??= new Dictionary<string, string>();

            ContractBase contract = contractType switch
            {
                "Agency" => new AgencyContract(),
                "Photography" => new PhotographyProductionContract(),
                "PhotographyMinority" => new PhotographyProductionContractMinority(),
                _ => throw new ArgumentException("Invalid contract type.")
            };

            string htmlTemplatePath = $"Templates/{contract.TemplateFileName}";

            if (!File.Exists(htmlTemplatePath))
            {
                throw new FileNotFoundException($"Template não encontrado: {htmlTemplatePath}");
            }

            string htmlTemplate = await File.ReadAllTextAsync(htmlTemplatePath).ConfigureAwait(true);

            string populatedHtml = await PopulateTemplate(htmlTemplate, parameters).ConfigureAwait(true);

            byte[] pdfBytes = await ConvertHtmlToPdf(populatedHtml, parameters).ConfigureAwait(true);

            contract.CodProposta = await GetNextCodPropostaAsync().ConfigureAwait(true);

            if (contractType == DefaulTemplatePhotography || contractType == DefaulTemplatePhotographyMinority)
            {ContractWithProducers contractWithProducers = await DefineContractAsync
                                   (contract, createContractModelRequest, model, contractType).ConfigureAwait(true);

                await _paymentService.ValidatePayment(contractWithProducers.Producers, createContractModelRequest.Payment, contract);
            }

            if (contractType == DefaulTemplateAgency)
            {

                await DefineContractAgencyAsync(contract, createContractModelRequest, model, contractType)
                      .ConfigureAwait(true);
            }

            await SaveContractAsync(contract, new MemoryStream(pdfBytes), model);

            return contract;
        }
        public async Task<List<ContractBase>> GenerateAllContractsAsync(CreateContractModelRequest createContractModelRequest, Model model)
        {

            Dictionary<string, string> parameters = new Dictionary<string, string?>
            {
                {"Cidade", createContractModelRequest.City}, {"Dia", createContractModelRequest.Day.ToString()},
                {"UF-Local", createContractModelRequest.UFContract}, {"Mês-Extenso", createContractModelRequest.Month},
                {"Ano", DateTime.Now.Year.ToString()}, {"Nome-Modelo", model.Name}, {"CPF-Modelo", model.CPF},
                {"Endereço-Modelo", model.Address}, {"Número-Residência", model.NumberAddress},
                {"Complemento-Modelo", model.Complement}, {"Bairro-Modelo", model.Neighborhood},
                {"Cidade-Modelo", model.City}, {"UF", model.UF}, {"CEP-Modelo", model.PostalCode},
                {"Telefone-Principal", model.TelefonePrincipal}, {"Telefone-Secundário", model.TelefoneSecundario},
                {"Valor-Contrato",createContractModelRequest.Payment.Valor.ToString("N2", new CultureInfo("pt-BR"))},
                {"Meses-Contrato", createContractModelRequest.MonthContract.ToString()}, {"Nome-Assinatura", model.Name}
            };

            await AddMinorModelInfo(model, parameters).ConfigureAwait(false);

            List<ContractBase> listContracts = [];

            string template = model.Age < 18 ? DefaulTemplatePhotographyMinority : DefaulTemplatePhotography;

            Dictionary<string, string> signaturePhotographyParams = await AddSignatureToParameters(parameters, template).ConfigureAwait(false);

            ContractBase photographyContract = await GenerateContractAsync(
                createContractModelRequest,
                model,
                template,
                signaturePhotographyParams
            ).ConfigureAwait(true);

            listContracts.Add(photographyContract);

            Dictionary<string, string> signatureAgencyParams = await AddSignatureToParameters(parameters,  DefaulTemplateAgency).ConfigureAwait(false);
            ContractBase agencyContract = await GenerateContractAsync(
                createContractModelRequest,
                model,
                DefaulTemplateAgency,
                signatureAgencyParams
            ).ConfigureAwait(true);

            listContracts.Add(agencyContract);

            if (listContracts.Any(c => c.TemplateFileName is "AgencyContract.html" or
                                                              "PhotographyProductionContract.html" or
                                                              "PhotographyProductionContractMinority.html"))
            {
                await _rabbitmqProducer.SendMensage(listContracts, model.CPF);
            }

            return listContracts;
        }
        public async Task<Dictionary<string, string>> AddSignatureToParameters(Dictionary<string, string> parameters, string contractType)
        {
            Dictionary<string, string> updatedParameters = new Dictionary<string, string>(parameters);

            string imageName = _configuration[$"Signatures:{contractType}"];

            string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", imageName);

            if (!File.Exists(imagePath))
            {
                throw new FileNotFoundException($"Imagem da assinatura não encontrada para o contrato {contractType}.", imagePath);
            }

            byte[] imageBytes = await File.ReadAllBytesAsync(imagePath);
            string imageBase64 = Convert.ToBase64String(imageBytes);

            updatedParameters["NameImageSignature"] = $"data:image/png;base64,{imageBase64}";

            return updatedParameters;
            return await Task.FromResult(updatedParameters);
        }
        public async Task AddMinorModelInfo(Model model, Dictionary<string, string> parameters)
        {
            if (model.Age < 18)
            {
                parameters.Add("Nome-Menor-Idade", model.Name);
                parameters.Add("CPF-Menor-Idade", model.CPF);
            }
            await Task.CompletedTask;
        }
        public async Task<ContractBase> GenerateContractCommitmentTerm(CreateRequestCommitmentTerm createRequestContractImageRights, string querymodel)
        {
            Model? model = await _modelRepository.GetModelByCriteriaAsync(querymodel);

            if (model == null)
            {
                throw new FileNotFoundException($"Modelo não encontrado:{querymodel}");
            }

            Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                {"Local-Contrato", DefaultCidadeEmpresa},
                {"Data-Contrato", DefaultDataContrato},
                {"Mês-Contrato", DefaultMesContrato},
                {"Nome-Empresa", DefaultNomeEmpresa},
                {"CNPJ-Empresa", DefaultCNPJEmpresa},
                {"Endereço-Empresa", DefaultEnderecoEmpresa},
                {"Numero-Empresa",DefaultNumeroEmpresa},
                {"Complemento-Empresa", DefaultComplementoEmpresa},
                {"Cidade-Empresa", DefaultCidadeEmpresa},
                {"Bairro-Empresa", DefaultBairroEmpresa},
                {"CEP-Empresa",DefaultCEPEmpresa},
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
                {"Data-Agendamento",createRequestContractImageRights.DataAgendamento.ToString()},
                {"Horário-Agendamento", createRequestContractImageRights.horaAgendamento},
                { "Valor-Cache", createRequestContractImageRights.ValorCache.ToString("C")}
            };

            string contractType = "Commitment";
            ContractBase contract = contractType switch
            {
                "Commitment" => new CommitmentTerm(),
                _ => throw new ArgumentException("Invalid contract type.")
            };

            contract.IdModel = model.IdModel;

            string htmlTemplatePath = $"Templates/{contract.TemplateFileName}";
            if (!File.Exists(htmlTemplatePath))
            {
                throw new FileNotFoundException($"Template não encontrado: {htmlTemplatePath}");
            }

            string htmlTemplate = await File.ReadAllTextAsync(htmlTemplatePath);

            string populatedHtml = await PopulateTemplate(htmlTemplate, parameters);

            byte[] pdfBytes = await ConvertHtmlToPdf(populatedHtml, parameters);

          //  await SaveContractAsync(contract, new MemoryStream(pdfBytes), model.CPF);

            return contract;
        }
        public async Task<ContractBase> GenetayeContractImageRightsTerm(string querymodel)
        {
            Model? model = await _modelRepository.GetModelByCriteriaAsync(querymodel);

            if (model == null)
            {
                throw new FileNotFoundException($"Modelo não encontrado:{querymodel}");
            }

            Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                {"Local-Contrato", DefaultCidadeEmpresa},
                {"Data-Contrato", DefaultDataContrato},
                {"Mês-Contrato", DefaultMesContrato},
                {"Nome-Empresa", DefaultNomeEmpresa},
                {"CNPJ-Empresa", DefaultCNPJEmpresa},
                {"Endereço-Empresa", DefaultEnderecoEmpresa},
                {"Numero-Empresa",DefaultNumeroEmpresa},
                {"Complemento-Empresa", DefaultComplementoEmpresa},
                {"Cidade-Empresa", DefaultCidadeEmpresa},
                {"Bairro-Empresa", DefaultBairroEmpresa},
                {"CEP-Empresa",DefaultCEPEmpresa},
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
                {"Vigência-Contrato", DefaultVigenciaContrato}
            };

            string contractType = "ImageRights";
            ContractBase contract = contractType switch
            {
                "ImageRights" => new ImageRightsTerm(),
                _ => throw new ArgumentException("Invalid contract type.")
            };

            contract.IdModel = model.IdModel;

            contract.CodProposta = await GetNextCodPropostaAsync().ConfigureAwait(true);

            string htmlTemplatePath = $"Templates/{contract.TemplateFileName}";

            if (!File.Exists(htmlTemplatePath))
            {
                throw new FileNotFoundException($"Template não encontrado: {htmlTemplatePath}");
            }

            string htmlTemplate = await File.ReadAllTextAsync(htmlTemplatePath).ConfigureAwait(true);

            string populatedHtml = await PopulateTemplate(htmlTemplate, parameters).ConfigureAwait(true);

            byte[] pdfBytes = await ConvertHtmlToPdf(populatedHtml, parameters).ConfigureAwait(true);

            await SaveContractAsync(contract, new MemoryStream(pdfBytes), model).ConfigureAwait(true);

            return contract;
        }
        public async Task<string?> GenerateProducerCodeContractAsync()
        {
            const string prefix = "PMCA";
            Random random = new();
            int randomNumber = random.Next(0, 999999);
            string code = $"{prefix}{randomNumber:000000}";
            return await Task.FromResult(code.ToString());
        }
        public async Task<IActionResult> GetMyContracts(string type = "files")
        {
            string username = await _jwtService.GetAuthenticatedUsernameAsync().ConfigureAwait(true);
            if (string.IsNullOrEmpty(username))
            {
                return new UnauthorizedResult();
            }

            Model? modelId = await _modelRepository.GetModelByCriteriaAsync(username).ConfigureAwait(true);
            if (modelId == null)
            {
                return new NotFoundObjectResult($"Nenhum modelo encontrado para o usuário: {username}");
            }

            List<ContractBase>? contracts = await _contractRepository.GetContractsByModelId(modelId.IdModel).ConfigureAwait(true);
            if (contracts == null || !contracts.Any())
            {
                return new NotFoundObjectResult("Nenhum contrato encontrado para o usuário.");
            }

            return type == "names"
                ? new OkObjectResult(contracts.Select(c => c.ContractFilePath).ToList())
                : new OkObjectResult(contracts.Select(c => new
                {
                    c.IdModel,
                    c.ContractFilePath,
                    ContentBase64 = c.Content != null ? Convert.ToBase64String(c.Content) : null
                }).ToList());
        }
        public async Task<List<ContractsModels>> GetContractsByModelIdAsync(Guid modelId)
        {
            using ModelAgencyContext context = _contextFactory.CreateDbContext();

            return await context.ContractsModels
                .Where(c => c.ModelId == modelId)
                .Select(c => new ContractsModels
                {
                    ModelId = c.ModelId,
                    ContractFilePath = c.ContractFilePath,
                    Content = c.Content
                })
                .ToListAsync().ConfigureAwait(true);

        }
        public async Task<byte[]> ExtractBytesFromString(string content)
        {
            int startIndex = content.IndexOf('[') + 1;
            int endIndex = content.LastIndexOf(']');

            string byteString = content.Substring(startIndex, endIndex - startIndex);

            byte[] bytes = byteString.Split(',')
                                     .Select(b => byte.Parse(b.Trim()))
                                     .ToArray();

            return await Task.FromResult(bytes);
        }
        public async Task<string> ConvertBytesToString(byte[] bytes)
        {
            return await Task.FromResult(Encoding.UTF8.GetString(bytes));
        }
        public async Task<Producers> ProcessProducersAsync(ContractBase contract, Model model )
        {
            using ModelAgencyContext context = _contextFactory.CreateDbContext();

            User? user = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.CodProducers == model.CodProducers);


            Producers producers = new Producers
            {
                CodProducers = model.CodProducers,
                NameProducer = user.Name,
                ContractId = contract.ContractId,

                AmountContract = contract?.ValorContrato ?? 0,
                InfoModel = new DetailsInfo
                {
                    IdModel = model.IdModel,
                    NameModel = model.Name,
                    DocumentModel = model.CPF
                },
                StatusContratc = Enums.StatusContratc.Ativo,
                CodProposal = contract.CodProposta ,
                TotalAgreements = 1
            };
            await _producersRepository.SaveProducersAsync(producers).ConfigureAwait(true);
            return producers;
        }
        public async Task<ContractWithProducers> DefineContractAsync
            (ContractBase contract, CreateContractModelRequest contractModelRequest, Model model, string contractType)
        {

            contract.IdModel = model.IdModel;
            contract.ContractId = Guid.NewGuid();
            string valorString = contractModelRequest.Payment.Valor != 0
                ? contractModelRequest.Payment.Valor.ToString(CultureInfo.InvariantCulture)
                : "0";

            contract.ValorContrato = decimal.Parse(
                valorString.Replace("R$", "")
                    .Replace(" ", "")
                    .Replace(".", "")
                    .Replace(",", "."),
                NumberStyles.Any,
                CultureInfo.InvariantCulture
            );

            contract.StatusPagamento = contractModelRequest.Payment.StatusPagamento;
            contract.PaymentId = Guid.NewGuid();
            contract.DataContrato = DateTime.UtcNow;
            contract.VigenciaContrato = DateTime.UtcNow;
            contract.FormaPagamento = contractModelRequest.Payment.MetodoPagamento;
            contract.StatusContratc = StatusContratc.Ativo;

            Producers producersProcess = await ProcessProducersAsync(contract, model).ConfigureAwait(true);

            contract.CodProducers = producersProcess.CodProducers;

            return new ContractWithProducers
            {
                Contract = contract,
                Producers = producersProcess
            };
        }
        public async Task<ContractBase> DefineContractAgencyAsync(ContractBase contract,
            CreateContractModelRequest contractModelRequest, Model model, string? contractType)
        {
            contract.IdModel = model.IdModel;
            contract.ContractId = Guid.NewGuid();
            string valorString = contractModelRequest.Payment.Valor != 0
                ? contractModelRequest.Payment.Valor.ToString(CultureInfo.InvariantCulture)
                : "0";

            contract.ValorContrato = decimal.Parse(
                valorString.Replace("R$", "")
                    .Replace(" ", "")
                    .Replace(".", "")
                    .Replace(",", "."),
                NumberStyles.Any,
                CultureInfo.InvariantCulture
            );

            contract.StatusPagamento = contractModelRequest.Payment.StatusPagamento;
            contract.PaymentId = Guid.NewGuid();
            contract.DataContrato = DateTime.UtcNow;
            contract.VigenciaContrato = DateTime.UtcNow;
            contract.FormaPagamento = contractModelRequest.Payment.MetodoPagamento;
            contract.StatusContratc = StatusContratc.Ativo;
            contract.CodProducers = await GenerateProducerCodeContractAsync().ConfigureAwait(true);
            return contract;
        }
    }
}