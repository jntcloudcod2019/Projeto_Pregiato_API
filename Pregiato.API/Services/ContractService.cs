using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
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

        public ContractService(IContractRepository contractRepository, DigitalSignatureService digitalSignatureService)
        {
            _contractRepository = contractRepository;
            _digitalSignatureService = digitalSignatureService;
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

            // Preenchimento do template HTML
            string htmlTemplate = await File.ReadAllTextAsync($"TemplatesContratos/{contract.TemplateFileName}");
            string populatedHtml = PopulateTemplate(htmlTemplate, parameters);

            // Converte o HTML para PDF
            byte[] pdfBytes = ConvertHtmlToPdf(populatedHtml);

            // Salva o contrato gerado
            await SaveContractAsync(contract, new MemoryStream(pdfBytes));

            return contract;
        }


        public async Task SaveContractAsync(ContractBase contract, Stream pdfStream)
            {
                string filePath = $"Contracts/{contract.ContractId}.pdf";
                using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                await pdfStream.CopyToAsync(fileStream);

                contract.ContractFilePath = filePath;
                await _contractRepository.SaveContractAsync(contract, pdfStream);
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
                // Utilize uma biblioteca de conversão como DinkToPdf
                return new byte[0];
            }
        }
    }
