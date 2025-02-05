using System.ComponentModel.DataAnnotations;

namespace Pregiato.API.Requests
{
    public class UploadPaymentReceiptRequest
    {
        public Guid PaymentId { get; set; } 

        [Required]
        public IFormFile File { get; set; } 
    }
}
