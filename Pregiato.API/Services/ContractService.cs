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
using Microsoft.Extensions.DependencyInjection;

namespace Pregiato.API.Services
{
    public class ContractService : IContractService
    {
        private readonly IContractRepository _contractRepository;
        private readonly IModelRepository _modelRepository;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IJwtService _jwtService;
        private readonly IPaymentService _paymentService;
        private readonly IConfiguration _configuration;
        private readonly IRabbitMQProducer _rabbitmqProducer;
        private readonly IBrowserService _browserService;

        public ContractService(
            IContractRepository contractRepository,
            IModelRepository modelRepository,
            IServiceScopeFactory scopeFactory,
            IJwtService jwtService,
            IPaymentService paymentService,
            IConfiguration configuration,
            IRabbitMQProducer rabbitMQProducer,
            IBrowserService browserService)
        {
            _contractRepository = contractRepository ?? throw new ArgumentNullException(nameof(contractRepository));
            _modelRepository = modelRepository ?? throw new ArgumentNullException(nameof(modelRepository));
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _configuration = configuration ?? throw new ArgumentException(nameof(configuration));
            _rabbitmqProducer = rabbitMQProducer ?? throw new ArgumentNullException(nameof(rabbitMQProducer));
            _browserService = browserService ?? throw new ArgumentNullException(nameof(browserService));
        }

        private static readonly string DefaultNomeEmpresa = "Pregiato Management";
        private static readonly string DefaultCNPJEmpresa = "34871424/0001-43";
        private static readonly string DefaultEnderecoEmpresa = "Rua Butantã";
        private static readonly string DefaultNumeroEmpresa = "468";
        private static readonly string DefaultComplementoEmpresa = "3º Andar";
        private static readonly string DefaultBairroEmpresa = "Pinheiros";
        private static readonly string DefaultCidadeEmpresa = "São Paulo";
        private static readonly string DefaultCEPEmpresa = "05424-000";
        private static readonly string DefaultDataContrato = DateTime.UtcNow.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        private static readonly string DefaultVigenciaContrato = DateTime.UtcNow.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        private static readonly string DefaultMesContrato = DateTime.UtcNow.ToString("MMMM", CultureInfo.InvariantCulture);

        public async Task<string> PopulateTemplate(string template, Dictionary<string, string> parameters)
        {
            if (string.IsNullOrWhiteSpace(template))
            {
                throw new ArgumentException("O template está vazio ou inválido.");
            }

            if (parameters == null || !parameters.Any())
            {
                throw new ArgumentException("Os parâmetros para preenchimento do template estão vazios ou nulos.");
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

            var pdfStream = await page.PdfStreamAsync(pdfOptions);

            using var memoryStream = new MemoryStream();
            await pdfStream.CopyToAsync(memoryStream);

            await page.CloseAsync();

            return memoryStream.ToArray();
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
            contract.ContractFilePath = $"CodigoProposta_{contract.CodProposta}_CPF:{cpfModelo}_{DateTime.UtcNow.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture)}_{contract.TemplateFileName}.pdf";

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ModelAgencyContext>();
            context.Contracts.Add(contract);
            await context.SaveChangesAsync();
        }

        private async Task<int> GetNextCodPropostaAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ModelAgencyContext>();
            var maxCodProposta = await context.Contracts.MaxAsync(c => (int?)c.CodProposta) ?? 109;
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
            contract.CodProposta = await GetNextCodPropostaAsync();

            // Usar TryGetValue para evitar dupla busca no dicionário
            contract.LocalContrato = parameters.TryGetValue("Cidade Modelo - UF", out var cidade) ? cidade : createContractModelRequest.City;
            contract.DataContrato = parameters.TryGetValue("Dia", out var dia) ? dia : createContractModelRequest.Day.ToString(CultureInfo.InvariantCulture);
            contract.MesContrato = parameters.TryGetValue("Mês-Contrato", out var mes) ? mes : createContractModelRequest.MonthContract.ToString(CultureInfo.InvariantCulture);
            if (!parameters.TryGetValue("Valor-Contrato", out var valorContrato))
            {
                throw new ArgumentException("A chave 'Valor-Contrato' é obrigatória.");
            }
            contract.ValorContrato = decimal.Parse(valorContrato.Replace("R$", "").Replace(".", "").Replace(",", ".").Trim(), CultureInfo.InvariantCulture);

            string htmlTemplatePath = $"Templates/{contract.TemplateFileName}";
            if (!File.Exists(htmlTemplatePath))
            {
                throw new FileNotFoundException($"Template não encontrado: {htmlTemplatePath}");
            }

            string htmlTemplate = await File.ReadAllTextAsync(htmlTemplatePath);

            string populatedHtml = await PopulateTemplate(htmlTemplate, parameters);

            byte[] pdfBytes = await ConvertHtmlToPdf(populatedHtml, parameters);

            await SaveContractAsync(contract, new MemoryStream(pdfBytes), parameters["CPF-Modelo"]);

            if (contractType == "Photography" || contractType == "PhotographyMinority")
            {
                var validationResult = await _paymentService.ValidatePayment(createContractModelRequest.Payment, contract);
            }

            return contract;
        }

