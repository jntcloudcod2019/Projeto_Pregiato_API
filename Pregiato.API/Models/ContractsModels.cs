namespace Pregiato.API.Models
{
    public class ContractsModels
    {
        public Guid ContractId { get; set; }
        public Guid ModelId { get; set; } 
        public string ContractFile { get; set; } 
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
