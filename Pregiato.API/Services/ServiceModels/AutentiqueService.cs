using Microsoft.EntityFrameworkCore;
using Pregiato.API.Data;
using Pregiato.API.Interfaces;
using Pregiato.API.Models;
using System.Diagnostics.Contracts;

namespace Pregiato.API.Services.ServiceModels
{
    public class AutentiqueService : IAutentiqueService
    {
        private readonly IDbContextFactory<ModelAgencyContext> _contextFactory;
        private readonly IContractRepository _contractRepository;
        private readonly IRabbitMQProducer _rabbitMQProducer;
        public AutentiqueService(IDbContextFactory<ModelAgencyContext> contextFactory, IContractRepository contractRepository, IRabbitMQProducer rabbitMQProducer)
        {
            _contextFactory = contextFactory;
            _contractRepository = contractRepository;
            _rabbitMQProducer = rabbitMQProducer;
        }

        public async Task<Task> ProcessDeleteContractAsync(DocumentsAutentique documentsAutentique)
        {

            var contractId = documentsAutentique.IdContract;
            var deleteContract = _rabbitMQProducer.SendMessageDeleteContractAsync(documentsAutentique, new ContractMessage
            {
                Action = "DELETE",
                IdDocumentAutentique = documentsAutentique.IdDocumentAutentique,
                IdContract = documentsAutentique.IdContract
            });

            Console.WriteLine($"[API] MENSAGEM DE EXCLUSÃO ENVIADA PARA A FILA. DOCUMENTO AUTENTIQUE ID: {documentsAutentique.IdDocumentAutentique}");

            return Task.CompletedTask;
        }

        public async Task<DocumentsAutentique> ProcessDocumentAutentiqueAsync(Guid idContract)
         {
            using ModelAgencyContext context = _contextFactory.CreateDbContext();
             var documentAutentique = await context.DocumentsAutentique
             .AsNoTracking()
             .FirstOrDefaultAsync(c => c.IdContract == idContract);

            if (documentAutentique == null)
            {
                Console.WriteLine($"[Webhook] Documento {documentAutentique.IdContract} não encontrado no banco.");
            }


            return documentAutentique;
        }

    }
}
