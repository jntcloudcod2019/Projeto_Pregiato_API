using Pregiato.API.Data;
using Pregiato.API.Models;

namespace Pregiato.API.Interface
{
    public interface IContractService
    {
        string GenerateContractPdf(ContractDto dto);
    }
}
