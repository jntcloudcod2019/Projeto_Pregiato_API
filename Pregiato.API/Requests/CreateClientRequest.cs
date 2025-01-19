using System.ComponentModel.DataAnnotations;

namespace Pregiato.API.Requests
{
    public class CreateClientRequest
    {
        [Required]
        [StringLength(255, ErrorMessage = "O nome deve ter no máximo 255 caracteres.")]
        public string Name { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Email inválido.")]
        public string Email { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "O contato deve ter no máximo 20 caracteres.")]
        public string Contact { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "O documento deve ter no máximo 50 caracteres.")]
        public string ClientDocument { get; set; }
    }
}
