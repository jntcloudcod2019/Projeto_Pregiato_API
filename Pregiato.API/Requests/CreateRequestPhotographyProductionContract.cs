using Pregiato.API.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace Pregiato.API.Requests
{
    public class CreateRequestPhotographyProductionContract
    {
        [SwaggerSchema("Valor do contrato")]
        public decimal AmoutContract { get; set; }

        [SwaggerSchema("Forma de pagamento")]
        public MetodoPagamento MetodoPagamento { get; set; }
    }
}
