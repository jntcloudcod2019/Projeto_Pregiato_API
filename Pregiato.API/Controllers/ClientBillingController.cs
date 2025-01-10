using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pregiato.API.Interface;
using Pregiato.API.Models;
using Pregiato.API.Requests;
using Swashbuckle.AspNetCore.Annotations;

namespace Pregiato.API.Controllers
{
    [Authorize]
    [ApiController]
    public class ClientBillingController : ControllerBase
    {
        private readonly IClientBillingRepository _clientBillingRepository;
        private readonly IClientRepository _clientRepository;

        public ClientBillingController(IClientBillingRepository clientBillingRepository, IClientRepository clientRepository)
        {
            _clientBillingRepository = clientBillingRepository;
            _clientRepository = clientRepository;
        }

        [HttpPost("/AddClientBilling")]
        [SwaggerOperation("Adiciona o faturamento por cliente.")]
        public async Task<IActionResult> AddClientBilling(Guid id, [FromBody] ClientBillingRequest createClientBillingRequest)
        {
            if (id == Guid.Empty)
            {return BadRequest();}

            var clientExists = await _clientRepository.GetByClientIdAsync(id);

            if (clientExists != null && clientExists.IdClient == id)
            {
                var clientBilling = new ClientBilling
                {
                    ClientId = clientExists.IdClient,
                    BillingId = new Guid(),
                    Amount = createClientBillingRequest.Amount,
                    BillingDate = createClientBillingRequest.BillingDate
                };
                await _clientBillingRepository.AddClientBillingAsync(clientBilling);
                return Ok();
            }
            return BadRequest();

        }

        [HttpPut("/UpdateClientBilling")]
        [SwaggerOperation("Atualiza o faturamento por cliente.")]
        public async Task<IActionResult> UpdateClientBilling(Guid id, [FromBody] ClientBillingRequest updateclientBillingRequest)
        {
            if (id == Guid.Empty)
            {return BadRequest();}

            var clientExists = await _clientRepository.GetByClientIdAsync(id);
            if (clientExists != null && clientExists.IdClient == id)
            {
                var clientBilling = new ClientBilling
                {
                    Amount = updateclientBillingRequest.Amount,
                    BillingDate = updateclientBillingRequest.BillingDate
                }; await _clientBillingRepository.AddClientBillingAsync(clientBilling);
                return Ok();
            }
            return BadRequest();
        }

        [HttpGet("/GetClientBillingID{id}")]
        [SwaggerOperation("Retorna faturamento do cliente por id")]
        public async Task <IActionResult> GetClientBillingForId (Guid id)
        {
            if (id == Guid.Empty)
            {return BadRequest();}

            var clientExists = await _clientRepository.GetByClientIdAsync(id);
            if (clientExists != null && clientExists.IdClient == id)
            {
                await _clientBillingRepository.GetByIdClientBillingAsync(id);
                return Ok();
            }
            return BadRequest();

        }

        [HttpGet("/GetClientBillingAll")]
        [SwaggerOperation("Retorna faturamento do cliente por id")]
        public async Task<IActionResult> GetClientBillingAll()
        {
            var clientbilling = await _clientBillingRepository.GetAllClientBillingAsync();
            return Ok(clientbilling);
        }
    }
}
