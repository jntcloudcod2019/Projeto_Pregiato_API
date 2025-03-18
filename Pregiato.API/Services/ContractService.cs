using iText.Layout;
using Microsoft.EntityFrameworkCore;
using Pregiato.API.Data;
using Pregiato.API.Interface;
using Pregiato.API.Models;
using Pregiato.API.Requests;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text.Json;
using SelectPdf;
using System.Text;
using System.Diagnostics.Contracts;
using iText.Commons.Actions.Contexts;

namespace Pregiato.API.Services
{
    public class ContractService : IContractService
    {
        private readonly IContractService _contractService;
        private readonly IContractRepository _contractRepository;
        private readonly IModelRepository _modelRepository;
        private readonly ModelAgencyContext _modelAgencyContext;
        private readonly IJwtService _jwtService;
        private readonly IPaymentService _paymentService;
        private readonly IConfiguration _configuration;
        private readonly IRabbitMQProducer _rabbitmqProducer;   

        public ContractService(IContractRepository contractRepository,
               IModelRepository modelRepository,
               ModelAgencyContext context,
               IJwtService jwtService,
               IPaymentService paymentSerice,
               IConfiguration configuration,
               IRabbitMQProducer rabbitMQProducer)
        {
            _contractRepository = contractRepository ?? throw new ArgumentNullException(nameof(context));
            _modelRepository = modelRepository;
            _modelAgencyContext = context ?? throw new ArgumentNullException(nameof(context));
            _jwtService = jwtService;
            _paymentService = paymentSerice;
            _configuration = configuration;
            _rabbitmqProducer = rabbitMQProducer;   
             
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
           
            return template;
        }

        public async Task<byte[]> ConvertHtmlToPdf(string populatedHtml, Dictionary<string, string> parameters)
        {

            HtmlToPdf converter = new HtmlToPdf();
            converter.Options.PdfPageSize = PdfPageSize.A4;
            converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;
            converter.Options.WebPageWidth = 1024;
            converter.Options.WebPageHeight = 0;
            converter.Options.MarginTop = 20;
            converter.Options.MarginBottom = 40;
            converter.Options.MarginLeft = 20;
            converter.Options.MarginRight = 20;

            Thread.Sleep(200); 

            SelectPdf.PdfDocument doc = converter.ConvertHtmlString(populatedHtml);

            using var memoryStream = new MemoryStream();
            doc.Save(memoryStream);
            doc.Close();

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

            contract.ContractFilePath = $"CodigoProposta_{contract.CodProposta}_CPF:{cpfModelo}_{DateTime.UtcNow:dd-MM-yyyy}_{contract.TemplateFileName}.pdf";
            await _contractRepository.SaveContractAsync(contract);
        }

        private async Task<int> GetNextCodPropostaAsync()
        {
            var maxCodProposta = await _modelAgencyContext.Contracts.MaxAsync(c => (int?)c.CodProposta) ?? 109;
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
            contract.LocalContrato = parameters.ContainsKey("Cidade Modelo - UF") ? parameters["Cidade Modelo - UF"] : createContractModelRequest.City;
            contract.DataContrato = parameters.ContainsKey("Dia") ? parameters["Dia"] : createContractModelRequest.Day.ToString();
            contract.MesContrato = parameters.ContainsKey("Mês-Contrato") ? parameters["Mês-Contrato"] : createContractModelRequest.MonthContract.ToString();
            contract.ValorContrato = parameters.ContainsKey("Valor-Contrato") ? decimal.Parse(parameters["Valor-Contrato"].Replace("R$", "").Replace(".", "").Replace(",", ".").Trim()) : throw new ArgumentException("A chave 'Valor-Contrato' é obrigatória.");
           
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
            { var validationResult = await _paymentService.ValidatePayment(createContractModelRequest.Payment, contract); }

            await _modelAgencyContext.SaveChangesAsync();

            return contract;
        }

        public async Task<List<ContractBase>> GenerateAllContractsAsync(CreateContractModelRequest createContractModelRequest)
        {
           
            var model = await _modelRepository.GetModelByCriteriaAsync(createContractModelRequest.ModelIdentification);

            if (model == null)
            {
                throw new KeyNotFoundException("Modelo não encontrado.");
            }
            
            var parameters = new Dictionary<string, string>
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
                {"Valor-Contrato", createContractModelRequest.Payment.Valor.ToString("N2", new CultureInfo("pt-BR"))},
                {"Meses-Contrato", createContractModelRequest.MonthContract.ToString()},
                {"Nome-Assinatura", model.Name}
            };
            
            AddMinorModelInfo(model, parameters);

            string templatePhotography = model.Age < 18 ? "PhotographyMinority" : "Photography";

            var contracts = new List<ContractBase>
            {
                await GenerateContractAsync(createContractModelRequest, model.IdModel, templatePhotography, AddSignatureToParameters(parameters, templatePhotography)),
                await GenerateContractAsync(createContractModelRequest, model.IdModel, "Agency", AddSignatureToParameters(parameters, "Agency"))
            };

            await _rabbitmqProducer.SendMensage(contracts, model.CPF);

            return contracts;
        }

        private Dictionary<string, string> AddSignatureToParameters(Dictionary<string, string> parameters, string contractType)
        {
            var updatedParameters = new Dictionary<string, string>(parameters);

            string imageName = _configuration[$"Signatures:{contractType}"];

            string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", imageName);

            if (!File.Exists(imagePath))
            {
                throw new FileNotFoundException($"Imagem da assinatura não encontrada para o contrato {contractType}.", imagePath);
            }

            updatedParameters["NameImageSignature"] = imagePath;
            return updatedParameters;
        }

