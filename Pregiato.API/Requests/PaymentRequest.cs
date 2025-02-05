using Pregiato.API.Models;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Pregiato.API.Requests
{
    public class PaymentRequest : Payment
    {
        [Required]  
        public string MetodoPagamento { get; set; }
        [Required]
        public decimal Valor { get; set; }
        public int? QuantidadeParcela { get; set; }
        public string? FinalCartao { get; set; }

        [DefaultValue("05-02-2025")]
        [DisplayFormat(DataFormatString = "{dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        [SwaggerSchema("Data Pagamento")]
        public DateTime? DataPagamento { get; set; }

        [Required]  
        public string StatusPagamento { get; set; }

        public byte[]? Comprovante { get; set; } = new byte[] { 0x50, 0x45, 0x4E, 0x44, 0x49, 0x4E, 0x47 };

        [DefaultValue("05-02-2025")]
        [SwaggerSchema("Data acordo Pagamento")]
        [DisplayFormat(DataFormatString = "{dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DataAcordoPagamento { get; set; } = null;

        [JsonIgnore]
        public new Guid Id { get; set; }
        [JsonIgnore]
        public new Guid ContractId { get; set; }
    }
}
