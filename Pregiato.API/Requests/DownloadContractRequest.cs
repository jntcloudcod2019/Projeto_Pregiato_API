using Swashbuckle.AspNetCore.Annotations;

namespace Pregiato.API.Requests
{

    [SwaggerSchema("Requisição para download de contrato.")]
    public class DownloadContractRequest
    {
        public Guid ? ContractId { get; set; }  
        public Guid ? ModelId { get; set; } 
        public int ? ProposalCode { get; set; }

    }
}
