using PuppeteerSharp;

namespace Pregiato.API.Interfaces
{
    public interface IBrowserService
    {
        Task<IBrowser> GetBrowserAsync();
    }
}
