using System.ComponentModel.DataAnnotations.Schema;

namespace Pregiato.API.DTO
{
    [NotMapped]
    public class ContractDTO 
    {
        public Guid? ContractId { get; set; }
        public Guid? ModelId { get; set; }
        public int ? ProposalCode {  get; set; }   
        public string ? ContractFilePath {  get; set; }    
        public byte [] ? Content { get; set; } 
    }
}