        private void AddMinorModelInfo(Model model, Dictionary<string, string> parameters)
        {
            if (model.Age < 18)
            {
                parameters.Add("Nome-Menor-Idade", model.Name);
                parameters.Add("CPF-Menor-Idade", model.CPF);
            }
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
            contract.CodProposta = await GetNextCodPropostaAsync();
            contract.LocalContrato = parameters.ContainsKey("Local-Contrato") ? parameters["Local-Contrato"] : DefaultCidadeEmpresa;
            contract.DataContrato = parameters.ContainsKey("Data-Contrato") ? parameters["Data-Contrato"] : DefaultDataContrato;
            contract.MesContrato = parameters.ContainsKey("Mês-Contrato") ? parameters["Mês-Contrato"] : DefaultMesContrato;
            contract.NomeEmpresa = parameters.ContainsKey("Nome-Empresa}") ? parameters["Nome-Empresa}"] : DefaultNomeEmpresa;
            contract.CNPJEmpresa = parameters.ContainsKey("CNPJ-Empresa") ? parameters["CNPJ-Empresa"] : DefaultCNPJEmpresa;
            contract.EnderecoEmpresa = parameters.ContainsKey("Endereço-Empresa") ? parameters["Endereço-Empresa"] : DefaultEnderecoEmpresa;
            contract.NumeroEmpresa = parameters.ContainsKey("Numero-Empresa") ? parameters["Numero-Empresa"] : DefaultNumeroEmpresa;
            contract.ComplementoEmpresa = parameters.ContainsKey("Complemento-Empresa") ? parameters["Complemento-Empresa"] : DefaultComplementoEmpresa;
            contract.BairroEmpresa = parameters.ContainsKey("Bairro-Empresa") ? parameters["Bairro-Empresa"] : DefaultBairroEmpresa;
            contract.CidadeEmpresa = parameters.ContainsKey("Cidade-Empresa") ? parameters["Cidade-Empresa"] : DefaultCidadeEmpresa;
            contract.CEPEmpresa = parameters.ContainsKey("CEP-Empresa") ? parameters["CEP-Empresa"] : DefaultCEPEmpresa;
            contract.VigenciaContrato = parameters.ContainsKey("Vigência-Contrato") ? parameters["Vigência-Contrato"] : DefaultVigenciaContrato;
            contract.NomeEmpresa = parameters.ContainsValue("Nome-Empresa") ? parameters["Nome-Empresa"] : "Pregiato management";
            contract.DataAgendamento = parameters.ContainsKey("Data-Agendamento") ? parameters["Data-Agendamento"] : createRequestContractImageRights.DataAgendamento.ToString();
            contract.HorarioAgendamento = parameters.ContainsKey("Horário-Agendamento") ? parameters["Horário-Agendamento"] : createRequestContractImageRights.horaAgendamento;
            contract.ValorCache = parameters.ContainsKey("Valor-Cache") ? decimal.Parse(parameters["Valor-Cache"].Replace("R$", "")
            .Replace(".", "").Replace(",", ".").Trim()) : throw new ArgumentException("A chave 'Valor-Contrato' é obrigatória.");

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
            contract.LocalContrato = parameters.ContainsKey("Local-Contrato") ? parameters["Local-Contrato"] : DefaultCidadeEmpresa;
            contract.DataContrato = parameters.ContainsKey("Data-Contrato") ? parameters["Data-Contrato"] : DefaultDataContrato;
            contract.MesContrato = parameters.ContainsKey("Mês-Contrato") ? parameters["Mês-Contrato"] : DefaultMesContrato;
            contract.NomeEmpresa = parameters.ContainsKey("Nome-Empresa}") ? parameters["Nome-Empresa}"] : DefaultNomeEmpresa;
            contract.CNPJEmpresa = parameters.ContainsKey("CNPJ-Empresa") ? parameters["CNPJ-Empresa"] : DefaultCNPJEmpresa;
            contract.EnderecoEmpresa = parameters.ContainsKey("Endereço-Empresa") ? parameters["Endereço-Empresa"] : DefaultEnderecoEmpresa;
            contract.NumeroEmpresa = parameters.ContainsKey("Numero-Empresa") ? parameters["Numero-Empresa"] : DefaultNumeroEmpresa;
            contract.ComplementoEmpresa = parameters.ContainsKey("Complemento-Empresa") ? parameters["Complemento-Empresa"] : DefaultComplementoEmpresa;
            contract.BairroEmpresa = parameters.ContainsKey("Bairro-Empresa") ? parameters["Bairro-Empresa"] : DefaultBairroEmpresa;
            contract.CidadeEmpresa = parameters.ContainsKey("Cidade-Empresa") ? parameters["Cidade-Empresa"] : DefaultCidadeEmpresa;
            contract.CEPEmpresa = parameters.ContainsKey("CEP-Empresa") ? parameters["CEP-Empresa"] : DefaultCEPEmpresa;
            contract.VigenciaContrato = parameters.ContainsKey("Vigência-Contrato") ? parameters["Vigência-Contrato"] : DefaultVigenciaContrato;

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
            return await _modelAgencyContext.Contracts
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
                                     .Select(b => byte.Parse(b.Trim()))
                                     .ToArray();

            return bytes;
        }

        public async Task<string> ConvertBytesToString(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }
    }
}