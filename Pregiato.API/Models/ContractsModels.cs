using System.ComponentModel.DataAnnotations.Schema;

namespace Pregiato.API.Models
{
    [NotMapped]
    public class ContractsModels : ContractBase
    {
        public Guid ContractId { get; set; }
        public Guid ModelId { get; set; }
        public string ContractFilePath { get; set; }
        public byte[] Content { get; set; }
        public override string TemplateFileName { get; }
    }
}
