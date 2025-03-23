using Pregiato.API.Interfaces;
using PuppeteerSharp;

namespace Pregiato.API.Services
{
    public class BrowserService : IBrowserService, IAsyncDisposable
    {
        private IBrowser _browser;

        public async Task<IBrowser> GetBrowserAsync()
        {
            if (_browser == null)
            {
                await new BrowserFetcher().DownloadAsync();
                _browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true,
                    Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
                });
            }
            return _browser;
        }

        public async ValueTask DisposeAsync()
        {
            if (_browser != null)
            {
                await _browser.CloseAsync();
            }
        }
    }
}