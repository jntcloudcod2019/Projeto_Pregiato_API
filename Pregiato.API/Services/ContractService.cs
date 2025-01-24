using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Pregiato.API.Data;
using Pregiato.API.Interface;
using Pregiato.API.Models;

namespace Pregiato.API.Services
{
        public class ContractService : IContractService
        {
        private readonly IContractService _contractService; 
        private readonly IContractRepository _contractRepository;
        private readonly DigitalSignatureService _digitalSignatureService;
        private readonly ModelAgencyContext _modelAgencyContext;   

        private readonly IModelRepository _modelRepository;

        public ContractService(IContractRepository contractRepository,
               DigitalSignatureService digitalSignatureService,
               IModelRepository modelRepository, 
               ModelAgencyContext context)
        {
            _contractRepository = contractRepository;
            _digitalSignatureService = digitalSignatureService;
            _modelRepository = modelRepository;
            _modelAgencyContext = context ?? throw new ArgumentNullException(nameof(context));

        }

        public async Task<ContractBase> GenerateContractAsync(Guid modelId, Guid jobId, string contractType, Dictionary<string, string> parameters)
        {
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

            // Leia o template HTML
            string htmlTemplate = await File.ReadAllTextAsync($"TemplatesContratos/{contract.TemplateFileName}");

            // Substitua os placeholders
            string populatedHtml = PopulateTemplate(htmlTemplate, parameters);

            // Converta o HTML para PDF
            byte[] pdfBytes = ConvertHtmlToPdf(populatedHtml);

            // Salve o contrato
            await SaveContractAsync(contract, new MemoryStream(pdfBytes));

            return contract;
        }

        public async Task<List<ContractBase>> GenerateAllContractsAsync(string? idModel = null, string? cpf = null, string? rg = null, Guid? jobId = null)
        {
            if (string.IsNullOrEmpty(idModel) && string.IsNullOrEmpty(cpf) && string.IsNullOrEmpty(rg))
            {
                throw new ArgumentException("Pelo menos um dos parâmetros 'idModel', 'cpf' ou 'rg' deve ser fornecido.");
            }

            // Busca o modelo no banco de dados
            var model = await _modelRepository.GetModelAllAsync( idModel,  cpf,  rg);

            if (model == null)
            {
                throw new KeyNotFoundException("Modelo não encontrado.");
            }

            // Prepara os parâmetros para os contratos
            var parameters = new Dictionary<string, string>
            {
                { "Nome-Modelo", model.Name },
                { "CPF-Modelo", model.CPF },
                { "RG-Modelo", model.RG },
                { "Endereço-Modelo", model.Address ?? "N/A" },
                { "Numero-Modelo", model.BankAccount ?? "N/A" },
                { "Bairro-Modelo", "N/A" },
                { "Cidade-Modelo", "N/A" },
                { "CEP-Modelo", model.PostalCode ?? "N/A" }
            };

            // Gera a lista de contratos
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

               var contract =  await _contractRepository.GetByIdContractAsync(contractId);
                if (contract == null)
                {
                    // throw new ContractNotFoundException(contractId); 
                    // _logger.LogError("Contrato não encontrado com ID: {contractId}", contractId);
                    return null;
                }
                throw new InvalidCastException(" Contrato não encontrado.");
              }

            private string PopulateTemplate(string template, Dictionary<string, string> parameters)
            {
                foreach (var param in parameters)
                {
                    template = template.Replace($"{{{param.Key}}}", param.Value);
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

                // Você pode usar um conversor HTML para PDF ou adicionar o HTML manualmente.
                document.Add(new Paragraph(html)); // Exemplo simples para adicionar texto.

                document.Close();
            }

            return memoryStream.ToArray();

        }

        public async Task SaveContractAsync(ContractBase contract, Stream pdfStream)
        {

            using var memoryStream = new MemoryStream();
            await pdfStream.CopyToAsync(memoryStream);
            var pdfBytes = memoryStream.ToArray();

            // Preenche o campo Content com os bytes do PDF
            contract.Content = pdfBytes;

            // Verifica se o modelo existe
            var model = await _modelAgencyContext.Models.FindAsync(contract.ModelId);
            if (model == null)
                throw new InvalidOperationException($"Modelo com ID {contract.ModelId} não encontrado.");

            // Gera o caminho do arquivo PDF
            contract.ContractFilePath = $"{contract.CodProposta}_{contract.ContractId}_{contract.TemplateFileName}_{model.Name.Replace(" ", "_")}.pdf";

            // Salva o contrato no banco de dados
            await _contractRepository.SaveContractAsync(contract);
        }

        private async Task<int> GetNextCodPropostaAsync()
        {
            var maxCodProposta = await _modelAgencyContext.Contracts.MaxAsync(c => (int?)c.CodProposta) ?? 109;
            return maxCodProposta + 1;
        }
    }
}
