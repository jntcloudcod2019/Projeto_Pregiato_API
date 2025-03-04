using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;

namespace Pregiato.API.Requests
{
    public class CreateContractModelRequest
    {
        [Required]
        public string ModelIdentification { get; set; }

        [Required]
        public string UFContract { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public int Day { get; set; }

        [Required]
        public string Month { get; set; }

        [Required]
        public int MonthContract { get; set; }

        [Required]
        public PaymentRequest Payment { get; set; } 
    }
}
