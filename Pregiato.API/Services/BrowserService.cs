using Pregiato.API.Interfaces;
using PuppeteerSharp;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pregiato.API.Services
{
    public class BrowserService : IBrowserService, IAsyncDisposable
    {
        private static IBrowser? _browser;
        private static readonly SemaphoreSlim _semaphore = new(1, 1);
        private const string BrowserCachePath = "Files/.local-browser";  // Padrão de diretório de cache de navegador
        private const int MaxRetries = 3;
        private const int TimeoutSeconds = 60;
        private static BrowserFetcher? _browserFetcher;

        // Cache em memória
        private static MemoryStream? _browserCacheMemoryStream;

        public BrowserService()
        {
            // Inicializar o BrowserFetcher no construtor
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), BrowserCachePath);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }

            _browserFetcher = new BrowserFetcher(new BrowserFetcherOptions
            {
                Path = fullPath
            });
        }

        public async Task<IBrowser> GetBrowserAsync()
        {
            if (_browser != null && _browser.IsConnected)
                return _browser;

            await _semaphore.WaitAsync();
            try
            {
                int retryCount = 0;
                RevisionInfo revisionInfo = null;

                while (retryCount < MaxRetries)
                {
                    try
                    {
                        if (_browser == null || !_browser.IsConnected)
                        {
                            // Verifique se o navegador está em cache de memória
                            if (_browserCacheMemoryStream == null)
                            {
                                // Obter informações sobre a versão do navegador
                                revisionInfo = await _browserFetcher.GetRevisionInfoAsync();

                                // Verificar se o navegador já está no cache local (diretório) ou se deve ser baixado
                                if (revisionInfo.Local)
                                {
                                    // O navegador está disponível no diretório local, continue com o fluxo
                                    // Não faz download se já estiver local
                                    return await LaunchBrowserAsync(revisionInfo.ExecutablePath);
                                }
                                else
                                {
                                    // Baixar o Chromium no cache de memória (primeiro passo)
                                    await _browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
                                    revisionInfo = await _browserFetcher.GetRevisionInfoAsync();
                                }
                            }

                            // Se o navegador não foi encontrado no cache de memória, verifique o diretório local
                            if (_browserCacheMemoryStream == null && !revisionInfo.Local)
                            {
                                // Baixar o Chromium para o diretório local (fallback)
                                await _browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
                                revisionInfo = await _browserFetcher.GetRevisionInfoAsync();
                            }

                            // Se o arquivo não foi encontrado nem no cache, nem no diretório local, baixe-o
                            if (!revisionInfo.Local)
                            {
                                throw new InvalidOperationException("Não foi possível encontrar o navegador no cache ou no disco.");
                            }

                            // Tenta carregar o navegador da memória se possível
                            string executablePathToUse = _browserCacheMemoryStream != null
                                ? await GetExecutableFromMemoryStream()  // Recuperar da memória
                                : revisionInfo?.ExecutablePath;         // Ou do disco, se não estiver na memória

                            if (string.IsNullOrEmpty(executablePathToUse))
                            {
                                throw new InvalidOperationException("O caminho do executável do navegador não foi encontrado.");
                            }

                            // Timeout de conexão
                            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(TimeoutSeconds));

                            return await LaunchBrowserAsync(executablePathToUse);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Captura de falhas gerais
                        CleanupBrowser();
                        throw new ApplicationException("Falha ao processar a inicialização do navegador", ex);
                    }
                }

                throw new TimeoutException($"Falha após {MaxRetries} tentativas de conexão.");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task<IBrowser> LaunchBrowserAsync(string executablePath)
        {
            // Iniciar o navegador
            _browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                ExecutablePath = executablePath,
                Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
            });

            // Configuração de evento de desconexão
            _browser.Disconnected += (_, _) => CleanupBrowser();

            return _browser;
        }

        private async Task<string> GetExecutableFromMemoryStream()
        {
            if (_browserCacheMemoryStream == null)
                throw new InvalidOperationException("O navegador não está disponível na memória.");

            // Crie um arquivo temporário no sistema de arquivos e copie o conteúdo da memória para esse arquivo
            var tempFilePath = Path.GetTempFileName();
            await using (var tempFileStream = new FileStream(tempFilePath, FileMode.Create))
            {
                _browserCacheMemoryStream.Seek(0, SeekOrigin.Begin);
                await _browserCacheMemoryStream.CopyToAsync(tempFileStream);
            }

            return tempFilePath;
        }

        private void CleanupBrowser()
        {
            _browser?.Dispose();
            _browser = null;
            _browserCacheMemoryStream?.Dispose();
            _browserCacheMemoryStream = null;
        }

        public async ValueTask DisposeAsync()
        {
            if (_browser != null)
            {
                await _browser.CloseAsync();
                CleanupBrowser();
            }
            _semaphore.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