        public async Task<List<ContractBase>> GenerateAllContractsAsync(CreateContractModelRequest createContractModelRequest, Model model)
        {
            var parameters = new Dictionary<string, string?>
            {
                {"Cidade", createContractModelRequest.City},
                {"Dia", createContractModelRequest.Day.ToString(CultureInfo.InvariantCulture)},
                {"UF-Local", createContractModelRequest.UFContract},
                {"Mês-Extenso", createContractModelRequest.Month},
                {"Ano", DateTime.Now.Year.ToString(CultureInfo.InvariantCulture)},
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
                {"Valor-Contrato", createContractModelRequest.Payment.Valor.ToString("N2", CultureInfo.InvariantCulture)},
                {"Meses-Contrato", createContractModelRequest.MonthContract.ToString(CultureInfo.InvariantCulture)},
                {"Nome-Assinatura", model.Name}
            };

            await AddMinorModelInfo(model, parameters);

            string templatePhotography = model.Age < 18 ? "PhotographyMinority" : "Photography";

            var contractTasks = new[]
            {
                GenerateContractAsync(createContractModelRequest, model.IdModel, templatePhotography, await AddSignatureToParameters(parameters, templatePhotography)),
                GenerateContractAsync(createContractModelRequest, model.IdModel, "Agency", await AddSignatureToParameters(parameters, "Agency"))
            };

            var contracts = (await Task.WhenAll(contractTasks)).ToList();

            return contracts;
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
                {"Numero-Empresa", DefaultNumeroEmpresa},
                {"Complemento-Empresa", DefaultComplementoEmpresa},
                {"Cidade-Empresa", DefaultCidadeEmpresa},
                {"Bairro-Empresa", DefaultBairroEmpresa},
                {"CEP-Empresa", DefaultCEPEmpresa},
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
                {"Data-Agendamento", createRequestContractImageRights.DataAgendamento.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)},
                {"Horário-Agendamento", createRequestContractImageRights.horaAgendamento},
                {"Valor-Cache", createRequestContractImageRights.ValorCache.ToString("C", CultureInfo.InvariantCulture)}
            };

            string contractType = "Commitment";
            ContractBase contract = contractType switch
            {
                "Commitment" => new CommitmentTerm(),
                _ => throw new ArgumentException("Invalid contract type.")
            };

            contract.ModelId = model.IdModel;
            contract.CodProposta = await GetNextCodPropostaAsync();

