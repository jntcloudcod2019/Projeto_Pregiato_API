using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pregiato.API.Models
{
    public class ModelsBilling
    {
        [Key]
        [ForeignKey("IdModel")]

        [Required]
        [Range(0, 99999999.99, ErrorMessage = "O valor deve ser positivo e menor que 100 milhões.")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime BillingDate { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // FK para Model
        [Required]
        public Guid IdModel { get; set; }

        [ForeignKey(nameof(IdModel))]
        public Model Model { get; set; } = null!;


    }
}
