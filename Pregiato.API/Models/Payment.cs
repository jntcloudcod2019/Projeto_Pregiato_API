using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Pregiato.API.Enums;
using Pregiato.API.Services.ServiceModels;
using Swashbuckle.AspNetCore.Annotations;

namespace Pregiato.API.Models
{
    public class Payment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid PaymentId { get; set; }

        // Se desejar, você pode manter a propriedade ContractId como dado sem relacionamento
        public Guid? ContractId { get; set; }  // agora é apenas uma propriedade, sem FK

        public decimal Valor { get; set; }
        public int? QuantidadeParcela { get; set; }
        public string? FinalCartao { get; set; }

        [DefaultValue("05-02-2025")]
        [SwaggerSchema("Data Pagamento")]
        [JsonConverter(typeof(JsonDateTimeConverter))]
        [DisplayFormat(DataFormatString = "{dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DataPagamento { get; set; }

        public StatusPagamento StatusPagamento { get; set; }
        public byte[]? Comprovante { get; set; }

        [DefaultValue("05-02-2025")]
        [SwaggerSchema("Data Pagamento")]
        [JsonConverter(typeof(JsonDateTimeConverter))]
        [DisplayFormat(DataFormatString = "{dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DataAcordoPagamento { get; set; }

        public string MetodoPagamento { get; set; } = string.Empty;

        [Column(TypeName = "text")]
        public ProviderEnum? Provider { get; set; }

        public string? AutorizationNumber { get; set; }

        [MaxLength(10)]
        [Required]
        public string? CodProducers { get; set; }
    }
}