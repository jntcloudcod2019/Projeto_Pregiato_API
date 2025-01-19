using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;

namespace Pregiato.API.Models
{
    public class Client
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid IdClient { get; set; }

        [Required]
        [StringLength(255, ErrorMessage = "O nome deve ter no máximo 255 caracteres.")]
        public string Name { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "O email deve ser válido.")]
        [StringLength(255, ErrorMessage = "O email deve ter no máximo 255 caracteres.")]
        public string Email { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "O documento deve ter no máximo 50 caracteres.")]
        public string ClientDocument { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "O contato deve ter no máximo 20 caracteres.")]
        public string Contact { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public bool Status { get; set; } = true;

    }
}
