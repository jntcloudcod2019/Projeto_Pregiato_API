using Pregiato.API.Models;

namespace Pregiato.API.Interfaces
{
    public interface IProcessWhatsApp
    {
        Task<Task> ProcessWhatsAppAsync(Model model, string userName, string password);
    }
}
