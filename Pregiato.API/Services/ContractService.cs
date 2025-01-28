using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Pregiato.API.Data;
using Pregiato.API.Interface;
using Pregiato.API.Models;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;

namespace Pregiato.API.Services
{
    public class ContractService : IContractService
    {
        private readonly IContractService _contractService;
        private readonly IContractRepository _contractRepository;
        private readonly IModelRepository _modelRepository;
        private readonly DigitalSignatureService _digitalSignatureService;
        private readonly ModelAgencyContext _modelAgencyContext;

        public ContractService(IContractRepository contractRepository,
               DigitalSignatureService digitalSignatureService,
               IModelRepository modelRepository,
               ModelAgencyContext context)
        {
            _contractRepository = contractRepository ?? throw new ArgumentNullException(nameof(context));
            _digitalSignatureService = digitalSignatureService;
            _modelRepository = modelRepository;
            _modelAgencyContext = context ?? throw new ArgumentNullException(nameof(context));
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
        private static readonly MetodoPagamento DefaulMetodoPagamento;
        private static readonly StatusPagamento DefaultStatusPagamento;

        public async Task<ContractBase> GenerateContractAsync(Guid modelId, Guid jobId, string contractType, Dictionary<string, string> parameters)
        {

            parameters ??= new Dictionary<string, string>();

            ContractBase contract = contractType switch
            {
                "Agency" => new AgencyContract(),
                "Photography" => new PhotographyProductionContract(),
                "Commitment" => new CommitmentTerm(),
                "ImageRights" => new ImageRightsTerm(),
                _ => throw new ArgumentException("Invalid contract type.")
            };

            contract.ModelId = modelId;
            contract.JobId = jobId;
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
            contract.VigenciaContrato = parameters.ContainsKey("Vigência-Contrato")  ? parameters["Vigência-Contrato"] : DefaultVigenciaContrato;            
            contract.NomeEmpresa = parameters.ContainsValue("Nome-Empresa") ? parameters["Nome-Empresa"] : "Pregiato management";
            contract.ValorContrato = parameters.ContainsKey("Valor-Contrato")
            ?decimal.Parse(parameters["Valor-Contrato"] .Replace("R$", "").Replace(".", "").Replace(",", ".").Trim()): throw new ArgumentException("A chave 'Valor-Contrato' é obrigatória.");
 

            string htmlTemplatePath = $"TemplatesContratos/{contract.TemplateFileName}";
            if (!File.Exists(htmlTemplatePath))
            {
                throw new FileNotFoundException($"Template não encontrado: {htmlTemplatePath}");
            }

            string htmlTemplate = await File.ReadAllTextAsync(htmlTemplatePath);

            string populatedHtml = PopulateTemplate(htmlTemplate, parameters);

            byte[] pdfBytes = ConvertHtmlToPdf(populatedHtml);

            await SaveContractAsync(contract, new MemoryStream(pdfBytes));

            return contract;
        }

        public async Task<List<ContractBase>> GenerateAllContractsAsync(string? idModel = null, string? cpf = null, string? rg = null, Guid? jobId = null, Dictionary<string, string>? paramet = null)
        {
            if (string.IsNullOrEmpty(idModel) && string.IsNullOrEmpty(cpf) && string.IsNullOrEmpty(rg))
            {
                throw new ArgumentException("Pelo menos um dos parâmetros 'idModel', 'cpf' ou 'rg' deve ser fornecido.");
            }

            var model = await _modelRepository.GetModelAllAsync(idModel, cpf, rg);

            if (model == null)
            {
                throw new KeyNotFoundException("Modelo não encontrado.");
            }

            string? valorContrato = null;
            string? formaPagamento = null;

            if (paramet != null)
            {
                paramet.TryGetValue("Valor-Contrato", out valorContrato); // Substitua "param1" pela chave específica
                paramet.TryGetValue("Forma-Pagamento", out formaPagamento); // Substitua "param2" pela outra chave específica
            }

            var parameters = new Dictionary<string, string>
            {       {"Local-Contrato", DefaultCidadeEmpresa},
                    {"Data-Contrato", DefaultDataContrato},
                    {"Mês-Contrato", DefaultMesContrato},
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
                    {"Nome-Empresa", DefaultNomeEmpresa},
                    {"CNPJ-Empresa", DefaultCNPJEmpresa},
                    {"Endereço-Empresa", DefaultEnderecoEmpresa},
                    {"Numero-Empresa",DefaultNumeroEmpresa},
                    {"Complemento-Empresa", DefaultComplementoEmpresa},
                    {"Cidade-Empresa", DefaultCidadeEmpresa},
                    {"Bairro-Empresa", DefaultBairroEmpresa},
                    {"CEP-Empresa",DefaultCEPEmpresa},
                    {"Vigência-Contrato",DefaultVigenciaContrato},
                    {"Valor-Contrato", valorContrato},
                    {"Forma-Pagamento", DefaulMetodoPagamento.ToString()}
            };

            var contracts = new List<ContractBase>
            {
                await GenerateContractAsync(model.IdModel, jobId.Value, "Agency", parameters),
                await GenerateContractAsync(model.IdModel, jobId.Value, "Photography", parameters),
                await GenerateContractAsync(model.IdModel, jobId.Value, "Commitment", parameters),
                await GenerateContractAsync(model.IdModel, jobId.Value, "ImageRights", parameters)
            };

            return contracts;
        }

        public async Task<ContractBase?> GetContractByIdAsync(Guid contractId)
        {

            var contract = await _contractRepository.GetByIdContractAsync(contractId);
            if (contract == null)
            {

                return null;
            }
            throw new InvalidCastException(" Contrato não encontrado.");
        }

        private string PopulateTemplate(string template, Dictionary<string, string> parameters)
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
                template = template.Replace($"{{{param.Key}}}", param.Value);
            }

