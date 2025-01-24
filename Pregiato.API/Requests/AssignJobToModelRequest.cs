using System.ComponentModel.DataAnnotations;

namespace Pregiato.API.Requests
{
    public class AssignJobToModelRequest
    {
        [Required]
        public Guid ModelId { get; set; } // ID do modelo ao qual o job será atribuído

        [Required]
        public Guid JobId { get; set; } // ID do job que será atribuído ao modelo

        [Required]
        public DateTime JobDate { get; set; } // Data do job

        [Required]
        [StringLength(255, ErrorMessage = "A localização deve ter no máximo 255 caracteres.")]
        public string Location { get; set; } // Local do job

        [Required]
        [StringLength(50, ErrorMessage = "O horário deve ter no máximo 50 caracteres.")]
        public string Time { get; set; } // Horário do job

        [StringLength(500, ErrorMessage = "A descrição adicional deve ter no máximo 500 caracteres.")]
        public string? AdditionalDescription { get; set; } // Descrição adicional opcional
    }
}
