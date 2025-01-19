using System.ComponentModel.DataAnnotations;

namespace Pregiato.API.Requests
{
    public class ClientBillingRequest
    {
        [Required(ErrorMessage = "O valor do faturamento é obrigatório.")]
        [Range(0, double.MaxValue, ErrorMessage = "O valor deve ser maior ou igual a zero.")]
        public decimal? Amount { get; set; }

        [Required(ErrorMessage = "A data do faturamento é obrigatória.")]
        public DateTime? BillingDate { get; set; }
    }
}
