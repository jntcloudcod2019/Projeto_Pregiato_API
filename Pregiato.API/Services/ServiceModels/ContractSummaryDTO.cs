namespace Pregiato.API.Services.ServiceModels
{
   public class ContractSummaryDTO
   {
        public Guid ContractId { get; set; }
        public Guid ModelId { get; set; }
        public string DataContrato { get; set; } // Alterado de DateTime? para string
        public string VigenciaContrato { get; set; } // Alterado de DateTime? para string
        public decimal ValorContrato { get; set; }
        public string? FormaPagamento { get; set; }
        public string? StatusPagamento { get; set; }
        public string? ContractFilePath { get; set; }
        public int CodProposta { get; set; }
    }
}
