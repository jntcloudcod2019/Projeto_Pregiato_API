using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Pregiato.API.Enums;
using Pregiato.API.System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace Pregiato.API.Requests
{
    public record CreateRequestCommitmentTerm
    {
        public string? CityContract { get; set; }
        public string? UFContract { get; set; }
        public int Day { get; set; }
        public string? Month { get; set; }
        public string? ModelIdentification { get; set; }
        public required string Mark { get; set; }
        [Required]
        [DefaultValue("05-02-2025")]
        [SwaggerSchema("DATA DA ATIVIDADE")]
        [JsonConverter(typeof(JsonDateTimeConverter))]
        [DisplayFormat(DataFormatString = "{dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DatOfActivity { get; set; }
        public TimeSpan AppointmentTime { get; set; }
        public string? Locality { get; set; }
        public decimal GrossCash { get; set; }
        public decimal NetCacheModel { get; set; }
        public PaymentMethod PaymentMethod { get; set; }

    }
}
