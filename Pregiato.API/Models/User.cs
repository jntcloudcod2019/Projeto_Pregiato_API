using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace Pregiato.API.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid UserId { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "O email deve ser válido.")]
        [StringLength(255, ErrorMessage = "O email deve ter no máximo 255 caracteres.")]
        public string ? Email { get; set; }

        [Required]
        [StringLength(255, ErrorMessage = "O nome deve ter no máximo 255 caracteres.")]
        public string ? Name { get; set; }

        [Required]
        [StringLength(255, ErrorMessage = "A senha deve ter no máximo 255 caracteres.")]
        public string ? PasswordHash { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "O tipo de usuário deve ter no máximo 50 caracteres.")]
        public string ? UserType { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
