using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pregiato.API.Interface;
using Pregiato.API.Models;
using Pregiato.API.Requests;
using Pregiato.API.Response;
using Swashbuckle.AspNetCore.Annotations;
using System;

namespace Pregiato.API.Controllers
{
    //[Authorize(Roles = "AdministratorPolicy,ManagerPolicy")]
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
        public async Task<IActionResult> GetAllClients()
        {
            var clients = await _clientRepository.GetAllClientsAsync();
            var activeClients = clients.Where(c => c.Status).Select(c => new ClientResponse
            {
                IdClient = c.IdClient,
                Name = c.Name,
                Email = c.Email,
                Contact = c.Contact,
                ClientDocument = c.ClientDocument,
                Status = c.Status,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            }).ToList();

            return Ok(activeClients);
        }

        [SwaggerOperation("Criação de clientes.")]
        [SwaggerResponse(201, "Cliente criado com sucesso.", typeof(ClientResponse))]
        [SwaggerResponse(400, "Erro de validação.")]
        [HttpPost("/AddClients")]
        public async Task<IActionResult> AddNewClient([FromBody] CreateClientRequest createClientRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var client = new Client
            {
                IdClient = Guid.NewGuid(),
                Name = createClientRequest.Name,
                Email = createClientRequest.Email,
                Contact = createClientRequest.Contact,
                ClientDocument = createClientRequest.ClientDocument,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = true
            };

            await _clientRepository.AddClientAsync(client);

            var response = new ClientResponse
            {
                IdClient = client.IdClient,
                Name = client.Name,
                Email = client.Email,
                Contact = client.Contact,
                ClientDocument = client.ClientDocument,
                Status = client.Status,
                CreatedAt = client.CreatedAt,
                UpdatedAt = client.UpdatedAt
            };

            return CreatedAtAction(nameof(GetAllClients), new { client.IdClient }, response);
        }

        [HttpPut("/UpdateClients/{id}")]
        [SwaggerOperation("Atualizar cadastro de clientes.")]
        public async Task<IActionResult> UpdateClient(Guid id, [FromBody] UpdateClientRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var client = await _clientRepository.GetByClientIdAsync(id);

            if (client == null || !client.Status)
            {
                return NotFound(new { message = "Cliente não encontrado ou inativo." });
            }

            client.Name = request.Name;
            client.Email = request.Email;
            client.Contact = request.Contact;
            client.ClientDocument = request.ClientDocument;
            client.UpdatedAt = DateTime.UtcNow;

            await _clientRepository.UpdateClientAsync(client);

            return NoContent();
        }

        [HttpDelete("/DeleteClients/{id}")]
        [SwaggerOperation("Deletar cadastro de clientes.")]
        public async Task<IActionResult> DeleteClient(Guid id)
        {
            var client = await _clientRepository.GetByClientIdAsync(id);

            if (client == null || !client.Status)
            {
                return NotFound(new { message = "Cliente não encontrado ou já inativo." });
            }

            // Inativar cliente ao invés de deletar
            client.Status = false;
            client.UpdatedAt = DateTime.UtcNow;

            await _clientRepository.UpdateClientAsync(client);

            return NoContent();
        }
    }
}
