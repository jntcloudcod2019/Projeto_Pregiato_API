using Pregiato.API.Enums;
using Pregiato.API.Models;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Pregiato.API.Requests
{
    public class PaymentRequest 
    {
        [Required]  
        public MetodoPagamento MetodoPagamento { get; set; }

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

        [DefaultValue("05-02-2025")]
        [SwaggerSchema("Data acordo Pagamento")]
        [DisplayFormat(DataFormatString = "{dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DataAcordoPagamento { get; set; } = null;

        [JsonIgnore]
        public new Guid ContractId { get; set; }
        public ProviderEnum? Provider { get; set; }
        public string? AutorizationNumber { get; set; }
    }
}
