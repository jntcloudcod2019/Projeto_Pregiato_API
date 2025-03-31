using System.ComponentModel.DataAnnotations;

namespace Pregiato.API.Models
{
    public class ModelsBilling
    {
        [Key]

        public Guid Id { get; set; }
        public Guid JobId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        public Job Job { get; set; }

        [Required]
        public DateTime BillingDate { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


        [Required]
        public Guid IdModel { get; set; }

        public Model Model { get; set; }


    }
}
