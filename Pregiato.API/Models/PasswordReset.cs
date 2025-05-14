using System.ComponentModel.DataAnnotations;

namespace Pregiato.API.Models
{
    public record class PasswordReset
    {
        [Key]
        public Guid ID { get; set; } = Guid.NewGuid();
        [Required]
        public string WhatsApp { get; set; } = null!;
        public string VerificationCode { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public bool Used { get; set; } = false;

    }
}
