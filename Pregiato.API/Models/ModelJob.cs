using System.ComponentModel.DataAnnotations;

namespace Pregiato.API.Models
{
    public class ModelJob
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public Guid ModelId{ get; set; }

        [Required]
        public Guid JobId {get; set;}

        [Required]
        public DateTime JobDate { get; set; }

        [Required]
        [MaxLength(255)]
        public string? Location { get; set; }

        [Required]
        [MaxLength(50)]
        public string Time { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? AdditionalDescription { get; set; }
        public bool IsPresent { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    }
}
