using System.ComponentModel.DataAnnotations;

namespace Pregiato.API.Models
{
    public class ModelJob
    {
        [Key]
        public Guid Id { get; set; }

        public Guid ModelId{ get; set; }

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

    }
}
