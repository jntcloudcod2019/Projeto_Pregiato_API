using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using iText.Kernel.Pdf.Canvas.Parser.ClipperLib;
using Pregiato.API.Enums;

namespace Pregiato.API.Models
{
    public class Producers
    {
        [Key] 
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid IdProducer { get; set; }

        [Required]
        [ForeignKey("ContractId")]
        public Guid IdContract { get; set; }

        [MaxLength(10)]
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

        [Required]
        public int CodProposal { get; set; }

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


