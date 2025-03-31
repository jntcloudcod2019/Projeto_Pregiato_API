using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http.HttpResults;
using Pregiato.API.Enums;

namespace Pregiato.API.Models
{
    public class Producers
    {

        public Guid ContractId { get; set; }

        public string? CodProducers { get; set; }

        public string? NameProducer { get; set; }

        [Required]
        public decimal AmountContract { get; set; }

        [Column(TypeName = "jsonb")]
        public DetailsInfo? InfoModel { get; set; }

        [Required]
        public StatusContratc StatusContratc { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ValidityContract { get; set; } = DateTime.UtcNow;
        public int CodProposal { get; set; }
        [Required]
        public int TotalAgreements { get; set; }

    }

    [NotMapped]
    public class DetailsInfo
    {
        public string? NameModel { get; set; }
        public Guid? IdModel { get; set; }
        public string? DocumentModel { get; set; }
    }
}


