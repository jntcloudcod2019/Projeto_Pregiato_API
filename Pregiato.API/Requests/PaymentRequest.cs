using Pregiato.API.Models;

namespace Pregiato.API.Requests
{
    public class PaymentRequest
    {
        public string MetodoPagamento { get; set; }
        public decimal Valor { get; set; }
        public int? QuantidadeParcela { get; set; }
        public string? FinalCartao { get; set; }
        public DateTime? DataPagamento { get; set; }
        public string StatusPagamento { get; set; }
        public byte[]? Comprovante { get; set; }
        public DateTime? DataAcordoPagamento { get; set; }

    }

}
