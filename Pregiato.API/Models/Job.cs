using Microsoft.Build.Framework;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Pregiato.API.Models
{
    public class Job
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ? IdJob { get; set; }

        [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres.")]
        public string Description { get; set; }

        [StringLength(20, ErrorMessage = "O status deve ter no máximo 20 caracteres.")]
        [RegularExpression(@"^(Confirmed|Cancel|Pending|Completed)$", ErrorMessage = "Status inválido.")]
        public string Status { get; set; } = "Pending";

        [System.ComponentModel.DataAnnotations.Required]
        public DateTime JobDate { get; set; }

        [StringLength(255, ErrorMessage = "A localização deve ter no máximo 255 caracteres.")]
        public string Location { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [System.ComponentModel.DataAnnotations.Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
