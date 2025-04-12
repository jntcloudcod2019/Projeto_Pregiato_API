using Pregiato.API.Enums;
using Pregiato.API.Services.ServiceModels;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Pregiato.API.System.Text.Json;
using Pregiato.API.System.Text.Json.Serialization;

namespace Pregiato.API.Requests
{
    public class PaymentRequest
    {
        [Required]
        public MetodoPagamento MetodoPagamento { get; set; }

        [Required]
        [JsonConverter(typeof(DecimalJsonConverter))]
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

        [JsonPropertyName("PROOFPIX")]
        [JsonConverter(typeof(JsonByteArrayConverter))]
        public byte[]? ProofPix { get; set; }

        public ProviderEnum? Provider { get; set; }
        public string? AutorizationNumber { get; set; }

      
    }

}