            // Usar TryGetValue para evitar dupla busca
            contract.LocalContrato = parameters.TryGetValue("Local-Contrato", out var local) ? local : DefaultCidadeEmpresa;
            contract.DataContrato = parameters.TryGetValue("Data-Contrato", out var data) ? data : DefaultDataContrato;
            contract.MesContrato = parameters.TryGetValue("Mês-Contrato", out var mesContrato) ? mesContrato : DefaultMesContrato;
            contract.NomeEmpresa = parameters.TryGetValue("Nome-Empresa", out var nomeEmpresa) ? nomeEmpresa : DefaultNomeEmpresa;
            contract.CNPJEmpresa = parameters.TryGetValue("CNPJ-Empresa", out var cnpj) ? cnpj : DefaultCNPJEmpresa;
            contract.EnderecoEmpresa = parameters.TryGetValue("Endereço-Empresa", out var endereco) ? endereco : DefaultEnderecoEmpresa;
            contract.NumeroEmpresa = parameters.TryGetValue("Numero-Empresa", out var numero) ? numero : DefaultNumeroEmpresa;
            contract.ComplementoEmpresa = parameters.TryGetValue("Complemento-Empresa", out var complemento) ? complemento : DefaultComplementoEmpresa;
            contract.BairroEmpresa = parameters.TryGetValue("Bairro-Empresa", out var bairro) ? bairro : DefaultBairroEmpresa;
            contract.CidadeEmpresa = parameters.TryGetValue("Cidade-Empresa", out var cidadeEmpresa) ? cidadeEmpresa : DefaultCidadeEmpresa;
            contract.CEPEmpresa = parameters.TryGetValue("CEP-Empresa", out var cep) ? cep : DefaultCEPEmpresa;
            contract.VigenciaContrato = parameters.TryGetValue("Vigência-Contrato", out var vigencia) ? vigencia : DefaultVigenciaContrato;
            contract.DataAgendamento = parameters.TryGetValue("Data-Agendamento", out var dataAgendamento) ? dataAgendamento : createRequestContractImageRights.DataAgendamento.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            contract.HorarioAgendamento = parameters.TryGetValue("Horário-Agendamento", out var horario) ? horario : createRequestContractImageRights.horaAgendamento;
            if (!parameters.TryGetValue("Valor-Cache", out var valorCache))
            {
                throw new ArgumentException("A chave 'Valor-Cache' é obrigatória.");
            }
            contract.ValorCache = decimal.Parse(valorCache.Replace("R$", "").Replace(".", "").Replace(",", ".").Trim(), CultureInfo.InvariantCulture);

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

        public async Task<ContractBase> GenerateContractImageRightsTerm(string querymodel)
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
                {"Numero-Empresa", DefaultNumeroEmpresa},
                {"Complemento-Empresa", DefaultComplementoEmpresa},
                {"Cidade-Empresa", DefaultCidadeEmpresa},
                {"Bairro-Empresa", DefaultBairroEmpresa},
                {"CEP-Empresa", DefaultCEPEmpresa},
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

            // Usar TryGetValue para evitar dupla busca
            contract.LocalContrato = parameters.TryGetValue("Local-Contrato", out var local) ? local : DefaultCidadeEmpresa;
            contract.DataContrato = parameters.TryGetValue("Data-Contrato", out var data) ? data : DefaultDataContrato;
            contract.MesContrato = parameters.TryGetValue("Mês-Contrato", out var mesContrato) ? mesContrato : DefaultMesContrato;
            contract.NomeEmpresa = parameters.TryGetValue("Nome-Empresa", out var nomeEmpresa) ? nomeEmpresa : DefaultNomeEmpresa;
            contract.CNPJEmpresa = parameters.TryGetValue("CNPJ-Empresa", out var cnpj) ? cnpj : DefaultCNPJEmpresa;
            contract.EnderecoEmpresa = parameters.TryGetValue("Endereço-Empresa", out var endereco) ? endereco : DefaultEnderecoEmpresa;
            contract.NumeroEmpresa = parameters.TryGetValue("Numero-Empresa", out var numero) ? numero : DefaultNumeroEmpresa;
            contract.ComplementoEmpresa = parameters.TryGetValue("Complemento-Empresa", out var complemento) ? complemento : DefaultComplementoEmpresa;
            contract.BairroEmpresa = parameters.TryGetValue("Bairro-Empresa", out var bairro) ? bairro : DefaultBairroEmpresa;
            contract.CidadeEmpresa = parameters.TryGetValue("Cidade-Empresa", out var cidadeEmpresa) ? cidadeEmpresa : DefaultCidadeEmpresa;
            contract.CEPEmpresa = parameters.TryGetValue("CEP-Empresa", out var cep) ? cep : DefaultCEPEmpresa;
            contract.VigenciaContrato = parameters.TryGetValue("Vigência-Contrato", out var vigencia) ? vigencia : DefaultVigenciaContrato;

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
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ModelAgencyContext>();
            return await context.Contracts
                .Where(c => c.ModelId == modelId)
                .Select(c => new ContractsModels
                {
                    ModelId = modelId,
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
                                    .Select(b => byte.Parse(b.Trim(), CultureInfo.InvariantCulture))
                                    .ToArray();

            return await Task.FromResult(bytes);
        }

        public async Task<string> ConvertBytesToString(byte[] bytes)
        {
            return await Task.FromResult(Encoding.UTF8.GetString(bytes));
        }
    }
}