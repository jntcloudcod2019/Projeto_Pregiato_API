using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Pregiato.API.Enums;
using Pregiato.API.Services.ServiceModels;
using Pregiato.API.System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace Pregiato.API.Models
{
    public class Payment
    {
        [Key]
        public Guid? PaymentId { get; set; }
        public Guid? ContractId { get; set; }  
        [Required]
        public decimal Valor { get; set; }
        public int? QuantidadeParcela { get; set; }
        public string? FinalCartao { get; set; }

        [DefaultValue("05-02-2025")]
        [SwaggerSchema("Data Pagamento")]
        [JsonConverter(typeof(JsonDateTimeConverter))]
        [DisplayFormat(DataFormatString = "{dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DataPagamento { get; set; } = DateTime.UtcNow;

        public StatusPagamento StatusPagamento { get; set; } = null!;
        public byte[]? Comprovante { get; set; }

        [DefaultValue("05-02-2025")]
        [SwaggerSchema("Data Pagamento")]
        [JsonConverter(typeof(JsonDateTimeConverter))]
        [DisplayFormat(DataFormatString = "{dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DataAcordoPagamento { get; set; } = DateTime.UtcNow;
        public string MetodoPagamento { get; set; } = string.Empty;

        [Column(TypeName = "text")]
        public ProviderEnum? Provider { get; set; }
        public string? AutorizationNumber { get; set; }
        public string? CodProducers { get; set; }
    }
}