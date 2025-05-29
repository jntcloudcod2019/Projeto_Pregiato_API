using System.ComponentModel.DataAnnotations;
using Pregiato.API.Enums;


namespace Pregiato.API.Models
{
    public abstract class ContractBase
    {

        [Key]
        public Guid ContractId { get; set; }
        public Guid? PaymentId { get; set; }

        public string? CodProducers { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        [Required]
        public DateTime DataContrato { get; set; } = DateTime.UtcNow;
        [Required]
        public DateTime VigenciaContrato {get; set; } = DateTime.UtcNow;
        [Required]
        public decimal ValorContrato { get; set; }
        [Required]
        public string? FormaPagamento { get; set; }
        [Required]
        public string StatusPagamento { get; set; } = "Paid";
        [Required]
        public string ContractFilePath { get; set; } = string.Empty;
        [Required]
        public byte[]? Content { get; set; }
        [Required]
        public abstract string TemplateFileName { get; }
        [Required]
        public int CodProposta { get; set; }
        [Required]
        public StatusContratc StatusContratc { get; set; } = StatusContratc.Ativo;
        public Guid? IdModel { get; set; }

    }


    public class AgencyContract : ContractBase
    {
        public override string TemplateFileName => "AgencyContract.html";
    }

    public class AgencyContractMinority : ContractBase
    {
        public override string TemplateFileName => "AgencyContractMinority.html";
    }

    public class PhotographyProductionContract : ContractBase
    {
        public override string TemplateFileName => "PhotographyProductionContract.html";
    }
 
    public class CommitmentTerm : ContractBase
    {
        public override string TemplateFileName => "CommitmentTerm.html";
    }

    public class ImageRightsTerm : ContractBase
    {
        public override string TemplateFileName => "ImageRightsTerm.html";
    }

    public class PhotographyProductionContractMinority : ContractBase
    {
        public override string TemplateFileName => "PhotographyProductionContractMinority.html";
    }


}
