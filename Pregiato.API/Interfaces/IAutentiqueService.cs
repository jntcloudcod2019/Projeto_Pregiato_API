using Pregiato.API.Models;
using Pregiato.API.Services.ServiceModels;

namespace Pregiato.API.Interfaces
{
    public interface IAutentiqueService
    {
        Task<DocumentsAutentique> ProcessDocumentAutentiqueAsync(Guid idContract);
        Task<Task> ProcessDeleteContractAsync(DocumentsAutentique documentsAutentique);
    }
}
