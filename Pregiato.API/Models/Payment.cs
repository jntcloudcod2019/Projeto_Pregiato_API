

namespace Pregiato.API.Models
{
    public class Payment
    {
        public Guid Id { get; set; }
        public Guid ContractId { get; set; } 
        public decimal Valor { get; set; }
        public int? QuantidadeParcela { get; set; } 
        public string FinalCartao { get; set; } 
        public DateTime? DataPagamento { get; set; } = DateTime.UtcNow; 
        public string  StatusPagamento{ get; set; } 
        public byte[]? Comprovante { get; set; } 
        public DateTime? DataAcordoPagamento { get; set; }
        public string MetodoPagamento { get; set; }
    }
}
