using iText.Layout;
using Microsoft.EntityFrameworkCore;
using Pregiato.API.Data;
using Pregiato.API.Interface;
using Pregiato.API.Models;
using Pregiato.API.Requests;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text.Json;
using System.Text;
using PuppeteerSharp.Media;
using PuppeteerSharp;
using Pregiato.API.Interfaces;
using System.Diagnostics.Contracts;
using Pregiato.API.Response;
using System.Net.NetworkInformation;

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

            foreach (var param in parameters)
            {
                template = template.Replace($"<span class=\"highlight\">{{{param.Key}}}</span>", param.Value);
                template = template.Replace($"{{{param.Key}}}", param.Value);
            }

            return await Task.FromResult(template);
        }
        public async Task<byte[]> ConvertHtmlToPdf(string populatedHtml, Dictionary<string, string> parameters)
        {

            try
            {
               
                var browser = await _browserService.GetBrowserAsync();

                await using var page = await browser.NewPageAsync();

                await page.SetContentAsync(populatedHtml, new NavigationOptions
                {
                    WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
                });

                var pdfOptions = new PdfOptions
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

                return await page.PdfDataAsync(pdfOptions);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Erro ao converter HTML para PDF: {ex.Message}");
                throw;
            }
        }
        public async Task SaveContractAsync(ContractBase contract, Stream pdfStream, string cpfModelo)
        {
            using var memoryStream = new MemoryStream();
            await pdfStream.CopyToAsync(memoryStream);
            var pdfBytes = memoryStream.ToArray();

            var jsonObject = new
            {
                type = "Buffer",
                data = pdfBytes.Select(b => (int)b).ToArray()
            };

            var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(jsonObject);

            contract.Content = jsonBytes;
            contract.ContractFilePath = $"Model_CPF:{cpfModelo}_{DateTime.UtcNow:dd-MM-yyyy}.pdf";
            await _contractRepository.SaveContractAsync(contract);         
        }

        private async Task<int> GetNextCodPropostaAsync()
        {
            var maxCodProposta = await _contextFactory.CreateDbContext().Contracts.MaxAsync(c => (int?)c.CodProposta) ?? 109;
            return maxCodProposta + 1;
        }

        public async Task<ContractBase> GenerateContractAsync(CreateContractModelRequest createContractModelRequest, Guid modelId, string contractType, Dictionary<string, string> parameters)
        {
            parameters ??= new Dictionary<string, string>();

            ContractBase contract = contractType switch
            {
                "Agency" => new AgencyContract(),
                "Photography" => new PhotographyProductionContract(),
                "PhotographyMinority" => new PhotographyProductionContractMinority(),
                _ => throw new ArgumentException("Invalid contract type.")
            };

            contract.ModelId = modelId;
            contract.ContractId = Guid.NewGuid();
            contract.CodProposta = await GetNextCodPropostaAsync();
            if (!parameters.TryGetValue("Valor-Contrato", out var valorContrato))
            {
                throw new ArgumentException("A chave 'Valor-Contrato' é obrigatória.");
            }

            contract.ValorContrato = decimal.Parse(valorContrato.Replace("R$", "").Replace(".", "").Replace(",", ".").Trim(), CultureInfo.InvariantCulture);
            contract.FormaPagamento = createContractModelRequest.Payment.MetodoPagamento;
            contract.StatusPagamento = createContractModelRequest.Payment.StatusPagamento;

            string htmlTemplatePath = $"Templates/{contract.TemplateFileName}";

            if (!File.Exists(htmlTemplatePath))
            {
                throw new FileNotFoundException($"Template não encontrado: {htmlTemplatePath}");
            }

            string htmlTemplate = await File.ReadAllTextAsync(htmlTemplatePath);

            string populatedHtml = await PopulateTemplate(htmlTemplate, parameters);

            byte[] pdfBytes = await ConvertHtmlToPdf(populatedHtml, parameters);

            await SaveContractAsync(contract, new MemoryStream(pdfBytes), parameters["CPF-Modelo"]);

            Producers producers; 
            using (var pdfStream = new MemoryStream(pdfBytes))
            {
                producers = await ProcessProducersAsync(contract, pdfStream, parameters["CPF-Modelo"]);
            }

            if (contractType == DefaulTemplatePhotography || contractType == DefaulTemplatePhotographyMinority)
            {
                var validationResult = await _paymentService.ValidatePayment(producers, createContractModelRequest.Payment, contract);
            }
            return contract;
        }

        public async Task<List<ContractBase>> GenerateAllContractsAsync(CreateContractModelRequest createContractModelRequest, Model model)
        {

            var parameters = new Dictionary<string, string?>
            {
                {"Cidade", createContractModelRequest.City},
                {"Dia", createContractModelRequest.Day.ToString()},
                {"UF-Local", createContractModelRequest.UFContract},
                {"Mês-Extenso", createContractModelRequest.Month},
                {"Ano", DateTime.Now.Year.ToString()},
                {"Nome-Modelo", model.Name},
                {"CPF-Modelo", model.CPF},
                {"RG-Modelo", model.RG},
                {"Endereço-Modelo", model.Address},
                {"Número-Residência", model.NumberAddress},
                {"Complemento-Modelo", model.Complement},
                {"Bairro-Modelo", model.Neighborhood},
                {"Cidade-Modelo", model.City},
                {"UF", model.UF},
                {"CEP-Modelo", model.PostalCode},
                {"Telefone-Principal", model.TelefonePrincipal},
                {"Telefone-Secundário", model.TelefoneSecundario},
                {"Valor-Contrato",createContractModelRequest.Payment.Valor.ToString("N2", new CultureInfo("pt-BR"))},
                {"Meses-Contrato", createContractModelRequest.MonthContract.ToString()},
                {"Nome-Assinatura", model.Name}
            };

            await AddMinorModelInfo(model, parameters);

            var listContracts = new List<ContractBase>();

            var template = model.Age < 18 ? DefaulTemplatePhotographyMinority : DefaulTemplatePhotography;
                                  
            var signaturePhotographyParams = await AddSignatureToParameters(parameters, template);
            
            var photographyContract = await GenerateContractAsync(
                createContractModelRequest,
                model.IdModel,
                template,
                signaturePhotographyParams
            );
           
            listContracts.Add(photographyContract);

            var signatureAgencyParams = await AddSignatureToParameters(parameters,  DefaulTemplateAgency);
            var agencyContract = await GenerateContractAsync(
                createContractModelRequest,
                model.IdModel,
                DefaulTemplateAgency,
                signatureAgencyParams
            );

            if (template == DefaulTemplateAgency)
            {
                await _rabbitmqProducer.SendMensage(listContracts, model.CPF);
            }
            
            listContracts.Add(agencyContract);
        
            return listContracts;
        }
        public async Task<Dictionary<string, string>> AddSignatureToParameters(Dictionary<string, string> parameters, string contractType)
        {
            var updatedParameters = new Dictionary<string, string>(parameters);

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
            var model = await _modelRepository.GetModelByCriteriaAsync(querymodel);

            if (model == null)
            {
                throw new FileNotFoundException($"Modelo não encontrado:{querymodel}");
            }

            var parameters = new Dictionary<string, string>
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

            contract.ModelId = model.IdModel;
           
            string htmlTemplatePath = $"Templates/{contract.TemplateFileName}";
            if (!File.Exists(htmlTemplatePath))
            {
                throw new FileNotFoundException($"Template não encontrado: {htmlTemplatePath}");
            }

            string htmlTemplate = await File.ReadAllTextAsync(htmlTemplatePath);

            string populatedHtml = await PopulateTemplate(htmlTemplate, parameters);

            byte[] pdfBytes = await ConvertHtmlToPdf(populatedHtml, parameters);

            await SaveContractAsync(contract, new MemoryStream(pdfBytes), model.CPF);

            return contract;
        }
        public async Task<ContractBase> GenetayeContractImageRightsTerm(string querymodel)
        {
            var model = await _modelRepository.GetModelByCriteriaAsync(querymodel);

            if (model == null)
            {
                throw new FileNotFoundException($"Modelo não encontrado:{querymodel}");
            }

            var parameters = new Dictionary<string, string>
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

            contract.ModelId = model.IdModel;
            contract.CodProposta = await GetNextCodPropostaAsync();
           
            string htmlTemplatePath = $"Templates/{contract.TemplateFileName}";
            if (!File.Exists(htmlTemplatePath))
            {
                throw new FileNotFoundException($"Template não encontrado: {htmlTemplatePath}");
            }

            string htmlTemplate = await File.ReadAllTextAsync(htmlTemplatePath);

            string populatedHtml = await PopulateTemplate(htmlTemplate, parameters);

            byte[] pdfBytes = await ConvertHtmlToPdf(populatedHtml, parameters);

            await SaveContractAsync(contract, new MemoryStream(pdfBytes), parameters["CPF-Modelo"]);

            return contract;
        }
        public async Task<IActionResult> GetMyContracts(string type = "files")
        {
            var username = await _jwtService.GetAuthenticatedUsernameAsync();
            if (string.IsNullOrEmpty(username))
            {
                return new UnauthorizedResult();
            }

            var modelId = await _modelRepository.GetModelByCriteriaAsync(username);
            if (modelId == null)
            {
                return new NotFoundObjectResult($"Nenhum modelo encontrado para o usuário: {username}");
            }

            var contracts = await _contractRepository.GetContractsByModelId(modelId.IdModel);
            if (contracts == null || !contracts.Any())
            {
                return new NotFoundObjectResult("Nenhum contrato encontrado para o usuário.");
            }

            return type == "names"
                ? new OkObjectResult(contracts.Select(c => c.ContractFilePath).ToList())
                : new OkObjectResult(contracts.Select(c => new
                {
                    c.ModelId,
                    c.ContractFilePath,
                    ContentBase64 = c.Content != null ? Convert.ToBase64String(c.Content) : null
                }).ToList());
        }
        public async Task<List<ContractsModels>> GetContractsByModelIdAsync(Guid modelId)
        {

            using var context = _contextFactory.CreateDbContext();

            return await context.ContractsModels
                .Where(c => c.ModelId == modelId)
                .Select(c => new ContractsModels
                {
                    ModelId = c.ModelId,
                    ContractFilePath = c.ContractFilePath,
                    Content = c.Content
                })
                .ToListAsync();

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

        public async Task<Producers> ProcessProducersAsync(ContractBase contract, Stream pdfStream, string cpfmodel)
        {
             var context = _contextFactory.CreateDbContext();

            
              var userResult = await context.Models
             .AsNoTracking()
             .Where(m => m.CPF == cpfmodel)
             .Select(m => new
             {
                 Model = m,
                 User = context.Users.FirstOrDefault(u => u.CodProducers == m.CodProducers)
             })
             .FirstOrDefaultAsync();

            
            var defaultProducerCode = "PMSYSAPI01";
            var defaultProducerName = "PMSYSAPI01";

           
            var producers = new Producers
            {
               
                CodProducers = userResult?.User?.CodProducers ?? defaultProducerCode,
                NameProducer = userResult?.User?.Name ?? defaultProducerName,
                IdContract = Guid.NewGuid(),
                AmountContract = contract?.ValorContrato ?? 0,
                InfoModel = new DetailsInfo
                {
                    IdModel = contract?.ModelId ?? Guid.Empty,
                    NameModel = contract?.Model?.Name ?? string.Empty,
                    DocumentModel = cpfmodel ?? string.Empty
                },
                StatusContratc = Enums.StatusContratc.Ativo,
                ValidityContract = contract?.VigenciaContrato ?? string.Empty,
                CodProposal = contract?.CodProposta ?? 0,
                TotalAgreements = 1
            };

            contract.CodProducers = producers.CodProducers;
      
            await SaveContractAsync(contract, pdfStream, cpfmodel);
            await _producersRepository.SaveProducersAsync(producers);
            return producers;
        }
    }
}