            var unfilledPlaceholders = Regex.Matches(template, @"\{.*?\}");
            if (unfilledPlaceholders.Count > 0)
            {
                throw new InvalidOperationException("O template ainda contém placeholders não preenchidos: " +
                                                    string.Join(", ", unfilledPlaceholders.Select(p => p.Value)));
            }

            return template; 
        }

        private byte[] ConvertHtmlToPdf(string html)
        {
            using var memoryStream = new MemoryStream();
            using (var pdfWriter = new PdfWriter(memoryStream))
            {
                using var pdfDocument = new PdfDocument(pdfWriter);
                using var document = new Document(pdfDocument);

                string plainText = ExtractPlainTextFromHtml(html);
                document.Add(new Paragraph(plainText));

                document.Close();
            }

            return memoryStream.ToArray();
        }

        private string ExtractPlainTextFromHtml(string html)
        {
            return Regex.Replace(html, "<.*?>", string.Empty);
        }

        public async Task SaveContractAsync(ContractBase contract, Stream pdfStream)
        {

            using var memoryStream = new MemoryStream();
            await pdfStream.CopyToAsync(memoryStream);
            var pdfBytes = memoryStream.ToArray();

            contract.Content = pdfBytes;

            var model = await _modelAgencyContext.Models.FindAsync(contract.ModelId);
            if (model == null)
                throw new InvalidOperationException($"Modelo com ID {contract.ModelId} não encontrado.");

            contract.ContractFilePath = $"{contract.CodProposta}_{contract.ContractId}_{contract.TemplateFileName}_{model.Name.Replace(" ", "_")}.pdf";

            await _contractRepository.SaveContractAsync(contract);
        }

        private async Task<int> GetNextCodPropostaAsync()
        {
            var maxCodProposta = await _modelAgencyContext.Contracts.MaxAsync(c => (int?)c.CodProposta) ?? 109;
            return maxCodProposta + 1;
        }

        public async Task<string> GenerateContractPdf(int? codProposta, Guid? contractId)
        {
            if (codProposta == null && contractId == null)
            {
                throw new ArgumentException("É necessário informar um Código de Proposta ou um ID de Contrato.");
            }

            var contract = await _contractRepository.GetContractByIdAsync(codProposta, contractId);
            if (contract == null)
            {
                throw new KeyNotFoundException("Contrato não encontrado.");
            }

            string contractFilePath = contract.ContractFilePath;
            if (string.IsNullOrEmpty(contractFilePath))
            {
                throw new InvalidOperationException("O caminho do arquivo do contrato está vazio.");
            }

            string templateType = contractFilePath.Split('_')[2]; 
            string templatePath = Path.Combine("TemplatesContratos", $"{templateType}.html");

            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException("Template do contrato não encontrado.", templatePath);
            }


            var model = await _modelRepository.GetByIdModelAsync(contract.ModelId);
            if (model == null)
            {
                throw new KeyNotFoundException("Modelo relacionado ao contrato não encontrado.");
            }

            string htmlTemplate = File.ReadAllText(templatePath);

            var parameters = new Dictionary<string, string>
            {       {"Local-Contrato", DefaultCidadeEmpresa},
                    {"Data-Contrato", DefaultDataContrato},
                    {"Mês-Contrato", DefaultMesContrato},
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
                    {"Nome-Empresa", DefaultNomeEmpresa},
                    {"CNPJ-Empresa", DefaultCNPJEmpresa},
                    {"Endereço-Empresa", DefaultEnderecoEmpresa},
                    {"Numero-Empresa",DefaultNumeroEmpresa},
                    {"Complemento-Empresa", DefaultComplementoEmpresa},
                    {"Bairro-Empresa", DefaultBairroEmpresa},
                    {"CEP-Empresa",DefaultCEPEmpresa},
                    {"Vigência-Contrato", DefaultVigenciaContrato}

            };

            string populatedHtml = PopulateTemplate(htmlTemplate, parameters);
       
            byte[] pdfBytes = ConvertHtmlToPdf(populatedHtml);
        
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string outputPath = Path.Combine(desktopPath, contractFilePath);

            File.WriteAllBytes(outputPath, pdfBytes);

            if (!File.Exists(outputPath))
            {
                throw new IOException("Falha ao salvar o PDF do contrato.");
            }

            return outputPath; 
        }
    }

}
