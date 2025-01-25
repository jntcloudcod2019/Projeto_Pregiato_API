namespace Pregiato.API.Models
{
    public abstract class ContractBase
    {
        public Guid ContractId { get; set; }
        public Guid ModelId { get; set; }
        public Guid JobId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string Neighborhood { get; set; } = "N/A"; // Bairro com valor padrão
        public string City { get; set; } = "N/A";
        public string LocalContrato { get; set; } = "São Paulo";
        public string DataContrato { get; set; } = DateTime.UtcNow.ToString("dd/MM/yyyy");
        public string MesContrato { get; set; } = DateTime.UtcNow.ToString("MMMM");

        public string ContractFilePath { get; set; } = string.Empty;

        // Propriedade para conteúdo do contrato, se necessário
        public byte[] Content { get; set; }

        public abstract string TemplateFileName { get; }

        public int CodProposta { get; set; }
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
