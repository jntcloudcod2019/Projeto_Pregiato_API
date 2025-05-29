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
    public class ContractService(IProducersRepository producersRepository,
           IContractRepository contractRepository,
           IModelRepository modelRepository,
           IPaymentService paymentSerice,
           IConfiguration configuration,
           IRabbitMQProducer rabbitMQProducer,
           IBrowserService browserService,
           IJwtService jwtService,
           IDbContextFactory<ModelAgencyContext> contextFactory,
           IUserRepository userRepository) : IContractService
    {
        private readonly IContractRepository _contractRepository = contractRepository;
        private readonly IModelRepository _modelRepository = modelRepository;
        private readonly IJwtService _jwtService = jwtService;
        private readonly IPaymentService _paymentService = paymentSerice;
        private readonly IConfiguration _configuration = configuration ?? throw new ArgumentException(nameof(configuration));
        private readonly IRabbitMQProducer _rabbitmqProducer = rabbitMQProducer;
        private readonly IBrowserService _browserService = browserService;
        private readonly IDbContextFactory<ModelAgencyContext> _contextFactory = contextFactory;
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IProducersRepository _producersRepository = producersRepository;
        private static readonly string DefaulTemplateAgency = "Agency";
        private static readonly string DefaulTemplatePhotography = "Photography";
        private static readonly string DefaulTemplateAgencyMinority = "AgencyMinority";
        private static readonly string DefaulTemplatePhotographyMinority = "PhotographyMinority";

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

            List<ContractBase> listContracts = [];

            Dictionary<string, string> signaturePhotographyParams = await AddSignatureToParameters(parameters, DefaulTemplatePhotography);

            ContractBase photographyContract = await GenerateContractAsync(
                createContractModelRequest,
                model,
                DefaulTemplatePhotography,
                signaturePhotographyParams
            ).ConfigureAwait(true);

            listContracts.Add(photographyContract);

            Dictionary<string, string> signatureAgencyParams = await AddSignatureToParameters(parameters, DefaulTemplateAgency);
            ContractBase agencyContract = await GenerateContractAsync(
                createContractModelRequest,
                model,
                DefaulTemplateAgency,
                signatureAgencyParams
            ).ConfigureAwait(true);

            listContracts.Add(agencyContract);

            if (listContracts.Any(c => c.TemplateFileName is "AgencyContract.html" or
                                                              "PhotographyProductionContract.html"))
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
        public async Task<ContractCommitmentTerm> GenerateContractCommitmentTerm(CreateRequestCommitmentTerm requestContractCommitmentTerm, Model model)
        {
            string contractType = "Commitment";
            ContractBase contract = contractType switch
            {
                "Commitment" => new CommitmentTerm(),
                _ => throw new ArgumentException("Invalid contract type.")
            };

            Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                {"Cidade", requestContractCommitmentTerm.CityContract},
                {"UF",requestContractCommitmentTerm.UFContract},
                {"Dia", requestContractCommitmentTerm.Day.ToString()},
                {"Mês-Extenso", requestContractCommitmentTerm.Month},
                {"Ano", DateTime.Now.Year.ToString()},
                {"Nome do Modelo", model.Name},
                {"CPF Modelo", model.CPF },
                {"Endereço do Modelo", model.Address},
                {"Número Modelo", model.NumberAddress},
                {"Complemento Modelo", model.Complement},
                {"Bairro Modelo", model.Neighborhood},
                {"Cidade Modelo", model.City},
                {"UF Modelo", model.UF },
                {"CEP Modelo", model.PostalCode},
                {"Telefone Principal", model.TelefonePrincipal},
                {"Telefone Secundário", model.TelefoneSecundario},
                {"Nome da Marca", requestContractCommitmentTerm.Mark},
                {"Data da Atividade",requestContractCommitmentTerm.DatOfActivity.ToString()},
                {"Horário do Job", requestContractCommitmentTerm.AppointmentTime.ToString()},
                {"Endereço do Local do Trabalho", requestContractCommitmentTerm.Locality},
                {"Valor Total", requestContractCommitmentTerm.GrossCash.ToString("N2", new CultureInfo("pt-BR"))},
                {"Valor Líquido", requestContractCommitmentTerm.NetCacheModel.ToString("N2", new CultureInfo("pt-BR"))},
                {"Forma de Pagamento", requestContractCommitmentTerm.PaymentMethod.ToString()},
                {"Nome-Assinatura", model.Name}

            };

            string htmlTemplatePath = $"Templates/{contract.TemplateFileName}";
            if (!File.Exists(htmlTemplatePath))
            {
                throw new FileNotFoundException($"Template não encontrado: {htmlTemplatePath}");
            }

            string htmlTemplate = await File.ReadAllTextAsync(htmlTemplatePath);

            string populatedHtml = await PopulateTemplate(htmlTemplate, parameters);

            await AddSignatureToParameters(parameters, DefaulTemplateAgency);

            byte[] pdfBytes = await ConvertHtmlToPdf(populatedHtml, parameters);

            var commitmentTer = await SaveContractCommitmentTer(requestContractCommitmentTerm, new MemoryStream(pdfBytes), model);

            await _rabbitmqProducer.SendCommitmentTerm(commitmentTer);

            return commitmentTer;
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
               // {"Vigência-Contrato", DefaultVigenciaContrato}
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
        public async Task<List<ContractBase>> GenereteContractAgencyPhotographyMinorityAsync
            (GenerateContractsMinorityRequest generateContractsMinorityRequest, Model model)
        {

            Dictionary<string, string> parameters = new Dictionary<string, string?>
            {
                {"Cidade", generateContractsMinorityRequest.City},
                {"Dia", generateContractsMinorityRequest.Day.ToString()},
                {"UF-Local", generateContractsMinorityRequest.UFContract},
                {"Mês-Extenso", generateContractsMinorityRequest.Month},
                {"Ano", DateTime.Now.Year.ToString()},
                {"Nome-Responsavel", generateContractsMinorityRequest.NameResponsible},
                {"CPF-Responsavel", generateContractsMinorityRequest.CPFResponsible},
                {"Endereço-Modelo", model.Address},
                {"Número-Residência", model.NumberAddress},
                {"Complemento-Modelo", model.Complement},
                {"Bairro-Modelo", model.Neighborhood},
                {"Cidade-Modelo", model.City},
                {"UF", model.UF},
                {"CEP-Modelo", model.PostalCode},
                {"Telefone-Principal", model.TelefonePrincipal},
                {"Telefone-Secundário", model.TelefoneSecundario},
                {"Valor-Contrato",generateContractsMinorityRequest.Payment.Valor.ToString("N2", new CultureInfo("pt-BR"))},
                {"Meses-Contrato", generateContractsMinorityRequest.MonthContract.ToString()},
                {"Nome-Assinatura", model.Name}
            };

            await AddMinorModelInfo(model, parameters);

            List<ContractBase> listContracts = [];

            Dictionary<string, string> signaturePhotographyParams = await AddSignatureToParameters(parameters, DefaulTemplateAgency);

            ContractBase photographyContract = await GenerateContractAsyncMinorityAsync(
              generateContractsMinorityRequest,
              model,
              DefaulTemplatePhotographyMinority,
              signaturePhotographyParams
          ).ConfigureAwait(true);

            listContracts.Add(photographyContract);

            Dictionary<string, string> signatureAgencyParams = await AddSignatureToParameters(parameters, DefaulTemplateAgency);

            ContractBase agencyContract = await GenerateContractAsyncMinorityAsync(
                generateContractsMinorityRequest,
                model,
                DefaulTemplateAgencyMinority,
                signatureAgencyParams
            ).ConfigureAwait(true);

            listContracts.Add(agencyContract);

            if (listContracts.Any(c => c.TemplateFileName is "AgencyContractMinority.html" or
                                                              "PhotographyProductionContractMinority.html"))
            {
                await _rabbitmqProducer.SendMensage(listContracts, model.CPF);
            }

            return listContracts;
        }
        public async Task<ContractBase> GenerateContractAsyncMinorityAsync
            (GenerateContractsMinorityRequest generateContractsMinorityRequest,Model model, string contractType, Dictionary<string, string> parameters)
        {
            parameters ??= new Dictionary<string, string>();

            ContractBase contract = contractType switch
            {
                "AgencyMinority" => new AgencyContractMinority(),
                "PhotographyMinority" => new PhotographyProductionContractMinority(),
                _ => throw new ArgumentException("Invalid contract type.")
            };

            string htmlTemplatePath = $"Templates/{contract.TemplateFileName}";

            if (!File.Exists(htmlTemplatePath))
            {
                throw new FileNotFoundException($"Template não encontrado: {htmlTemplatePath}");
            }

            string htmlTemplate = await File.ReadAllTextAsync(htmlTemplatePath);

            string populatedHtml = await PopulateTemplate(htmlTemplate, parameters);

            byte[] pdfBytes = await ConvertHtmlToPdf(populatedHtml, parameters);

            contract.CodProposta = await GetNextCodPropostaAsync();

            CreateContractModelRequest createContractModel = new ();
            createContractModel.NameProducers = generateContractsMinorityRequest.NameProducers;
            createContractModel.ModelIdentification = generateContractsMinorityRequest.ModelIdentification;
            createContractModel.Payment = generateContractsMinorityRequest?.Payment;
            createContractModel.Day = generateContractsMinorityRequest.Day;
            createContractModel.Month = generateContractsMinorityRequest?.Month;
            createContractModel.MonthContract = generateContractsMinorityRequest.MonthContract;
            createContractModel.UFContract = generateContractsMinorityRequest?.UFContract;
            createContractModel.City = generateContractsMinorityRequest?.City;

            if (contractType == DefaulTemplatePhotographyMinority)
            {
                ContractWithProducers contractWithProducers = await DefineContractAsync
                                   (contract, createContractModel, model, contractType);

                await _paymentService.ValidatePayment(contractWithProducers.Producers, createContractModel.Payment, contract);
            }

            if (contractType == DefaulTemplateAgencyMinority)
            {

                await DefineContractAgencyAsync(contract, createContractModel, model, contractType);
            }
            await SaveContractAsync(contract, new MemoryStream(pdfBytes), model);
            return contract;
        }

        public async Task<ContractCommitmentTerm> SaveContractCommitmentTer(CreateRequestCommitmentTerm contractCommitment, Stream pdfStream, Model model)
        {
            var contract = new ContractCommitmentTerm()
            {
                IDcontract = Guid.NewGuid(),
                IDModel = model.IdModel,
                CpfModel = model.CPF,
                NameModel = model.Name,
                Mark = contractCommitment.Mark,
                DatOfActivity = contractCommitment.DatOfActivity.HasValue
                ? DateTime.SpecifyKind(contractCommitment.DatOfActivity.Value,
                DateTimeKind.Utc) : (DateTime?)null,
                AppointmentTime = contractCommitment.AppointmentTime,
                Locality = contractCommitment.Locality,
                GrossCash = contractCommitment.GrossCash,
                NetCacheModel = contractCommitment.NetCacheModel,
                PaymentMethod = contractCommitment.PaymentMethod,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

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
            contract.ContractFilePath = $"Termo_de_Comprometimento_:{model.Name}";

            await _contractRepository.SaveCommitmentTermAsync(contract);

            return contract;
        }
    };
}