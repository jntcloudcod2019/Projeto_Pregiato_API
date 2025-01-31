using Pregiato.API.Models;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Pregiato.API.Requests
{
    public class PaymentRequest
    {
        public string MetodoPagamento { get; set; }
        public decimal Valor { get; set; }
        public int? QuantidadeParcela { get; set; }
        public string? FinalCartao { get; set; }

        [SwaggerSchema("Data Pagamento")]
        [DisplayFormat(DataFormatString = "{dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DataPagamento { get; set; }
        public string StatusPagamento { get; set; }
        public byte[]? Comprovante { get; set; }
        [SwaggerSchema("Data Pagamento")]
        [DisplayFormat(DataFormatString = "{dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DataAcordoPagamento { get; set; } = null;

    }

}
