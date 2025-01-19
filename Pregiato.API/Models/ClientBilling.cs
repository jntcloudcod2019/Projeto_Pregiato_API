using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Pregiato.API.Models
{
    public class ClientBilling
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid BillingId { get; set; }

        [Required]
        public Guid ClientId { get; set; }

        [Required]
        [Range(0, 99999999.99, ErrorMessage = "O valor deve ser positivo e menor que 100 milhões.")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime BillingDate { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
