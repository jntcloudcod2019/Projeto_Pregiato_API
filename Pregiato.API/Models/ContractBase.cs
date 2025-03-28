using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Pregiato.API.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace Pregiato.API.Models
{
    public abstract class ContractBase
    {

        [Key]
        public Guid PaymentId { get; set; }

        [ForeignKey("Contract")]
        public Guid ContractId { get; set; }

        public ContractBase? Contract { get; set; }

        [MaxLength(10)]
        [Required]
        public string? CodProducers { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
 
        public string DataContrato { get; set; } = DateTime.UtcNow.ToString("dd/MM/yyyy");
        public string VigenciaContrato {get; set; } = DateTime.UtcNow.ToString("dd/MM/yyyy");
        public decimal ValorContrato { get; set; }
        public string? FormaPagamento { get; set; }
        public string StatusPagamento { get; set; } = "N/A";

        public string ContractFilePath { get; set; } = string.Empty;
        public byte[]? Content { get; set; }
        public abstract string TemplateFileName { get; }
        public int CodProposta { get; set; }

        [Required]
        public StatusContratc StatusContratc { get; set; } = StatusContratc.Ativo;

        [Required]
        public Guid ModelId { get; set; }

        [ForeignKey(nameof(ModelId))]
        public Model? Model { get; set; }

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
    [NotMapped]
    public class AgencyContract : ContractBase
    {
        public override string TemplateFileName => "AgencyContract.html";
    }
    [NotMapped]
    public class PhotographyProductionContract : ContractBase
    {
        public override string TemplateFileName => "PhotographyProductionContract.html";
    }
    [NotMapped]
    public class CommitmentTerm : ContractBase
    {
        public override string TemplateFileName => "CommitmentTerm.html";
    }
    [NotMapped]
    public class ImageRightsTerm : ContractBase
    {
        public override string TemplateFileName => "ImageRightsTerm.html";
    }
    [NotMapped]
    public class PhotographyProductionContractMinority : ContractBase
    {
        public override string TemplateFileName => "PhotographyProductionContractMinority.html";
    }


}
