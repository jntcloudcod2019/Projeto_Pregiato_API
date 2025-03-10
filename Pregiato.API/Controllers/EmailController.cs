using Microsoft.AspNetCore.Mvc;
using Pregiato.API.Interface;

[ApiController]
[Route("api/email")]
public class EmailController : ControllerBase
{
    private readonly IEmailService _emailService;

    public EmailController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    //[HttpPost("enviar")]
    // public async Task<IActionResult> EnviarEmail([FromBody] EmailRequest request)
    //{
    //    if (request == null || string.IsNullOrEmpty(request.ToEmail))
    //        return BadRequest("O e-mail de destino é obrigatório.")
    //   // bool sucesso = await _emailService.SendEmailAsync(request.ToEmail, request.Subject, request.Body)
    //    if (sucesso)
    //        return Ok("E-mail enviado com sucesso.");
    //    else
    //        return StatusCode(500, "Erro ao enviar e-mail.");
    //}
}

public class EmailRequest
{
    public string ToEmail { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
}
