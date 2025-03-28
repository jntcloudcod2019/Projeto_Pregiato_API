using Pregiato.API.Interfaces;
using PuppeteerSharp;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pregiato.API.Services
{
    public class BrowserService : IBrowserService, IAsyncDisposable
    {
        private static IBrowser? _browser;
        private static readonly SemaphoreSlim _semaphore = new(1, 1);
        private const string BrowserCachePath = "Files/.local-browser";
        private static BrowserFetcher? _browserFetcher;
        private const int MaxRetries = 3;
        private const int TimeoutSeconds = 60;

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
                while (retryCount < MaxRetries)
                {
                    try
                    {
                        if (_browser == null || !_browser.IsConnected)
                        {
                            // Validação de download existente
                            var revisionInfo = await _browserFetcher.GetRevisionInfoAsync();

                            if (!revisionInfo.Local)
                            {
                                // Download com cache
                                await _browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
                                revisionInfo = await _browserFetcher.GetRevisionInfoAsync();
                            }

                            // Timeout de conexão
                            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(TimeoutSeconds));

                            try
                            {
                                // Inicia o navegador
                                _browser = await Puppeteer.LaunchAsync(new LaunchOptions
                                {
                                    Headless = true,
                                    ExecutablePath = revisionInfo.ExecutablePath,
                                    Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
                                });

                                // Configuração de evento de desconexão
                                _browser.Disconnected += (_, _) => CleanupBrowser();

                                return _browser;
                            }
                            catch (OperationCanceledException)
                            {
                                // Timeout ocorreu
                                CleanupBrowser();
                                retryCount++;
                                if (retryCount >= MaxRetries)
                                    throw new TimeoutException($"Falha após {MaxRetries} tentativas de conexão.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        CleanupBrowser();
                        throw new ApplicationException("Falha na inicialização do navegador", ex);
                    }
                }
                throw new TimeoutException($"Falha após {MaxRetries} tentativas de conexão");
            }
            finally
            {
                _semaphore.Release();
            }
        }
        private void CleanupBrowser()
        {
            _browser?.Dispose();
            _browser = null;
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