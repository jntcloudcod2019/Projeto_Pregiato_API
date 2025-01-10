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
    [Route("api/Client")]
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
        public async Task<IActionResult> AddNewClient([FromBody] CreateClientRequest createClientRequest) 
        
        { 
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var client = new Client
            {
                IdClient = new Guid(),
                Name = createClientRequest.Name,
                Email = createClientRequest.Email,
                Contact = createClientRequest.Contact,
                ClientDocument = createClientRequest.ClientDocument                 
            };

            await _clientRepository.AddClientAsync(client);
            return CreatedAtAction(nameof(_clientRepository.GetByClientIdAsync), new { client.IdClient });
        }
       
        [HttpPut("/UpdateClients/{id}")]
        [SwaggerOperation("Atualizar cadastro de clientes.")]
        public async Task<IActionResult> UpdateClient(Guid id, [FromBody] UpdateClientRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var clientExists = await _clientRepository.GetByClientIdAsync(id);

            if (clientExists == null)
            {
                return NotFound();
            }

            clientExists.Name = request.Name;
            clientExists.ClientDocument = request.ClientDocument;
            clientExists.Contact = request.Contact;
            clientExists.Email = request.Email;

            await _clientRepository.UpdateClientAsync(clientExists);

            return NoContent();
        }

        [HttpDelete("/DeleteClients/{id}")]
        [SwaggerOperation("Deletar cadastro de clientes.")]
        public async Task<IActionResult> DeleteClient(Guid id)
        {
           await _clientRepository.DeleteClientAsync(id);

            if (id == null)
            {
                return NotFound();

            }
            await _clientRepository.DeleteClientAsync(id);

            return new EmptyResult();

        }

    }
}
