using Microsoft.Extensions.Options;
using Pregiato.API.Interface;
using Pregiato.API.Services.ServiceModels;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Pregiato.API.Models;


namespace Pregiato.API.Services
{
    public class AuthenticService : IAuthenticService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenctiSettings _settings;
        private readonly IModelRepository _modelRepository;
        private readonly IContractService _contractService; 

        public AuthenticService(HttpClient httpClient, IOptions<AuthenctiSettings> settings, IModelRepository modelRepository, IContractService contractService)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _modelRepository = modelRepository;

            _httpClient.BaseAddress = new Uri(_settings.BaseUrl);

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.Token);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _contractService = contractService; 
        }

        /// <summary>
        /// Faz upload de um contrato PDF para o Autentique
        /// </summary>
        public async Task<List<ContractsModels>> UploadContractAsync(List<ContractBase> contracts, Guid modelId)
        {
            var model = await _modelRepository.GetByIdModelAsync(modelId);

            if (model == null)
            {
                throw new KeyNotFoundException("Modelo não encontrado.");
            }

            var contratosAtualizados = new List<ContractsModels>();
            var documentosIds = new List<string>();

            foreach (var contrato in contracts)
            {
                try
                {
                    // 🔹 Utilizando a conversão correta para extrair os bytes do PDF
                    string contentString = await _contractService.ConvertBytesToString(contrato.Content);
                    byte[] pdfBytes = await _contractService.ExtractBytesFromString(contentString);

                    // 🔹 Criando a estrutura GraphQL correta
                    var query = new
                    {
                        query = @"
                mutation CreateDocument($document: DocumentInput!, $signers: [SignerInput!]!) {
                    createDocument(document: $document, signers: $signers) {
                        id
                    }
                }",
                        variables = new
                        {
                            document = new
                            {
                                name = contrato.ContractFilePath,
                                file = "" // O arquivo será referenciado via `map`
                            },
                            signers = new[]
                            {
                        new { name = model.Name, email = model.Email, action = "SIGN" }
                    }
                        }
                    };

                    // 🔹 Criando `map` para associar o arquivo corretamente
                    var map = new Dictionary<string, object>
            {
                { "file1", new[] { "variables.document.file" } }
            };

                    var formData = new MultipartFormDataContent();

                    // 🔹 Adicionando o JSON da query no FormData
                    formData.Add(new StringContent(JsonSerializer.Serialize(query), Encoding.UTF8, "application/json"), "operations");

                    // 🔹 Adicionando o mapeamento do arquivo no FormData
                    formData.Add(new StringContent(JsonSerializer.Serialize(map), Encoding.UTF8, "application/json"), "map");

                    // 🔹 Adicionando o PDF corretamente no FormData
                    var pdfContent = new ByteArrayContent(pdfBytes);
                    pdfContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                    formData.Add(pdfContent, "file1", "contrato.pdf"); // 🔹 Nome do arquivo deve corresponder ao `map`

                    var response = await _httpClient.PostAsync("v2/graphql", formData);

                    var responseData = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"🔹 Resposta da API do Autentique: {responseData}");

                    using var doc = JsonDocument.Parse(responseData);

                    // 🔹 Verificando se houve erro na resposta
                    if (doc.RootElement.TryGetProperty("errors", out var errorElement))
                    {
                        throw new Exception($"Erro da API Autentique: {errorElement}");
                    }

                    if (!doc.RootElement.TryGetProperty("data", out var dataElement) ||
                        !dataElement.TryGetProperty("createDocument", out var createDocumentElement))
                    {
                        throw new Exception($"Erro: 'createDocument' não encontrado na resposta. Resposta completa: {responseData}");
                    }

                    var documentoId = createDocumentElement.GetProperty("id").GetString();
                    documentosIds.Add(documentoId);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erro no upload do contrato {contrato.ContractFilePath}: {ex.Message}");
                }
            }

            if (documentosIds.Any())
            {
                await SendContractsForSignatureAsync(documentosIds);
            }

            return contratosAtualizados;
        }


        /// <summary>
        /// Envia múltiplos contratos já cadastrados no Autentique para assinatura.
        /// </summary>
        public async Task SendContractsForSignatureAsync(List<string> documentosIds)
        {
            try
            {
                foreach (var documentoId in documentosIds)
                {
                    var query = new
                    {
                        query = @"
                        mutation SendDocument($id: ID!) {
                            sendDocument(id: $id) {
                                id
                            }
                        }",
                        variables = new { id = documentoId }
                    };

                    var content = new StringContent(JsonSerializer.Serialize(query), Encoding.UTF8, "application/json");

                    var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                    var response = await _httpClient.PostAsync("v2/graphql", content);

                    if (!response.IsSuccessStatusCode)
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        throw new Exception($"Erro ao enviar contrato {documentoId} para assinatura: {error}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao enviar contratos para assinatura: {ex.Message}");
            }
        }
    }
}
