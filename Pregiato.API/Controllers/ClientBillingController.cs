using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pregiato.API.Interface;
using Pregiato.API.Models;
using Pregiato.API.Requests;
using Swashbuckle.AspNetCore.Annotations;

namespace Pregiato.API.Controllers
{
    [Route("api/[controller]")]
    public class ClientBillingController : ControllerBase
    {
        private readonly IClientBillingRepository _clientBillingRepository;
        private readonly IClientRepository _clientRepository;

        public ClientBillingController(IClientBillingRepository clientBillingRepository, IClientRepository clientRepository)
        {
            _clientBillingRepository = clientBillingRepository;
            _clientRepository = clientRepository;
        }

        [Authorize(Policy = "AdministratorPolicy")]
        [Authorize(Policy = "Manager")]
        [SwaggerOperation("Adiciona o faturamento por cliente.")]
        [HttpPost]
        public async Task<IActionResult> AddClientBilling(Guid id, [FromBody] ClientBillingRequest createClientBillingRequest)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "O ID do cliente não pode ser vazio." });
            }

            var clientExists = await _clientRepository.GetByClientIdAsync(id);

            if (clientExists != null && clientExists.IdClient == id)
            {
                var clientBilling = new ClientBilling
                {
                    ClientId = clientExists.IdClient,
                    BillingId = Guid.NewGuid(),
                    Amount = (decimal)createClientBillingRequest.Amount, 
                    BillingDate = createClientBillingRequest.BillingDate ?? DateTime.UtcNow 
                };

                await _clientBillingRepository.AddClientBillingAsync(clientBilling);
                return Ok(new { message = "Faturamento adicionado com sucesso!" });
            }

            return NotFound(new { message = "Cliente não encontrado." });

        }

        [Authorize(Policy = "AdministratorPolicy")]
        [Authorize(Policy = "Manager")]
        [SwaggerOperation("Atualiza o faturamento por cliente.")]
        [HttpPost]  
        public async Task<IActionResult> UpdateClientBilling(Guid id, [FromBody] ClientBillingRequest updateclientBillingRequest)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "O ID do cliente não pode ser vazio." });
            }

            var clientExists = await _clientRepository.GetByClientIdAsync(id);
            if (clientExists == null || clientExists.IdClient != id)
            {
                return NotFound(new { message = "Cliente não encontrado." });
            }

            var clientBilling = await _clientBillingRepository.GetByIdClientBillingAsync(id);

            if (clientBilling == null)
            {
                return NotFound(new { message = "Faturamento do cliente não encontrado." });
            }

           
            clientBilling.Amount = updateclientBillingRequest.Amount ?? clientBilling.Amount; 
            clientBilling.BillingDate = updateclientBillingRequest.BillingDate ?? clientBilling.BillingDate; 

            await _clientBillingRepository.UpdateClientBillingAsync(clientBilling);

            return Ok(new { message = "Faturamento atualizado com sucesso!" });
        }

        [Authorize(Policy = "AdministratorPolicy")]
        [Authorize(Policy = "Manager")]
        [HttpGet("/GetClientBillingID{id}")]
        [SwaggerOperation("Retorna faturamento do cliente por id")]
        [HttpPost]
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

        [Authorize(Policy = "AdministratorPolicy")]
        [Authorize(Policy = "Manager")]
        [HttpGet("/GetClientBillingAll")]
        [SwaggerOperation("Retorna faturamento do cliente por id")]
        public async Task<IActionResult> GetClientBillingAll()
        {
            var clientbilling = await _clientBillingRepository.GetAllClientBillingAsync();
            return Ok(clientbilling);
        }
    }
}
