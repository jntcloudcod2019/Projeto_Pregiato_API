using System.ComponentModel.DataAnnotations;

namespace Pregiato.API.Requests
{
    public class CreateContractModelRequest
    {
        [Required]
        public string? NameProducers { get; set; }

        [Required]
        public string? ModelIdentification { get; set; }

        [Required]
        public string? UFContract { get; set; }

        [Required]
        public string? City { get; set; }

        [Required]
        public int Day { get; set; }

        [Required]
        public string? Month { get; set; }

        [Required]
        public int MonthContract { get; set; }

        [Required]
        public PaymentRequest? Payment { get; set; }
    }
}
