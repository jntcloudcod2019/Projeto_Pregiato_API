using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Pregiato.API.Models
{
    public abstract class ContractBase
    {
        [Key]
        public Guid ContractId { get; set; }

        [ForeignKey("Model")]
        public Guid ModelId { get; set; }
        [JsonIgnore]
        public Model Model { get; set; }
        public Guid JobId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string Neighborhood { get; set; } 
        public string City { get; set; } 
        public string LocalContrato { get; set; } 
        public string DataContrato { get; set; } = DateTime.UtcNow.ToString("dd/MM/yyyy");
        public string MesContrato { get; set; } = DateTime.UtcNow.ToString("MMMM");
        public string NomeEmpresa { get; set; } 
        public string CNPJEmpresa { get; set; } 
        public string EnderecoEmpresa { get; set;} 
        public string NumeroEmpresa { get; set; } 
        public string ComplementoEmpresa { get; set;} 
        public string BairroEmpresa { get; set; } 
        public string CidadeEmpresa { get; set; } 
        public string CEPEmpresa { get; set;}
        public string VigenciaContrato {get; set; } = DateTime.UtcNow.ToString("dd/MM/yyyy");
        public decimal ValorContrato { get; set; } 
        public string ? FormaPagamento { get; set; }
        public string StatusPagamento { get; set; } = "N/A";

        [SwaggerSchema("Data do Pagamento")]
        [DisplayFormat(DataFormatString = "{dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public string DataAgendamento { get; set; } = "N/A"; 
        public string HorarioAgendamento { get; set; } = "N/A"; 
        public decimal? ValorCache { get; set; } 
        public string ContractFilePath { get; set; } = string.Empty;
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
