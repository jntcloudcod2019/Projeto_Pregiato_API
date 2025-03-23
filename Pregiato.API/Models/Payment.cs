using Pregiato.API.Enums;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;


namespace Pregiato.API.Models
{
    public class Payment
    {
        public Guid Id { get; set; }
        public Guid ContractId { get; set; } 
        public decimal Valor { get; set; }
        public int? QuantidadeParcela { get; set; } 
        public string FinalCartao { get; set; }

        [DefaultValue("05-02-2025")]
        [SwaggerSchema("Data Pagamento")]
        [JsonConverter(typeof(JsonDateTimeConverter))]
        [DisplayFormat(DataFormatString = "{dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DataPagamento { get; set; }
        public StatusPagamento  StatusPagamento{ get; set; } 
        public byte[]? Comprovante { get; set; }

        [DefaultValue("05-02-2025")]
        [SwaggerSchema("Data Pagamento")]
        [JsonConverter(typeof(JsonDateTimeConverter))]
        [DisplayFormat(DataFormatString = "{dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DataAcordoPagamento { get; set; }
        public string MetodoPagamento { get; set; }

        [Column(TypeName = "text")]
        public ProviderEnum? Provider { get; set; }
        public string? AutorizationNumber { get; set; }

    }
}
