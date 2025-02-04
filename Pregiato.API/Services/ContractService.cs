using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.EntityFrameworkCore;
using Pregiato.API.Data;
using Pregiato.API.Interface;
using Pregiato.API.Models;
using Pregiato.API.Requests;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IJwtService _jwtService;   

        public ContractService(IContractRepository contractRepository,
               DigitalSignatureService digitalSignatureService,
               IModelRepository modelRepository,
               ModelAgencyContext context,
               IJwtService jwtService)
        {
            _contractRepository = contractRepository ?? throw new ArgumentNullException(nameof(context));
            _digitalSignatureService = digitalSignatureService;
            _modelRepository = modelRepository;
            _modelAgencyContext = context ?? throw new ArgumentNullException(nameof(context));
            _jwtService = jwtService;   
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

        public byte[] ConvertHtmlToPdf(string html)
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

        public async Task SaveContractAsync(ContractBase contract, Stream pdfStream, string cpfModelo)
        {
            using var memoryStream = new MemoryStream();
            await pdfStream.CopyToAsync(memoryStream);
            var pdfBytes = memoryStream.ToArray();

            contract.Content = pdfBytes;
        

            contract.ContractFilePath = $"CodigoPorposta_{contract.CodProposta}_CPF:{cpfModelo}_{contract.TemplateFileName}.pdf";

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

        public async Task<ContractBase> GenerateContractAsync(PaymentRequest paymentRequest, Guid modelId, string contractType, Dictionary<string, string> parameters)
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
        
            await SaveContractAsync(contract, new MemoryStream(pdfBytes), parameters["CPF-Modelo"]);

            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                ContractId = contract.ContractId,
                Valor = contract.ValorContrato,
                QuantidadeParcela = paymentRequest.QuantidadeParcela,
                FinalCartao = paymentRequest.FinalCartao,
                DataPagamento = ConvertToUtcDateTime(paymentRequest.DataPagamento.Value),
                MetodoPagamento = paymentRequest.MetodoPagamento,
                StatusPagamento = paymentRequest.StatusPagamento,
                Comprovante = paymentRequest.Comprovante,
                DataAcordoPagamento = ConvertToUtcDateTime(paymentRequest.DataAcordoPagamento.Value)
            };

            await _modelAgencyContext.AddAsync(payment);    

            return contract;
        }

        public async Task<List<ContractBase>> GenerateAllContractsAsync(PaymentRequest paymentRequest, string? idModel = null, string? cpf = null, string? rg = null)
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
                    {"Valor-Contrato",paymentRequest.Valor.ToString("C")},
                    {"Forma-Pagamento", paymentRequest.MetodoPagamento}
            };

            var contracts = new List<ContractBase>
            {
                await GenerateContractAsync(paymentRequest, model.IdModel, "Agency", parameters),
                await GenerateContractAsync(paymentRequest, model.IdModel,  "Photography", parameters),
                await GenerateContractAsync( paymentRequest, model.IdModel,  "ImageRights", parameters)
            };

            return contracts;
        }

       public async Task<ContractBase>GenerateContractCommitmentTerm(CreateRequestContractImageRights createRequestContractImageRights, string querymodel)
        {
            var model = await _modelRepository.GetModelByCriteriaAsync(querymodel);

            if (model == null)
            {
                //Inseri retorno
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
                    { "Valor-Cache", createRequestContractImageRights.valorChache.ToString("C")}
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

            string htmlTemplatePath = $"TemplatesContratos/{contract.TemplateFileName}";
            if (!File.Exists(htmlTemplatePath))
            {
                throw new FileNotFoundException($"Template não encontrado: {htmlTemplatePath}");
            }

            string htmlTemplate = await File.ReadAllTextAsync(htmlTemplatePath);

            string populatedHtml = PopulateTemplate(htmlTemplate, parameters);

            byte[] pdfBytes = ConvertHtmlToPdf(populatedHtml);

            await SaveContractAsync(contract, new MemoryStream(pdfBytes), model.CPF);
            
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


        private DateTime ConvertToUtcDateTime(DateTime dateTime)
        {
            if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue)
            {
                throw new ArgumentException("A data fornecida é inválida.");
            }

            return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        }

        public async Task<List<ContractsModels>> GetContractsByModelIdAsync(Guid modelId)
        {
            return await _modelAgencyContext.Contracts
           .Where(c => c.ModelId == modelId)
           .Select(c => new  ContractsModels
           {
               ModelId = modelId,   
               ContractFilePath = c.ContractFilePath,   
               Content = c.Content
           })
           .ToListAsync();
        }
    }  




}
