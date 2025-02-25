using Microsoft.AspNetCore.Mvc;
using Pregiato.API.Interface;

namespace Pregiato.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : Controller
    {
        [HttpPost("send-email")]
        public async Task<IActionResult> EnviarEmail([FromServices] IEmailService emailService)
        {
            await emailService.SendEmailAsync("jonathanfrnnd3@gmail.com", "Teste de E-mail Interno", "<h1>Funcionou!</h1><p>Este e-mail foi enviado para o servidor SMTP interno.</p>");
            return Ok("E-mail enviado com sucesso!");
        }
    }
}
