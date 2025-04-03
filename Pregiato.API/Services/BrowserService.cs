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
        private const string BrowserCachePath = "Files/.local-browser"; 
        private const int MaxRetries = 3;
        private const int TimeoutSeconds = 60;
        private static BrowserFetcher? _browserFetcher;

        private static MemoryStream? _browserCacheMemoryStream;

        public BrowserService()
        {
           
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
                            
                            if (_browserCacheMemoryStream == null)
                            {
                               
                                revisionInfo = await _browserFetcher.GetRevisionInfoAsync();

                                
                                if (revisionInfo.Local)
                                {
                                    return await LaunchBrowserAsync(revisionInfo.ExecutablePath);
                                }
                                else
                                {
                                   
                                    await _browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
                                    revisionInfo = await _browserFetcher.GetRevisionInfoAsync();
                                }
                            }

                            if (_browserCacheMemoryStream == null && !revisionInfo.Local)
                            {
                                
                                await _browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
                                revisionInfo = await _browserFetcher.GetRevisionInfoAsync();
                            }

                            if (!revisionInfo.Local)
                            {
                                throw new InvalidOperationException("Não foi possível encontrar o navegador no cache ou no disco.");
                            }

                           
                            string executablePathToUse = _browserCacheMemoryStream != null
                                ? await GetExecutableFromMemoryStream() 
                                : revisionInfo?.ExecutablePath;         

                            if (string.IsNullOrEmpty(executablePathToUse))
                            {
                                throw new InvalidOperationException("O caminho do executável do navegador não foi encontrado.");
                            }

                            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(TimeoutSeconds));

                            return await LaunchBrowserAsync(executablePathToUse);
                        }
                    }
                    catch (Exception ex)
                    {
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

            _browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                ExecutablePath = executablePath,
                Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
            });

            _browser.Disconnected += (_, _) => CleanupBrowser();

            return _browser;
        }

        private async Task<string> GetExecutableFromMemoryStream()
        {
            if (_browserCacheMemoryStream == null)
                throw new InvalidOperationException("O navegador não está disponível na memória.");

            var tempFilePath = Path.GetTempFileName();
            await using (var tempFileStream = new FileStream(tempFilePath, FileMode.Create))
            {
                _browserCacheMemoryStream.Seek(0, SeekOrigin.Begin);
                await _browserCacheMemoryStream.CopyToAsync(tempFileStream).ConfigureAwait(true);
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
