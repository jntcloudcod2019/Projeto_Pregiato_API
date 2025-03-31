using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pregiato.API.Enums;

namespace Pregiato.API.Models
{
    public class Producers
    {

        [Required]
        public Guid ContractId { get; set; }

        public ContractBase Contract { get; set; }  
        [Required]
        public string? CodProducers { get; set; }

        [Required]
        public string? NameProducer { get; set; }

        [Required]
        public decimal AmountContract { get; set; }

        [Column(TypeName = "jsonb")]
        public DetailsInfo? InfoModel { get; set; }

        [Required]
        public StatusContratc StatusContratc { get; set; }

        [Required]
        public string? CreatedAt { get; set; } = DateTime.UtcNow.ToString("dd/MM/yyyy");

        [Required]
        public string? UpdatedAt { get; set; } = DateTime.UtcNow.ToString("dd/MM/yyyy");

        public string? ValidityContract { get; set; } = DateTime.UtcNow.ToString("dd/MM/yyyy");
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


