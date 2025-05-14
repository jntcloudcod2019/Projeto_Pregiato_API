using Pregiato.API.Models;

namespace Pregiato.API.Interfaces
{
    public interface IProcessWhatsApp
    {
        Task<Task> ProcessWhatsAppModelAsync(Model model, string userName, string password);
        Task<Task> ProcessWhatsAppCollaboratorAsync(User user, string userName, string password);
    }
}
