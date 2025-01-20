namespace Pregiato.API.Models
{
    public abstract class ContractBase
    {
        public Guid ContractId { get; set; }
        public Guid ModelId { get; set; }
        public Guid JobId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string ContractFilePath { get; set; }
        public abstract string TemplateFileName { get; }
    }

    public class AgencyContract : ContractBase
    {
        public override string TemplateFileName => "AgencyContract.html";
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
}
