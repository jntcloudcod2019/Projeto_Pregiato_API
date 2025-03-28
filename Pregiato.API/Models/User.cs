using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Pregiato.API.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid UserId { get; set; }

        public string? CodProducers { get; set; }

        [Required]
        public string? Email { get; set; }

        [Required]
        public string? Name { get; set; }

        [Required]
        public string? NickName { get; set; }

        [Required]
        public string? PasswordHash { get; set; }

        [Required]
        public string? UserType { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
