using System.ComponentModel.DataAnnotations;

namespace Pregiato.API.Models
{
    public class Job
    {
        [Key]
        public Guid JobId { get; set; }
        public Guid IdModel { get; set; }

        [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres.")]
        public string Description { get; set; }

        [StringLength(20, ErrorMessage = "O status deve ter no máximo 20 caracteres.")]
        [RegularExpression(@"^(Confirmed|Cancel|Pending|Completed)$", ErrorMessage = "Status inválido.")]
        public string Status { get; set; } = "Pending";

        public DateTime JobDate { get; set; }

        [StringLength(255, ErrorMessage = "A localização deve ter no máximo 255 caracteres.")]
        public string Location { get; set; }

        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

      
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


    }
}
