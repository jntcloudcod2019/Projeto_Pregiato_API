using System.ComponentModel.DataAnnotations;

namespace Pregiato.API.Requests
{
    public record GenerateContractsMinorityRequest
    {
        public required string NameProducers { get; set; }
        public required string NameResponsible { get; set; }
        public required string CPFResponsible { get; set; }
        public required string ModelIdentification { get; set; }
        public required string UFContract { get; set; }
        public required string City { get; set; }
        public required int Day { get; set; }
        public required string Month { get; set; }
        public required int MonthContract { get; set; }
        [Required]
        public PaymentRequest? Payment { get; set; }
    }
}
