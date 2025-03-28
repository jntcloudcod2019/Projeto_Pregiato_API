using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pregiato.API.Models
{
    public class ModelJob
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        // FK para Model
        [Required]
        public Guid IdModel { get; set; }
        [ForeignKey(nameof(IdModel))]
        public Model? Model { get; set; }

        // FK para Job
        [Required]
        public Guid IdJob { get; set; }
        [ForeignKey(nameof(IdJob))]
        public Job? Job { get; set; }

        [Required]
        public DateTime JobDate { get; set; }

        [MaxLength(255)]
        public string? Location { get; set; }

        [Required]
        [MaxLength(50)]
        public string Time { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? AdditionalDescription { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
