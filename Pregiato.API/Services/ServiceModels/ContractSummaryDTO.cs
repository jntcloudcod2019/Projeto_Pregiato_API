using System.Runtime.InteropServices.JavaScript;

namespace Pregiato.API.Services.ServiceModels
{
   public class ContractSummaryDTO
   {
        public Guid ? ContractId { get; set; }
        public Guid ? ModelId { get; set; }
        public DateTime DataContrato { get; set; } 
        public DateTime VigenciaContrato { get; set; } 
        public decimal ValorContrato { get; set; }
        public string? FormaPagamento { get; set; }
        public string? StatusPagamento { get; set; }
        public string? ContractFilePath { get; set; }
        public int CodProposta { get; set; }
    }
}
