using Microsoft.EntityFrameworkCore.ValueGeneration;
using Pregiato.API.Enums;
using Pregiato.API.Services.ServiceModels;
using Pregiato.API.System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Pregiato.API.Models
{
    public class ContractCommitmentTerm
    {
        [Key]
        public Guid IDcontract { get; set; } = Guid.NewGuid();

        [Required]
        public Guid IDModel { get; set; }
        [Required]
        public string? NameModel { get; set; }
        [Required]
        public string? CpfModel { get; set; }

        [Required]
        public string? Mark { get; set; }

        [Required]
        [JsonConverter(typeof(JsonDateTimeConverter))]
        public DateTime? DatOfActivity { get; set; }

        [Required]
        public TimeSpan AppointmentTime { get; set; }

        [Required]
        public string? Locality { get; set; }

        [Required]
        public decimal GrossCash { get; set; }

        [Required]
        public decimal NetCacheModel { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        [Required]
        public string ContractFilePath { get; set; } = string.Empty;
        [Required]
        public byte[]? Content { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    }
}
