using System.ComponentModel.DataAnnotations;

namespace Pregiato.API.Models
{
    public class ModelsBilling
    {
       public Guid IdModel { get; set; }
       [Required]
       public decimal? Amout { get; set; }
       [Required]
       public DateTime? BillingDate { get; set; }

    } 
}
