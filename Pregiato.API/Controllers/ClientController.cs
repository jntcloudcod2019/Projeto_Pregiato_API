using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Core.Types;
using Pregiato.API.Interface;
using Pregiato.API.Models;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Pregiato.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientController : ControllerBase
    {
        private readonly IClientRepository _clientRepository;

        public ClientController(IClientRepository clientRepository)
        {
            _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
        }

        [HttpGet("/GetAllClients")]
        [SwaggerOperation("Retorna todos os clientes cadastrados.")]
        //[ProducesResponseType(typeof(ApiSuccessResponse<IdStatusResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAllClients()
        {
            var clients = await _clientRepository.GetAllClientsAsync();
            return Ok(clients);
        }

        [HttpPost("/AddClients")]
        [SwaggerOperation("Criação de clientes.")]
        public async Task<IActionResult> AddNewClient([FromBody] Client client)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

    
            await _clientRepository.AddClientAsync(client); 
           
            return CreatedAtAction(nameof(GetAllClients), new { client.ClientId, client.Name });
        }



    }
}
