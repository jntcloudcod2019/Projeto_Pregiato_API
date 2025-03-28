using System.ComponentModel.DataAnnotations;
using Pregiato.API.Enums;

namespace Pregiato.API.Requests
{
    public class CreateProducersRequest
    {
        [Required]
        public Guid IdProducer { get; set; }

        [Required]
        public string NameProducer { get; set; }

        [Required]
        public Guid IdContract { get; set; }

        [Required]
        public decimal AmountContract { get; set; }

        public DetailsInfoRequest? InfoModel { get; set; }

        [Required]
        public StatusContratc StatusContratc { get; set; }

        public string? ValidityContract { get; set; }

        [Required]
        public int CodProposal { get; set; }

        public int TotalAgreements { get; set; } = 0;

    }

    public class DetailsInfoRequest
    {
        public string? NameModel { get; set; }
        public Guid IdModel { get; set; }
        public string? DocumentModel { get; set; }

    }
}
